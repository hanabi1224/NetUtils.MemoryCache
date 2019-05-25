using System;
using System.Diagnostics;

namespace NetUtils
{
    public interface IPerfMonitorScope
    {
        TimeSpan Elapsed { get; }

        long ElapsedMilliseconds { get; }

        DateTimeOffset StartTime { get; }
    }

    public class PerfMonitorScope : DisposableBase, IPerfMonitorScope
    {
        private readonly Stopwatch _stopwatch;
        private readonly Action<Stopwatch> _onExit;

        public PerfMonitorScope(Action<Stopwatch> onExit = null)
        {
            StartTime = DateTimeOffset.Now;
            _stopwatch = Stopwatch.StartNew();
            _onExit = onExit;
        }

        public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;

        public TimeSpan Elapsed => _stopwatch.Elapsed;

        public DateTimeOffset StartTime { get; }

        protected override void DisposeResources()
        {
            _stopwatch.Stop();
            _onExit?.Invoke(_stopwatch);
        }
    }
}
