using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace NetUtils
{
    public class DisposableContainer : DisposableBase
    {
        private readonly ConcurrentBag<IDisposable> _bag = new ConcurrentBag<IDisposable>();

        public void Register(IDisposable @object)
        {
            _bag.Add(@object);
        }

        protected override void DisposeResources()
        {
            foreach (IDisposable item in _bag)
            {
                try
                {
                    item?.Dispose();
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.ToString());
                }
            }
        }
    }
}
