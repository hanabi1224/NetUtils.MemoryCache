using System;

namespace NetUtils.MemoryCache
{
    public interface ICacheItem
    {
        string Key { get; }

        object Data { get; }

        bool IsDataDisposable { get; }

        string MetaData { get; }

        DateTimeOffset LastUpdateUtc { get; }

        DateTimeOffset LastAccessUtc { get; }

        TimeSpan TimeToLive { get; }

        DateTimeOffset GetExpireUtc(CacheExpirePolicy cacheExpirePolicy);
    }
}
