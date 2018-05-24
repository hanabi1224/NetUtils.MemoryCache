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
        private readonly ConcurrentQueue<IDisposable> _itemsToDispose = new ConcurrentQueue<IDisposable>();

        private readonly ReaderWriterLockSlim _lockForClean = new ReaderWriterLockSlim();
        private readonly Lazy<ReaderWriterLockSlim> _lockForLazyData = LazyUtils.ToLazy(() => new ReaderWriterLockSlim());

        private DateTimeOffset _lastClean = DateTimeOffset.MinValue;

        public static readonly TimeSpan DefaultCacheCleanInternal = TimeSpan.FromMinutes(5);

        public MemoryCacheInstance(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Name { get; }

        public TimeSpan CleanInternal { get; set; } = DefaultCacheCleanInternal;

        public bool UseStrictThreadSafeModeForAutoReload { get; set; } = true;

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
                        return;
                    }

                    // Cannot add / delete while iterating a list / dict directly
                    var keysToRemove = _keyDataMappings.Where(pair => pair.Value.IsExpired).Select(pair => pair.Key).ToList();
                    if (keysToRemove?.Count > 0)
                    {
                        foreach (var key in keysToRemove)
                        {
                            TryDeleteKey(key);
                        }
                    }

                    while (_itemsToDispose.TryDequeue(out var cacheItem))
                    {
                        cacheItem.Dispose();
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

        public T GetAutoReloadDataWithCache<T>(
            string key,
            Func<T> dataFactory,
            Func<string> eTagFactory,
            TimeSpan timeToLive,
            TimeSpan dataUpdateDetectInternal,
            out string eTag,
            bool shouldReloadInBackground = true)
        {
            var cacheItem = GetAutoReloadDataWithCacheInner<T>(
                key,
                dataFactory,
                eTagFactory,
                timeToLive,
                dataUpdateDetectInternal,
                shouldReloadInBackground);

            try
            {
                var val = cacheItem.Data as Lazy<T>;
                eTag = cacheItem.ETag;
                return val.Value;
            }
            catch
            {
                if (cacheItem.PreviousCacheItem != null)
                {
                    var val = cacheItem.PreviousCacheItem.Data as Lazy<T>;
                    eTag = cacheItem.PreviousCacheItem.ETag;
                    var rollbackResult = val.Value;

                    // Rollback
                    cacheItem.Data = cacheItem.PreviousCacheItem.Data;
                    eTag = cacheItem.ETag = cacheItem.PreviousCacheItem.ETag;
                    cacheItem.PreviousCacheItem = null;

                    return rollbackResult;
                }

                throw;
            }
        }

        private CacheItem GetAutoReloadDataWithCacheInner<T>(
            string key,
            Func<T> dataFactory,
            Func<string> eTagFactory,
            TimeSpan timeToLive,
            TimeSpan dataUpdateDetectInternal,
            bool shouldReloadInBackground)
        {
            if (TryGetDataInner(key, out var cacheItem)
                && shouldReloadInBackground)
            {
                var isUpdateProbobalyNeeded = cacheItem.LastETagCheckUtc.AddSafe(dataUpdateDetectInternal) < DateTimeOffset.UtcNow;
                if (isUpdateProbobalyNeeded)
                {
                    Task.Run(() =>
                    {
                        SetOrUpdateLazyData(key, cacheItem, LazyUtils.ToLazy(dataFactory), LazyUtils.ToLazy(eTagFactory), timeToLive, dataUpdateDetectInternal, shouldWaitForLock: false);
                    });
                }

                return cacheItem;
            }

            if (UseStrictThreadSafeModeForAutoReload)
            {
                _lockForLazyData.Value.EnterWriteLock();
                TryGetDataInner(key, out cacheItem);
            }

            try
            {
                return SetOrUpdateLazyData(key, cacheItem, LazyUtils.ToLazy(dataFactory), LazyUtils.ToLazy(eTagFactory), timeToLive, dataUpdateDetectInternal, shouldWaitForLock: true);
            }
            finally
            {
                if (UseStrictThreadSafeModeForAutoReload)
                {
                    _lockForLazyData.Value.ExitWriteLock();
                }
            }
        }

        private CacheItem SetOrUpdateLazyData<T>(
            string key,
            CacheItem oldCacheItem,
            Lazy<T> dataFactory,
            Lazy<string> eTagFactory,
            TimeSpan timeToLive,
            TimeSpan dataUpdateDetectInternal,
            bool shouldWaitForLock)
        {
            var isUpdateNeeded = oldCacheItem == null || oldCacheItem.IsUpdateNeeded(eTagFactory, dataUpdateDetectInternal, shouldWaitForLock);
            if (!isUpdateNeeded)
            {
                return oldCacheItem;
            }

            return AddOrUpdate(key, dataFactory, timeToLive, eTagFactory?.Value, shouldStorePreviousCacheItem: true);
        }

        public T GetData<T>(string key)
        {
            if (TryGetDataInner(key, out var cacheItem))
            {
                try
                {
                    return (T)cacheItem.Data;
                }
                catch (InvalidCastException e)
                {
                    Trace.TraceError(e.ToString());
                    return default(T);
                }
            }

            return default(T);
        }

        public object GetData(string key)
        {
            if (TryGetDataInner(key, out var cacheItem) && cacheItem != null)
            {
                return cacheItem.Data;
            }

            return null;
        }

        public bool TryDeleteKey(string key)
        {
            if (_keyDataMappings.TryRemove(key, out var item))
            {
                if (item.IsDataDisposable)
                {
                    _itemsToDispose.Enqueue(item);
                }

                return true;
            }

            return false;
        }

        public void SetData(string key, object data, TimeSpan timeToLive, string eTag = null)
        {
            AddOrUpdate(key, data, timeToLive, eTag, shouldStorePreviousCacheItem: false);
        }

        private CacheItem AddOrUpdate(string key, object data, TimeSpan timeToLive, string eTag, bool shouldStorePreviousCacheItem)
        {
            return _keyDataMappings.AddOrUpdate(
                key,
                k => new CacheItem(key, data, eTag, timeToLive),
                (k, c) =>
                {
                    if (c.IsUpdateNeeded(eTag))
                    {
                        if (c.PreviousCacheItem?.IsDataDisposable == true)
                        {
                            _itemsToDispose.Enqueue(c.PreviousCacheItem);
                        }

                        c.PreviousCacheItem = null;
                        var newItem = new CacheItem(k, data, eTag, timeToLive);
                        if (shouldStorePreviousCacheItem)
                        {
                            newItem.PreviousCacheItem = c;
                        }

                        return newItem;
                    }

                    c.LastAccessUtc = DateTimeOffset.UtcNow;
                    c.TimeToLive = timeToLive;

                    return c;
                });
        }

        internal bool TryGetDataInner(string key, out CacheItem cacheItem)
        {
            var success = false;
            cacheItem = null;

            success = _keyDataMappings.TryGetValue(key, out cacheItem) && cacheItem != null;
            if (!success)
            {
                return false;
            }

            if (!cacheItem.IsExpired)
            {
                cacheItem.LastAccessUtc = DateTimeOffset.UtcNow;
                return true;
            }

            TryDeleteKey(key);
            return false;
        }

        protected override void DisposeResources()
        {
            //// Add lock here

            foreach (var pair in _keyDataMappings)
            {
                pair.Value?.Dispose();
            }

            _keyDataMappings.Clear();

            _lockForClean.Dispose();
            (_lockForLazyData as IDisposable).Dispose();
        }
    }
}
