namespace NetUtils.MemoryCache.Utils
{
    using System;

    public static class LazyUtils
    {
        public static Lazy<T> ToLazy<T>(this Func<T> func)
        {
            _ = func ?? throw new ArgumentNullException(nameof(func));
            return new LazyDisposable<T>(func, isThreadSafe: true);
        }
    }
}
