using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public abstract class ListPresenter<T> : BasePresenter
    {
        private readonly object _sync;
        private CancellationTokenSource _singleTaskCancellationTokenSource;

        public bool IsLastReaded { get; protected set; }
        protected const int ServerMaxCount = 20;
        protected string OffsetUrl = string.Empty;

        public virtual int Count => Items.Count;
        protected readonly List<T> Items;
        

        public T this[int position]
        {
            get
            {
                lock (Items)
                {
                    if (position > -1 && position < Items.Count)
                        return Items[position];
                }
                return default(T);
            }
        }

        protected ListPresenter()
        {
            _sync = new object();
            Items = new List<T>();
        }

        public void Clear()
        {
            lock (Items)
                Items.Clear();
            IsLastReaded = false;
            OffsetUrl = string.Empty;
        }

        protected async Task<TResult> RunAsSingleTask<TResult>(Func<CancellationTokenSource, Task<TResult>> func, bool cancelPrevTask = true)
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
                return await func(_singleTaskCancellationTokenSource);
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

        protected async Task<TResult> RunAsSingleTask<T1, TResult>(Func<CancellationTokenSource, T1, Task<TResult>> func, T1 param1, bool cancelPrevTask = true)
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
                return await func(_singleTaskCancellationTokenSource, param1);
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

        protected async Task<TResult> RunAsSingleTask<T1, T2, TResult>(Func<CancellationTokenSource, T1, T2, Task<TResult>> func, T1 param1, T2 param2, bool cancelPrevTask = true)
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
                return await func(_singleTaskCancellationTokenSource, param1, param2);
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
