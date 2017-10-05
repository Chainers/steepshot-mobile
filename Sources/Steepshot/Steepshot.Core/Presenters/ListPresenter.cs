using System;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public abstract class ListPresenter : BasePresenter
    {
        private readonly object _sync;
        private CancellationTokenSource _singleTaskCancellationTokenSource;

        public bool IsLastReaded { get; set; }
        protected const int ServerMaxCount = 20;
        protected string OffsetUrl = string.Empty;

        public abstract int Count { get; }

        protected ListPresenter()
        {
            _sync = new object();
        }

        protected async Task<TResult> RunAsSingleTask<T, TResult>(Func<T, CancellationTokenSource, Task<TResult>> func, T parameters, bool cancelPrevTask = true)
        {
            lock (_sync)
            {
                if (_singleTaskCancellationTokenSource != null)
                {
                    if (cancelPrevTask)
                        _singleTaskCancellationTokenSource.Cancel();
                    else
                        return default(TResult);
                }
                _singleTaskCancellationTokenSource = new CancellationTokenSource();
            }
            try
            {
                return await func(parameters, _singleTaskCancellationTokenSource);
            }
            catch (OperationCanceledException)
            {
                // to do nothing
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
            finally
            {
                lock (_sync)
                {
                    _singleTaskCancellationTokenSource.Dispose();
                    _singleTaskCancellationTokenSource = null;
                }
            }
            return default(TResult);
        }

        public void LoadCancel()
        {
            lock (_sync)
                _singleTaskCancellationTokenSource?.Cancel();
        }
    }
}