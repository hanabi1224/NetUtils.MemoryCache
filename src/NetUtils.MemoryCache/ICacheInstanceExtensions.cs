namespace NetUtils.MemoryCache
{
    using System;

    public static class ICacheInstanceExtensions
    {
        public static T GetAutoReloadDataWithInterval<T>(
            this ICacheInstance cache,
            string key,
            Func<T> dataFactory,
            TimeSpan timeToLive,
            TimeSpan dataReloadInternal,
            bool shouldReloadInBackground = true)
        {
            return cache.GetAutoReloadDataWithCache<T>(
                key: key,
                dataFactory: dataFactory,
                eTagFactory: null,
                timeToLive: timeToLive,
                dataUpdateDetectInternal: dataReloadInternal,
                shouldReloadInBackground: shouldReloadInBackground);
        }
    }
}
