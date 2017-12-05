using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Models;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public abstract class ListPresenter<T> : BasePresenter
    {
        private readonly object _sync;
        private CancellationTokenSource _singleTaskCancellationTokenSource;

        protected const int ServerMaxCount = 20;
        protected readonly List<T> Items;
        protected string OffsetUrl = string.Empty;

        public bool IsLastReaded { get; protected set; }
        public event Action<Status> SourceChanged;

        public virtual int Count => Items.Count;


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

        public void Clear(bool isNotify = true)
        {
            lock (Items)
                Items.Clear();
            IsLastReaded = false;
            OffsetUrl = string.Empty;
            if (isNotify)
                NotifySourceChanged();
        }


        protected async Task<List<string>> RunAsSingleTask(Func<CancellationToken, Task<List<string>>> func, bool cancelPrevTask = true)
        {
            var available = ConnectionService.IsConnectionAvailable();
            if (!available)
                return new List<string> { Localization.Errors.InternetUnavailable };

            CancellationToken ts;
            lock (_sync)
            {
                if (_singleTaskCancellationTokenSource != null)
                {
                    if (cancelPrevTask)
                        _singleTaskCancellationTokenSource.Cancel();
                    else
                        return null;
                }
                _singleTaskCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(OnDisposeCts.Token);
                ts = _singleTaskCancellationTokenSource.Token;
            }
            try
            {
                return await func(ts);
            }
            catch (OperationCanceledException)
            {
                // to do nothing
            }
            catch (ApplicationExceptionBase ex)
            {
                return new List<string> { ex.Message };
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
            finally
            {
                lock (_sync)
                {
                    if (_singleTaskCancellationTokenSource != null)
                    {
                        _singleTaskCancellationTokenSource.Dispose();
                        _singleTaskCancellationTokenSource = null;
                    }
                }
            }
            return null;
        }

        protected async Task<List<string>> RunAsSingleTask<T1>(Func<CancellationToken, T1, Task<List<string>>> func, T1 param1, bool cancelPrevTask = true)
        {
            var available = ConnectionService.IsConnectionAvailable();
            if (!available)
                return new List<string> { Localization.Errors.InternetUnavailable };

            CancellationToken ts;
            lock (_sync)
            {
                if (_singleTaskCancellationTokenSource != null)
                {
                    if (cancelPrevTask)
                        _singleTaskCancellationTokenSource.Cancel();
                    else
                        return null;
                }
                _singleTaskCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(OnDisposeCts.Token);
                ts = _singleTaskCancellationTokenSource.Token;
            }
            try
            {
                return await func(ts, param1);
            }
            catch (OperationCanceledException)
            {
                // to do nothing
            }
            catch (ApplicationExceptionBase ex)
            {
                return new List<string> { ex.Message };
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
            finally
            {
                lock (_sync)
                {
                    if (_singleTaskCancellationTokenSource != null)
                    {
                        _singleTaskCancellationTokenSource.Dispose();
                        _singleTaskCancellationTokenSource = null;
                    }
                }
            }
            return null;
        }

        public void LoadCancel()
        {
            lock (_sync)
            {
                _singleTaskCancellationTokenSource?.Cancel();
                _singleTaskCancellationTokenSource = null;
            }
        }

        internal void NotifySourceChanged()
        {
            SourceChanged?.Invoke(new Status(true));
        }

        internal void NotifySourceChanged(bool isChanged)
        {
            SourceChanged?.Invoke(new Status(isChanged));
        }
    }
}
