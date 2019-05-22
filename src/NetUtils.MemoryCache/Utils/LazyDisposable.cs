
using System;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace NetUtils.MemoryCache.Utils
{
    public class LazyDisposable<T> : Lazy<T>, IDisposable
    {
        [ExcludeFromCodeCoverage]
        public LazyDisposable() : base() { }

        [ExcludeFromCodeCoverage]
        public LazyDisposable(bool isThreadSafe) : base(isThreadSafe) { }

        [ExcludeFromCodeCoverage]
        public LazyDisposable(LazyThreadSafetyMode mode) : base(mode) { }

        [ExcludeFromCodeCoverage]
        public LazyDisposable(Func<T> valueFactory) : base(valueFactory) { }

        [ExcludeFromCodeCoverage]
        public LazyDisposable(Func<T> valueFactory, bool isThreadSafe) : base(valueFactory, isThreadSafe) { }

        [ExcludeFromCodeCoverage]
        public LazyDisposable(Func<T> valueFactory, LazyThreadSafetyMode mode) : base(valueFactory, mode) { }

        ~LazyDisposable()
        {
            Dispose(false);
        }

        #region IDisposable
        private bool _isDisposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                DisposeResources();
            }

            _isDisposed = true;
        }

        protected void DisposeResources()
        {
            if (IsValueCreated)
            {
                if (Value is IDisposable disposable)
                {
                    disposable?.Dispose();
                }
                else if (Value is IEnumerable enumerable)
                {
                    foreach (var data in enumerable)
                    {
                        try
                        {
                            if (data is IDisposable innerDisposable)
                            {
                                innerDisposable?.Dispose();
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
        #endregion
    }
}
