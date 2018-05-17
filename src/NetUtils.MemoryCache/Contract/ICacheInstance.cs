namespace NetUtils.MemoryCache
{
    using System;
    using System.Threading.Tasks;

    public interface ICacheInstance : IDisposable
    {
        string Name { get; }

        CacheExpirePolicy CacheExpirePolicy { get; }

        TimeSpan CleanInternal { get; set; }

        TimeSpan DataDisposeDelay { get; set; }

        bool UseStrictThreadSafeMode { get; set; }

        int Size { get; }

        bool TryGetData(string key, out ICacheItem cacheItem);

        bool TrySetData(string key, object data, TimeSpan timeToLive, string metadata = null);

        bool TryDeleteKey(string key);

        T GetDataOrCreate<T>(string key, Func<T> constructor, TimeSpan timeout, string metadata = null, bool shouldReloadInBackground = true);

        Task<T> GetDataOrCreateAsync<T>(string key, Func<Task<T>> constructor, TimeSpan timeout, string metadata = null, bool shouldReloadInBackground = true);

        void Clear();

        void CleanIfNeeded();
    }
}
