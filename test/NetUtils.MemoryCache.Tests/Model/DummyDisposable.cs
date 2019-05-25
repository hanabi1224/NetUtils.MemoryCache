using System;

namespace NetUtils.MemoryCache.Tests
{
    public class DummyDisposable : DisposableBase
    {
        private readonly Action _callbackOnDispose;

        public DummyDisposable(Action callbackOnDispose)
        {
            _callbackOnDispose = callbackOnDispose;
        }

        protected override void DisposeResources()
        {
            _callbackOnDispose?.Invoke();
        }
    }
}
