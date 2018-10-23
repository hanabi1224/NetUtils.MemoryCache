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
        private readonly ConcurrentDictionary<string, object> _keyTmpLockMappings = new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<string, CacheItem> _keyDataMappings = new ConcurrentDictionary<string, CacheItem>();
        private readonly ConcurrentQueue<IDisposable> _itemsToDispose = new ConcurrentQueue<IDisposable>();

        private readonly ReaderWriterLockSlim _lockForClean = new ReaderWriterLockSlim();

        private DateTimeOffset _lastClean = DateTimeOffset.MinValue;

        public static readonly TimeSpan DefaultCacheCleanInternal = TimeSpan.FromMinutes(5);

        public MemoryCacheInstance(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Name { get; }

        public TimeSpan CleanInternal { get; set; } = DefaultCacheCleanInternal;

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

            eTag = cacheItem.ETag;
            return (T)cacheItem.Data;
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
#pragma warning disable VSTHRD110
                    Task.Run(() =>
#pragma warning restore VSTHRD110
                    {
                        SetOrUpdateLazyData(key, cacheItem, LazyUtils.ToLazy(dataFactory), LazyUtils.ToLazy(eTagFactory), timeToLive, dataUpdateDetectInternal, shouldWaitForLock: false);
                    });
                }

                return cacheItem;
            }

            return SetOrUpdateLazyData(key, cacheItem, LazyUtils.ToLazy(dataFactory), LazyUtils.ToLazy(eTagFactory), timeToLive, dataUpdateDetectInternal, shouldWaitForLock: true);
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
            object tmpLock = null;
            object lockToWait = null;
            if (oldCacheItem == null)
            {
                lockToWait = tmpLock = _keyTmpLockMappings.GetOrAdd(key, new object());
            }
            else
            {
                lockToWait = oldCacheItem.LockObj;
            }

            bool lockAquired = false;
            try
            {
                if (tmpLock != null)
                {
                    Monitor.Enter(lockToWait);
                    lockAquired = true;
                    if (TryGetDataInner(key, out var updatedOldCacheItem))
                    {
                        return updatedOldCacheItem;
                    }
                }
                else if (shouldWaitForLock)
                {
                    Monitor.Enter(lockToWait);
                    lockAquired = true;
                }
                else
                {
                    lockAquired = Monitor.TryEnter(lockToWait);
                }

                if (!lockAquired)
                {
                    return oldCacheItem;
                }

                var isUpdateNeeded = oldCacheItem?.IsUpdateNeeded(eTagFactory, dataUpdateDetectInternal) != false;
                if (!isUpdateNeeded)
                {
                    return oldCacheItem;
                }

                return AddOrUpdate(key, dataFactory.Value, timeToLive, eTagFactory?.Value);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                if (oldCacheItem != null)
                {
                    return oldCacheItem;
                }

                throw;
            }
            finally
            {
                if (tmpLock != null)
                {
                    _keyTmpLockMappings.TryRemove(key, out _);
                }

                if (lockAquired)
                {
                    Monitor.Exit(lockToWait);
                }
            }
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

            _keyTmpLockMappings.TryRemove(key, out _);

            return false;
        }

        public void SetData(string key, object data, TimeSpan timeToLive, string eTag = null)
        {
            AddOrUpdate(key, data, timeToLive, eTag);
        }

        private CacheItem AddOrUpdate(string key, object data, TimeSpan timeToLive, string eTag)
        {
            return _keyDataMappings.AddOrUpdate(
                key,
                k => new CacheItem(key, data, eTag, timeToLive),
                (k, c) =>
                {
                    if (c.IsUpdateNeeded(eTag))
                    {
                        if (c.IsDataDisposable)
                        {
                            _itemsToDispose.Enqueue(c);
                        }

                        var newItem = new CacheItem(k, data, eTag, timeToLive);
                        return newItem;
                    }

                    c.LastAccessUtc = DateTimeOffset.UtcNow;
                    c.TimeToLive = timeToLive;

                    return c;
                });
        }

        internal bool TryGetDataInner(string key, out CacheItem cacheItem)
        {
            var success = _keyDataMappings.TryGetValue(key, out cacheItem) && cacheItem != null;
            if (!success)
            {
                cacheItem = null;
                return false;
            }

            if (!cacheItem.IsExpired)
            {
                cacheItem.LastAccessUtc = DateTimeOffset.UtcNow;
                return true;
            }

            TryDeleteKey(key);
            cacheItem = null;
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
            _keyTmpLockMappings.Clear();

            _lockForClean.Dispose();
        }
    }
}
