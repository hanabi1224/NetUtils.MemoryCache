
using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using NetUtils.MemoryCache.Utils;

namespace NetUtils.MemoryCache
{
    internal class CacheItem : DisposableBase, ICacheItem
    {
        internal readonly object LockObj = new object();

        private readonly WeakReference<string> _key;
        public CacheItem(string key, object data, string eTag, TimeSpan timeToLive)
        {
            _key = new WeakReference<string>(key);
            Data = data;
            ETag = eTag;
            TimeToLive = timeToLive;

            LastAccessUtc = LastETagCheckUtc = DateTimeOffset.UtcNow;
            IsDataDisposable = Data is IDisposable;
            IsDataEnumerable = Data is IEnumerable;
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

        public object Data { get; set; }

        public bool IsDataDisposable { get; }

        public bool IsDataEnumerable { get; }

        public string ETag { get; set; }

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

        public bool IsUpdateNeeded(Lazy<string> newETagFactory, TimeSpan dataUpdateDetectInternal)
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

            return false;
        }

        protected override void DisposeResources()
        {
            if (IsDataDisposable)
            {
                (Data as IDisposable)?.Dispose();
            }
            else if (IsDataEnumerable)
            {
                var enumerable = Data as IEnumerable;
                if (enumerable != null)
                {
                    foreach (var data in enumerable)
                    {
                        try
                        {
                            if (data is IDisposable disposable)
                            {
                                disposable?.Dispose();
                            }
                        }
                        catch (Exception e)
                        {
                            Trace.TraceError(e.ToString());
                        }
                    }
                }
            }
        }
    }
}
