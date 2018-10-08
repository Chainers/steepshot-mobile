using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models;
using Steepshot.Core.Localization;
using System.Collections;
using System.Data;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Models.Common;
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

        protected ListPresenter(IConnectionService connectionService, ILogService logService)
            : base(connectionService, logService)
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


        protected async Task<OperationResult<TResult>> RunAsSingleTaskAsync<TResult>(Func<CancellationToken, Task<OperationResult<TResult>>> func, bool cancelPrevTask = true)
        {
            var available = ConnectionService.IsConnectionAvailable();
            if (!available)
                return new OperationResult<TResult>(new ValidationException(LocalizationKeys.InternetUnavailable));

            CancellationToken ts;
            lock (_sync)
            {
                if (_singleTaskCancellationTokenSource != null)
                {
                    if (cancelPrevTask)
                        _singleTaskCancellationTokenSource.Cancel();
                    else
                        return new OperationResult<TResult>(new OperationCanceledException());
                }
                _singleTaskCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(OnDisposeCts.Token);
                ts = _singleTaskCancellationTokenSource.Token;
            }

            try
            {
                return await func(ts).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ts.IsCancellationRequested)
                    return new OperationResult<TResult>(new OperationCanceledException());

                available = ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ValidationException(LocalizationKeys.InternetUnavailable));

                await LogService.ErrorAsync(ex).ConfigureAwait(false);
                return new OperationResult<TResult>(ex);
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

        protected async Task<OperationResult<TResult>> RunAsSingleTaskAsync<T1, TResult>(Func<T1, CancellationToken, Task<OperationResult<TResult>>> func, T1 arg1, bool cancelPrevTask = true)
        {
            var available = ConnectionService.IsConnectionAvailable();
            if (!available)
                return new OperationResult<TResult>(new ValidationException(LocalizationKeys.InternetUnavailable));

            CancellationToken ts;
            lock (_sync)
            {
                if (_singleTaskCancellationTokenSource != null)
                {
                    if (cancelPrevTask)
                        _singleTaskCancellationTokenSource.Cancel();
                    else
                        return new OperationResult<TResult>(new OperationCanceledException());
                }
                _singleTaskCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(OnDisposeCts.Token);
                ts = _singleTaskCancellationTokenSource.Token;
            }

            try
            {
                return await func(arg1, ts).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ts.IsCancellationRequested)
                    return new OperationResult<TResult>(new OperationCanceledException());

                available = ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ValidationException(LocalizationKeys.InternetUnavailable));

                await LogService.ErrorAsync(ex).ConfigureAwait(false);
                return new OperationResult<TResult>(ex);
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
