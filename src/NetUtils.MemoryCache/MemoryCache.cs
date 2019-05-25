using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace NetUtils.MemoryCache
{
    public static class MemoryCache
    {
        private static readonly ConcurrentDictionary<string, ICacheInstance> s_namedCacheInstances = new ConcurrentDictionary<string, ICacheInstance>();

        private static readonly Timer s_cacheCleanTimer = new Timer(_ => CleanUp(), null, CacheCleanCheckInternal, CacheCleanCheckInternal);

        public static readonly TimeSpan DefaultCacheCleanCheckInternal = TimeSpan.FromMinutes(1);

        private static TimeSpan s_cacheCleanCheckInternal = DefaultCacheCleanCheckInternal;
        public static TimeSpan CacheCleanCheckInternal
        {
            get
            {
                return s_cacheCleanCheckInternal;
            }
            set
            {
                s_cacheCleanCheckInternal = value;
                s_cacheCleanTimer.Change(s_cacheCleanCheckInternal, s_cacheCleanCheckInternal);
            }
        }

        public static ICacheInstance DefaultInstance { get; } = GetNamedInstance(nameof(DefaultInstance));

        public static ICacheInstance GetNamedInstance(string name)
        {
            _ = name ?? throw new ArgumentNullException(nameof(name));

            ICacheInstance result = s_namedCacheInstances.GetOrAdd(
                name,
                key => new MemoryCacheInstance(name));

            return result;
        }

        public static bool TryDeleteNamedInstance(string name)
        {
            name.RequireNotNullOrWhiteSpace(nameof(name));

            if (s_namedCacheInstances.TryRemove(name, out ICacheInstance cacheInstance))
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
            var snapshot = s_namedCacheInstances.Values.ToList();
            foreach (ICacheInstance instance in snapshot)
            {
                instance.CleanIfNeeded();
            }
        }
    }
}
