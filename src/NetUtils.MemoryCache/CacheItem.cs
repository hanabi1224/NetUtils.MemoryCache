namespace NetUtils.MemoryCache
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using Utils;

    internal class CacheItem : DisposableBase, ICacheItem
    {
        public CacheItem(string key, object data, string metadata)
        {
            this.Key = key;
            this.Data = data;
            this.MetaData = metadata;
        }

        public string Key { get; set; }

        public object Data { get; set; }

        public bool IsDataDisposable => Data is IDisposable;

        public string MetaData { get; set; }

        public DateTimeOffset LastUpdateUtc { get; set; }

        public DateTimeOffset LastAccessUtc { get; set; }

        public TimeSpan TimeToLive { get; set; }

        public DateTimeOffset GetExpireUtc(CacheExpirePolicy cacheExpirePolicy)
        {
            if (TimeToLive == TimeSpan.MaxValue
                || TimeToLive == Timeout.InfiniteTimeSpan)
            {
                return DateTimeOffset.MaxValue;
            }
            else
            {
                try
                {
                    return (cacheExpirePolicy == CacheExpirePolicy.ExpireOnLastUpdate ? LastUpdateUtc : LastAccessUtc) + TimeToLive;
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.ToString());
                    return DateTimeOffset.MaxValue;
                }
            }
        }

        protected override void DisposeResources()
        {
            (Data as IDisposable)?.Dispose();
        }
    }
}
