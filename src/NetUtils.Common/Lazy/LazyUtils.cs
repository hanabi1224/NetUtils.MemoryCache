﻿
using System;
using System.Collections;
using System.Threading.Tasks;

namespace NetUtils
{
    public static class LazyUtils
    {
        private static readonly Type s_typeIDisposable = typeof(IDisposable);
        private static readonly Type s_typeIEnumerable = typeof(IEnumerable);

        public static Lazy<T> ToLazy<T>(this Func<T> func, bool isThreadSafe = true)
        {
            _ = func ?? throw new ArgumentNullException(nameof(func));

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
            _ = func ?? throw new ArgumentNullException(nameof(func));

            return ToLazy(func: () => func.Invoke().ConfigureAwait(false).GetAwaiter().GetResult(), isThreadSafe: isThreadSafe);
        }

        public static AsyncLazy<T> ToAsyncLazy<T>(this Func<Task<T>> func)
        {
            _ = func ?? throw new ArgumentNullException(nameof(func));

            return new AsyncLazy<T>(func);
        }
    }
}
