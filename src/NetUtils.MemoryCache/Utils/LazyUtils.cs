namespace NetUtils.MemoryCache.Utils
{
    using System;
    using System.Threading.Tasks;

    public static class LazyUtils
    {
        private static readonly Type _typeIDisposable = typeof(IDisposable);

        public static Lazy<T> ToLazy<T>(this Func<T> func, bool isThreadSafe = true)
        {
            if (func == null)
            {
                return null;
            }

            if (_typeIDisposable.IsAssignableFrom(typeof(T)))
            {
                return new LazyDisposable<T>(func, isThreadSafe: isThreadSafe);
            }
            else
            {
                return new Lazy<T>(func, isThreadSafe: isThreadSafe);
            }
        }

        public static Lazy<T> ToLazy<T>(this Func<Task<T>> func, bool isThreadSafe = true)
        {
            if (func == null)
            {
                return null;
            }

            if (_typeIDisposable.IsAssignableFrom(typeof(T)))
            {
                return new LazyDisposable<T>(() => func.Invoke().ConfigureAwait(false).GetAwaiter().GetResult(), isThreadSafe: isThreadSafe);
            }
            else
            {
                return new Lazy<T>(() => func.Invoke().ConfigureAwait(false).GetAwaiter().GetResult(), isThreadSafe: isThreadSafe);
            }
        }
    }
}
