
using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;

namespace NetUtils
{
    public class LazyDisposable<T> : Lazy<T>, IDisposableObservable
    {
        public LazyDisposable() : base() { }

        public LazyDisposable(bool isThreadSafe) : base(isThreadSafe) { }

        public LazyDisposable(LazyThreadSafetyMode mode) : base(mode) { }

        public LazyDisposable(Func<T> valueFactory) : base(valueFactory) { }

        public LazyDisposable(Func<T> valueFactory, bool isThreadSafe) : base(valueFactory, isThreadSafe) { }

        public LazyDisposable(Func<T> valueFactory, LazyThreadSafetyMode mode) : base(valueFactory, mode) { }

        ~LazyDisposable()
        {
            Dispose(false);
        }

        #region IDisposableObservable
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                DisposeResources();
            }

            IsDisposed = true;
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
