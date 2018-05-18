using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace NetUtils.MemoryCache
{
    public static class MemoryCache
    {
        private static readonly ConcurrentDictionary<string, ICacheInstance> _namedCacheInstances = new ConcurrentDictionary<string, ICacheInstance>();

        private static readonly Timer _cacheCleanTimer;

        public static readonly TimeSpan DefaultCacheCleanCheckInternal = TimeSpan.FromMinutes(1);

        static MemoryCache()
        {
            _cacheCleanTimer = new Timer(_ => CleanUp(), null, CacheCleanCheckInternal, CacheCleanCheckInternal);
        }

        private static TimeSpan _cacheCleanCheckInternal = DefaultCacheCleanCheckInternal;
        public static TimeSpan CacheCleanCheckInternal
        {
            get
            {
                return _cacheCleanCheckInternal;
            }
            set
            {
                _cacheCleanCheckInternal = value;
                _cacheCleanTimer.Change(_cacheCleanCheckInternal, _cacheCleanCheckInternal);
            }
        }

        public static ICacheInstance DefaultInstance { get; } = GetNamedInstance(nameof(DefaultInstance));

        public static ICacheInstance GetNamedInstance(string name)
        {
            _ = name ?? throw new ArgumentNullException(nameof(name));

            var result = _namedCacheInstances.GetOrAdd(
                name,
                key => new MemoryCacheInstance(name));

            return result;
        }

        public static bool TryDeleteNamedInstance(string name)
        {
            _ = name ?? throw new ArgumentNullException(nameof(name));

            if (_namedCacheInstances.TryRemove(name, out var cacheInstance))
            {
                using (cacheInstance)
                {
                    return true;
                }
            }

            return false;
        }

        private static void CleanUp()
        {
            var snapshot = _namedCacheInstances.Values.ToList();
            foreach (var instance in snapshot)
            {
                instance.CleanIfNeeded();
            }
        }
    }
}
