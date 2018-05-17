namespace NetUtils.MemoryCache
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Utils;

    internal class MemoryCacheInstance : DisposableBase, ICacheInstance
    {
        private readonly ConcurrentDictionary<string, CacheItem> _keyDataMappings = new ConcurrentDictionary<string, CacheItem>();

        private readonly ReaderWriterLockSlim _lockForClean = new ReaderWriterLockSlim();
        private readonly ReaderWriterLockSlim _lockForLazyData = new ReaderWriterLockSlim();

        private DateTimeOffset _lastClean = DateTimeOffset.MinValue;

        public static readonly TimeSpan DefaultCacheCleanInternal = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan DefaultDataDisposeDelay = TimeSpan.FromMinutes(30);

        public MemoryCacheInstance(string name, CacheExpirePolicy cacheExpirePolicy)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            CacheExpirePolicy = cacheExpirePolicy;
        }

        public string Name { get; }

        public CacheExpirePolicy CacheExpirePolicy { get; }

        public TimeSpan CleanInternal { get; set; } = DefaultCacheCleanInternal;

        public TimeSpan DataDisposeDelay { get; set; } = DefaultDataDisposeDelay;

        public bool UseStrictThreadSafeMode { get; set; } = false;

        public int Size => _keyDataMappings.Count;

        public void CleanIfNeeded()
        {
            if (_lockForClean.TryEnterWriteLock(0))
            {
                try
                {
                    var utcNow = DateTimeOffset.UtcNow;
                    if (utcNow < _lastClean + CleanInternal)
                    {
                        // Cannot add / delete while iterating a list / dict directly
                        var keysToRemove = _keyDataMappings.Where(pair => pair.Value.GetExpireUtc(CacheExpirePolicy) < utcNow).Select(pair => pair.Key).ToList();
                        if (keysToRemove?.Count > 0)
                        {
                            foreach (var key in keysToRemove)
                            {
                                TryDeleteKey(key);
                            }
                        }
                    }

                    _lastClean = utcNow;
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.ToString());
                }
                finally
                {
                    _lockForClean.ExitWriteLock();
                }
            }
        }

        public void Clear()
        {
            var keysToRemove = _keyDataMappings.Select(pair => pair.Key).ToList();
            foreach (var key in keysToRemove)
            {
                TryDeleteKey(key);
            }
        }

        public T GetDataOrCreate<T>(string key, Func<T> constructor, TimeSpan timeout, string metadata = null, bool shouldReloadInBackground = true)
        {
            return GetLazyDataOrCreate(key, constructor, timeout, metadata, shouldReloadInBackground).Value;
        }

        public Task<T> GetDataOrCreateAsync<T>(string key, Func<Task<T>> constructor, TimeSpan timeout, string metadata = null, bool shouldReloadInBackground = true)
        {
            var lazyData = GetLazyDataOrCreateFromTask(key, constructor, timeout, metadata, shouldReloadInBackground);
            return Task.FromResult(lazyData.Value);
        }

        private Lazy<T> GetLazyDataOrCreate<T>(string key, Func<T> constructor, TimeSpan timeout, string metadata, bool shouldReloadInBackground)
        {
            if (UseStrictThreadSafeMode)
            {
                _lockForLazyData.EnterWriteLock();
            }

            try
            {
                var lazy = LazyUtils.ToLazy(constructor);
                var cacheItem = GetDataOrCreateCacheItem(key, () => lazy, timeout, metadata, shouldReloadInBackground);
                if (cacheItem != null)
                {
                    var val = cacheItem.Data as Lazy<T>;
                    if (val == null && cacheItem.Data is T)
                    {
                        val = LazyUtils.ToLazy(() => (T)cacheItem.Data);
                    }

                    return val;
                }
                else
                {
                    return LazyUtils.ToLazy(() => default(T));
                }
            }
            finally
            {
                if (UseStrictThreadSafeMode)
                {
                    _lockForLazyData.ExitWriteLock();
                }
            }
        }

        private Lazy<T> GetLazyDataOrCreateFromTask<T>(string key, Func<Task<T>> constructor, TimeSpan timeout, string metadata, bool shouldReloadInBackground)
        {
            var lazy = GetLazyDataOrCreate(key, () => constructor().ConfigureAwait(false).GetAwaiter().GetResult(), timeout, metadata, shouldReloadInBackground);
            return lazy;
        }

        private ICacheItem GetDataOrCreateCacheItem<T>(string key, Func<T> constructor, TimeSpan timeToLive, string metadata, bool shouldReloadInBackground)
        {
            CacheItem cacheItem;
            if (shouldReloadInBackground)
            {
                if (TryGetDataInner(key, false, out cacheItem))
                {
                    if (cacheItem.GetExpireUtc(CacheExpirePolicy) < DateTimeOffset.UtcNow)
                    {
                        Task.Run(() => TrySetData(key, () => constructor(), timeToLive, metadata));
                    }
                    else
                    {
                        cacheItem.LastAccessUtc = DateTimeOffset.UtcNow;
                    }

                    return cacheItem;
                }
            }
            else
            {
                if (TryGetDataInner(key, true, out cacheItem))
                {
                    return cacheItem;
                }
            }

            TrySetData(key, () => constructor(), timeToLive, metadata);
            if (TryGetDataInner(key, true, out cacheItem))
            {
                return cacheItem;
            }

            return null;
        }

        public bool TryGetData(string key, out ICacheItem cacheItem)
        {
            if (TryGetDataInner(key, true, out var data))
            {
                cacheItem = data;
                return true;
            }

            cacheItem = null;
            return false;
        }

        private bool TryGetDataInner(string key, bool shouldCheckDataExpire, out CacheItem cacheItem)
        {
            var success = false;
            cacheItem = null;

            try
            {
                success = _keyDataMappings.TryGetValue(key, out cacheItem);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
            }

            if (!success)
            {
                return false;
            }

            if (cacheItem != null
                && (!shouldCheckDataExpire || cacheItem.GetExpireUtc(CacheExpirePolicy) > DateTimeOffset.UtcNow))
            {
                if (shouldCheckDataExpire)
                {
                    cacheItem.LastAccessUtc = DateTimeOffset.UtcNow;
                }

                return true;
            }

            TryDeleteKey(key);
            return false;
        }

        public bool TryDeleteKey(string key)
        {
            try
            {
                if (_keyDataMappings.TryRemove(key, out var item))
                {
                    if (item.IsDataDisposable || DataDisposeDelay >= TimeSpan.FromMilliseconds(1))
                    {
                        Task.Run(async () =>
                        {
                            await Task.Delay(DataDisposeDelay).ConfigureAwait(false);
                            item.Dispose();
                        });
                    }
                    else
                    {
                        item.Dispose();
                    }

                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
            }

            return false;
        }

        public bool TrySetData(string key, object data, TimeSpan timeToLive, string metadata = null)
        {
            return TrySetData(key, () => data, timeToLive, metadata);
        }

        private bool TrySetData(string key, Func<object> dataConstructor, TimeSpan timeToLive, string metadata)
        {
            try
            {
                var data = dataConstructor();
                var cacheItem = _keyDataMappings.AddOrUpdate(
                    key,
                    k => new CacheItem(key, data, metadata),
                    (k, c) =>
                    {
                        c.Data = data;
                        c.MetaData = metadata;
                        return c;
                    });

                cacheItem.LastUpdateUtc = cacheItem.LastAccessUtc = DateTimeOffset.UtcNow;
                cacheItem.TimeToLive = timeToLive;

                return true;
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
            }

            return false;
        }

        protected override void DisposeResources()
        {
            foreach (var pair in _keyDataMappings)
            {
                pair.Value?.Dispose();
            }

            _keyDataMappings.Clear();

            _lockForClean?.Dispose();
            _lockForLazyData?.Dispose();
        }
    }
}
