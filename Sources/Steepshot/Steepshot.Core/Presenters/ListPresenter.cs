using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Errors;
using Steepshot.Core.Models;
using Steepshot.Core.Utils;
using Steepshot.Core.Localization;
using System.Net;
using System.Collections;
using System.Data;

namespace Steepshot.Core.Presenters
{
    public abstract class ListPresenter<T> : BasePresenter, IList<T>
    {
        private readonly object _sync;
        private CancellationTokenSource _singleTaskCancellationTokenSource;

        protected const int ServerMaxCount = 40;
        protected readonly List<T> Items;
        protected string OffsetUrl = string.Empty;

        public bool IsLastReaded { get; protected set; }
        public event Action<Status> SourceChanged;

        protected ListPresenter()
        {
            _sync = new object();
            Items = new List<T>();
        }


        public virtual void Clear(bool isNotify)
        {
            bool isEmpty;
            lock (Items)
            {
                isEmpty = Items.Count == 0;
                Items.Clear();
            }
            IsLastReaded = false;
            OffsetUrl = string.Empty;
            if (isNotify)
                NotifySourceChanged(nameof(Clear), isEmpty);
        }

        public void LoadCancel()
        {
            lock (_sync)
            {
                _singleTaskCancellationTokenSource?.Cancel();
                _singleTaskCancellationTokenSource = null;
            }
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
            catch (WebException ex)
            {
                AppSettings.Reporter.SendCrash(ex);
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
            catch (WebException ex)
            {
                AppSettings.Reporter.SendCrash(ex);
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


        public int FindIndex(Predicate<T> match)
        {
            lock (Items)
                return Items.FindIndex(match);
        }

        internal virtual void NotifySourceChanged(string sender, bool isChanged)
        {
            SourceChanged?.Invoke(new Status(sender, isChanged));
        }

        #region IList<T>

        public bool IsReadOnly => true;


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
            set { throw new ReadOnlyException("Collection is closed for outside changes."); }
        }

        public int Count
        {
            get
            {
                if (Items != null)
                {
                    lock (Items)
                        return Items.Count;
                }
                return 0;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (Items)
                return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            throw new ReadOnlyException("Collection is closed for outside changes.");
        }

        public void Clear()
        {
            Clear(true);
        }

        public bool Contains(T item)
        {
            lock (Items)
                return Items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (Items)
                Items.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            throw new ReadOnlyException("Collection is closed for outside changes.");
        }

        public int IndexOf(T item)
        {
            lock (Items)
                return Items.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            throw new ReadOnlyException("Collection is closed for outside changes.");
        }

        public void RemoveAt(int index)
        {
            throw new ReadOnlyException("Collection is closed for outside changes.");
        }

        #endregion
    }
}
