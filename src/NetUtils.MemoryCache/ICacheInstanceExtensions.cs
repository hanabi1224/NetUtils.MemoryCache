
using System;
using System.Threading.Tasks;

namespace NetUtils.MemoryCache
{
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
                eTag: out _,
                shouldReloadInBackground: shouldReloadInBackground);
        }

        public static T GetAutoReloadDataWithInterval<T>(
            this ICacheInstance cache,
            string key,
            Func<Task<T>> dataFactory,
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
                eTag: out _,
                shouldReloadInBackground: shouldReloadInBackground);
        }

        public static T GetAutoReloadDataWithCache<T>(
            this ICacheInstance cache,
            string key,
            Func<Task<T>> dataFactory,
            Func<Task<string>> eTagFactory,
            TimeSpan timeToLive,
            TimeSpan dataUpdateDetectInternal,
            out string eTag,
            bool shouldReloadInBackground = true)
        {
            Func<T> df = () => dataFactory().ConfigureAwait(false).GetAwaiter().GetResult();
            Func<string> ef = () => eTagFactory?.Invoke().ConfigureAwait(false).GetAwaiter().GetResult();
            return cache.GetAutoReloadDataWithCache<T>(
                key: key,
                dataFactory: df,
                eTagFactory: ef,
                timeToLive: timeToLive,
                dataUpdateDetectInternal: dataUpdateDetectInternal,
                eTag: out eTag,
                shouldReloadInBackground: shouldReloadInBackground);
        }

        public static T GetAutoReloadDataWithCache<T>(
            this ICacheInstance cache,
            string key,
            Func<Task<T>> dataFactory,
            Func<Task<string>> eTagFactory,
            TimeSpan timeToLive,
            TimeSpan dataUpdateDetectInternal,
            bool shouldReloadInBackground = true)
        {
            return cache.GetAutoReloadDataWithCache<T>(
                key: key,
                dataFactory: dataFactory,
                eTagFactory: eTagFactory,
                timeToLive: timeToLive,
                dataUpdateDetectInternal: dataUpdateDetectInternal,
                eTag: out _,
                shouldReloadInBackground: shouldReloadInBackground);
        }

        public static T GetAutoReloadDataWithCache<T>(
            this ICacheInstance cache,
            string key,
            Func<T> dataFactory,
            Func<string> eTagFactory,
            TimeSpan timeToLive,
            TimeSpan dataUpdateDetectInternal,
            bool shouldReloadInBackground = true)
        {
            return cache.GetAutoReloadDataWithCache<T>(
                key: key,
                dataFactory: dataFactory,
                eTagFactory: eTagFactory,
                timeToLive: timeToLive,
                dataUpdateDetectInternal: dataUpdateDetectInternal,
                eTag: out _,
                shouldReloadInBackground: shouldReloadInBackground);
        }
    }
}
