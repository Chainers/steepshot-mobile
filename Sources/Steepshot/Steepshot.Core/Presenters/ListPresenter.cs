using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Errors;
using Steepshot.Core.Models;
using Steepshot.Core.Utils;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Presenters
{
    public abstract class ListPresenter<T> : BasePresenter
    {
        private readonly object _sync;
        private CancellationTokenSource _singleTaskCancellationTokenSource;

        protected const int ServerMaxCount = 40;
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

        public virtual void Clear(bool isNotify = true)
        {
            lock (Items)
                Items.Clear();
            IsLastReaded = false;
            OffsetUrl = string.Empty;
            if (isNotify)
                NotifySourceChanged(nameof(Clear), true);
        }


        protected async Task<ErrorBase> RunAsSingleTask(Func<CancellationToken, Task<ErrorBase>> func, bool cancelPrevTask = true)
        {
            var available = ConnectionService.IsConnectionAvailable();
            if (!available)
                return new AppError(LocalizationKeys.InternetUnavailable);

            CancellationToken ts;
            lock (_sync)
            {
                if (_singleTaskCancellationTokenSource != null)
                {
                    if (cancelPrevTask)
                        _singleTaskCancellationTokenSource.Cancel();
                    else
                        return new CanceledError();
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
                return new CanceledError();
            }
            catch (ErrorBase ex)
            {
                return ex;
            }
            catch (System.Net.WebException)
            {
                return new RequestError();
            }
            catch (Exception ex)
            {
                if (ts.IsCancellationRequested)
                {
                    return new CanceledError();
                }
                else
                {
                    AppSettings.Reporter.SendCrash(ex);
                    return new AppError(LocalizationKeys.UnknownError);
                }
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
        }

        protected async Task<ErrorBase> RunAsSingleTask<T1>(Func<T1, CancellationToken, Task<ErrorBase>> func, T1 param1, bool cancelPrevTask = true)
        {
            var available = ConnectionService.IsConnectionAvailable();
            if (!available)
                return new AppError(LocalizationKeys.InternetUnavailable);

            CancellationToken ts;
            lock (_sync)
            {
                if (_singleTaskCancellationTokenSource != null)
                {
                    if (cancelPrevTask)
                        _singleTaskCancellationTokenSource.Cancel();
                    else
                        return new CanceledError();
                }
                _singleTaskCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(OnDisposeCts.Token);
                ts = _singleTaskCancellationTokenSource.Token;
            }
            try
            {
                return await func(param1, ts);
            }
            catch (OperationCanceledException)
            {
                return new CanceledError();
            }
            catch (ErrorBase ex)
            {
                return ex;
            }
            catch (Exception ex)
            {
                if (ts.IsCancellationRequested)
                {
                    return new CanceledError();
                }
                else
                {
                    AppSettings.Reporter.SendCrash(ex, param1);
                    return new AppError(LocalizationKeys.UnknownError);
                }
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
        }

        public void LoadCancel()
        {
            lock (_sync)
            {
                _singleTaskCancellationTokenSource?.Cancel();
                _singleTaskCancellationTokenSource = null;
            }
        }

        internal void NotifySourceChanged(string sender, bool isChanged)
        {
            SourceChanged?.Invoke(new Status(sender, isChanged));
        }
    }
}
