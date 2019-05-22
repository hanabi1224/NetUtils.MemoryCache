
using System;
using System.Collections;
using System.Threading.Tasks;

namespace NetUtils.MemoryCache.Utils
{
    public static class LazyUtils
    {
        private static readonly Type s_typeIDisposable = typeof(IDisposable);
        private static readonly Type s_typeIEnumerable = typeof(IEnumerable);

        public static Lazy<T> ToLazy<T>(this Func<T> func, bool isThreadSafe = true)
        {
            if (func == null)
            {
                return null;
            }

            if (s_typeIDisposable.IsAssignableFrom(typeof(T))
                || s_typeIEnumerable.IsAssignableFrom(typeof(T)))
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

            if (s_typeIDisposable.IsAssignableFrom(typeof(T))
                || s_typeIEnumerable.IsAssignableFrom(typeof(T)))
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
