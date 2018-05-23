namespace NetUtils.MemoryCache
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using Utils;

    internal class CacheItem : DisposableBase, ICacheItem
    {
        private readonly object _lock = new object();

        private readonly WeakReference<string> _key;
        public CacheItem(string key, object data, string eTag, TimeSpan timeToLive)
        {
            _key = new WeakReference<string>(key);
            this.Data = data;
            this.ETag = eTag;
            this.TimeToLive = timeToLive;

            LastAccessUtc = LastETagCheckUtc = DateTimeOffset.UtcNow;
            IsDataDisposable = Data is IDisposable;
        }

        public string Key
        {
            get
            {
                if (_key.TryGetTarget(out var target))
                {
                    return target;
                }

                return null;
            }
        }

        public object Data { get; }

        public bool IsDataDisposable { get; }

        public string ETag { get; }

        public DateTimeOffset LastAccessUtc { get; set; }

        public DateTimeOffset LastETagCheckUtc { get; private set; }

        public TimeSpan TimeToLive { get; set; }

        public bool IsExpired
        {
            get
            {
                if (TimeToLive == TimeSpan.MaxValue
                || TimeToLive == Timeout.InfiniteTimeSpan)
                {
                    return false;
                }
                else
                {
                    try
                    {
                        return LastAccessUtc + TimeToLive < DateTimeOffset.UtcNow;
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError(e.ToString());
                        return false;
                    }
                }
            }
        }

        public bool IsUpdateNeeded(string newETag)
        {
            return !(ETag == newETag && newETag != null);
        }

        public bool IsUpdateNeeded(Lazy<string> newETagFactory, TimeSpan dataUpdateDetectInternal, bool shouldWaitForLock)
        {
            bool lockAquired = false;
            if (shouldWaitForLock)
            {
                Monitor.Enter(_lock);
                lockAquired = true;
            }
            else
            {
                lockAquired = Monitor.TryEnter(_lock);
            }

            if (lockAquired)
            {
                try
                {
                    if (dataUpdateDetectInternal <= TimeSpan.Zero)
                    {
                        return true;
                    }

                    if (LastETagCheckUtc.AddSafe(dataUpdateDetectInternal) > DateTimeOffset.UtcNow)
                    {
                        return false;
                    }

                    LastETagCheckUtc = DateTimeOffset.UtcNow;
                    return IsUpdateNeeded(newETagFactory?.Value);
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.ToString());
                }
                finally
                {
                    Monitor.Exit(_lock);
                }
            }

            return false;
        }

        protected override void DisposeResources()
        {
            if (IsDataDisposable)
            {
                (Data as IDisposable).Dispose();
            }
        }
    }
}
