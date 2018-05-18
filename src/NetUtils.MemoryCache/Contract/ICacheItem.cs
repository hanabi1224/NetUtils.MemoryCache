using System;

namespace NetUtils.MemoryCache
{
    internal interface ICacheItem
    {
        string Key { get; }

        object Data { get; }

        bool IsDataDisposable { get; }

        string ETag { get; }

        DateTimeOffset LastAccessUtc { get; }

        TimeSpan TimeToLive { get; }

        bool IsExpired { get; }
    }
}
