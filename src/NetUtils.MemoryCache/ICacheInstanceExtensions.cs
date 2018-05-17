namespace NetUtils.MemoryCache
{
    using System;
    using System.Diagnostics;

    public static class ICacheInstanceExtensions
    {
        public static T GetData<T>(this ICacheInstance cache, string key)
        {
            if (cache.TryGetData(key, out var cacheItem))
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

        public static object GetData(this ICacheInstance cache, string key)
        {
            if (cache.TryGetData(key, out var cacheItem) && cacheItem != null)
            {
                return cacheItem.Data;
            }

            return null;
        }
    }
}
