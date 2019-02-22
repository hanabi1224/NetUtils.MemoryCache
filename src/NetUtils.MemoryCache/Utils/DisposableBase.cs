
using System;

namespace NetUtils.MemoryCache.Utils
{
    public abstract class DisposableBase : IDisposable
    {
        private bool _isDisposed;

        ~DisposableBase()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
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
                this.DisposeResources();
            }

            _isDisposed = true;
        }

        protected abstract void DisposeResources();
    }
}
