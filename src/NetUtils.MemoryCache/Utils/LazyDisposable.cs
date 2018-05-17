namespace NetUtils.MemoryCache.Utils
{
    using System;
    using System.Threading;

    public class LazyDisposable<T> : Lazy<T>, IDisposable
    {
        public LazyDisposable() : base() { }

        public LazyDisposable(bool isThreadSafe) : base(isThreadSafe) { }

        public LazyDisposable(LazyThreadSafetyMode mode) : base(mode) { }

        public LazyDisposable(Func<T> valueFactory) : base(valueFactory) { }

        public LazyDisposable(Func<T> valueFactory, bool isThreadSafe) : base(valueFactory, isThreadSafe) { }

        public LazyDisposable(Func<T> valueFactory, LazyThreadSafetyMode mode) : base(valueFactory, mode) { }

        ~LazyDisposable()
        {
            this.Dispose(false);
        }

        #region IDisposable
        private bool isDisposed;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.DisposeResources();
            }

            this.isDisposed = true;
        }

        protected void DisposeResources()
        {
            if (this.IsValueCreated && this.Value is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        #endregion
    }
}
