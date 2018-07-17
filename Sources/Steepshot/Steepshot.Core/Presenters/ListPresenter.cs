using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Errors;
using Steepshot.Core.Models;
using Steepshot.Core.Localization;
using System.Net;
using System.Collections;
using System.Data;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public abstract class ListPresenter<T> : BasePresenter, IList<T>, IListPresenter
    {
        private readonly object _sync;
        private CancellationTokenSource _singleTaskCancellationTokenSource;
        protected const int ServerMaxCount = 40;
        protected readonly List<T> Items;
        protected string OffsetUrl = string.Empty;
        public event Action<Status> SourceChanged;

        public bool IsLastReaded { get; protected set; }

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

        protected async Task<Exception> RunAsSingleTask(Func<CancellationToken, Task<Exception>> func, bool cancelPrevTask = true)
        {
            var available = AppSettings.ConnectionService.IsConnectionAvailable();
            if (!available)
                return new ValidationError(LocalizationKeys.InternetUnavailable);

            CancellationToken ts;
            lock (_sync)
            {
                if (_singleTaskCancellationTokenSource != null)
                {
                    if (cancelPrevTask)
                        _singleTaskCancellationTokenSource.Cancel();
                    else
                        return new OperationCanceledException();
                }
                _singleTaskCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(OnDisposeCts.Token);
                ts = _singleTaskCancellationTokenSource.Token;
            }
            try
            {
                return await func(ts);
            }
            catch (WebException ex)
            {
                return new RequestError(ex);
            }
            catch (Exception ex)
            {
                if (ts.IsCancellationRequested)
                    return new OperationCanceledException();

                available = AppSettings.ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new ValidationError(LocalizationKeys.InternetUnavailable);

                return ex;
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

        protected async Task<Exception> RunAsSingleTask<T1>(Func<T1, CancellationToken, Task<Exception>> func, T1 param1, bool cancelPrevTask = true)
        {
            var available = AppSettings.ConnectionService.IsConnectionAvailable();
            if (!available)
                return new ValidationError(LocalizationKeys.InternetUnavailable);

            CancellationToken ts;
            lock (_sync)
            {
                if (_singleTaskCancellationTokenSource != null)
                {
                    if (cancelPrevTask)
                        _singleTaskCancellationTokenSource.Cancel();
                    else
                        return new OperationCanceledException();
                }
                _singleTaskCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(OnDisposeCts.Token);
                ts = _singleTaskCancellationTokenSource.Token;
            }
            try
            {
                return await func(param1, ts);
            }
            catch (WebException ex)
            {
                return new RequestError(ex);
            }
            catch (Exception ex)
            {
                if (ts.IsCancellationRequested)
                    return new OperationCanceledException();

                available = AppSettings.ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new ValidationError(LocalizationKeys.InternetUnavailable);

                return ex;
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
