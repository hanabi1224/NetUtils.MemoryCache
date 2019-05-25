using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NetUtils
{
    public class AsyncLazy<TValue> : DisposableBase
    {
        private readonly TaskCompletionSource<TValue> _tcs = new TaskCompletionSource<TValue>();

        private readonly Func<Task<TValue>> _taskFactory;

        private int _taskInitiated;

        public AsyncLazy(Func<Task<TValue>> taskFactory)
        {
            _taskFactory = taskFactory.RequireNotNull(nameof(taskFactory));
        }

        public bool IsValueCreated => _tcs.Task.Status == TaskStatus.RanToCompletion;

        public async Task<TValue> GetValueAsync()
        {
            if (Interlocked.CompareExchange(ref _taskInitiated, 1, 0) == 1)
            {
                return await _tcs.Task.ConfigureAwait(false);
            }

            // We're first, so we need to kick off the task
            try
            {
                Task<TValue> task = _taskFactory();
                TValue result = await task.ConfigureAwait(false);
                _tcs.SetResult(result);
                return result;
            }
            catch (OperationCanceledException)
            {
                _tcs.SetCanceled();
                throw;
            }
            catch (Exception exception)
            {
                _tcs.SetException(exception);
                throw;
            }
        }

        protected override void DisposeResources()
        {
            if (IsValueCreated)
            {
                var value = GetValueAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                if (value is IDisposable disposable)
                {
                    disposable?.Dispose();
                }
                else if (value is IEnumerable enumerable)
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
    }
}
