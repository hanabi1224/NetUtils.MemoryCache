namespace NetUtils.MemoryCache.Utils
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

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
