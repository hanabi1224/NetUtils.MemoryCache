namespace NetUtils.MemoryCache
{
    using System;

    public interface ICacheInstance : IDisposable
    {
        string Name { get; }

        TimeSpan CleanInternal { get; set; }

        int Size { get; }

        T GetAutoReloadDataWithCache<T>(
            string key,
            Func<T> dataFactory,
            Func<string> eTagFactory,
            TimeSpan timeToLive,
            TimeSpan dataUpdateDetectInternal,
            out string eTag,
            bool shouldReloadInBackground = true);

        void SetData(string key, object data, TimeSpan timeToLive, string eTag = null);

        bool TryDeleteKey(string key);

        T GetData<T>(string key);

        object GetData(string key);

        void Clear();

        void CleanIfNeeded();
    }
}
