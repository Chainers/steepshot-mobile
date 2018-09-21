using System;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Clients;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Utils;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Presenters
{
    public abstract class BasePresenter
    {
        public SteepshotApiClient Api;

        protected CancellationTokenSource OnDisposeCts;


        public void SetClient(SteepshotApiClient steepshotApiClient)
        {
            TasksCancel();
            Api = steepshotApiClient;
        }


        public async Task<OperationResult<object>> TrySubscribeForPushesAsync(PushNotificationsModel model)
        {
            return await TryRunTaskAsync<PushNotificationsModel, object>(Api.SubscribeForPushesAsync, CancellationToken.None, model).ConfigureAwait(false);
        }


        #region TryRunTaskAsync

        protected static async Task<OperationResult<TResult>> TryRunTaskAsync<TResult>(Func<CancellationToken, Task<OperationResult<TResult>>> func, CancellationToken ct)
        {
            try
            {
                var available = AppSettings.ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ValidationException(LocalizationKeys.InternetUnavailable));

                return await func(ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ct.IsCancellationRequested)
                    return new OperationResult<TResult>(new OperationCanceledException());

                var available = AppSettings.ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ValidationException(LocalizationKeys.InternetUnavailable));

                await AppSettings.Logger.ErrorAsync(ex).ConfigureAwait(false);

                return new OperationResult<TResult>(ex);
            }
        }

        protected static async Task<OperationResult<TResult>> TryRunTaskAsync<T1, TResult>(Func<T1, CancellationToken, Task<OperationResult<TResult>>> func, CancellationToken ct, T1 param1)
        {
            try
            {
                var available = AppSettings.ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ValidationException(LocalizationKeys.InternetUnavailable));

                return await func(param1, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ct.IsCancellationRequested)
                    return new OperationResult<TResult>(new OperationCanceledException());

                var available = AppSettings.ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ValidationException(LocalizationKeys.InternetUnavailable));

                await AppSettings.Logger.ErrorAsync(ex).ConfigureAwait(false);

                return new OperationResult<TResult>(ex);
            }
        }

        protected static async Task<OperationResult<TResult>> TryRunTaskAsync<T1, T2, TResult>(Func<T1, T2, CancellationToken, Task<OperationResult<TResult>>> func, CancellationToken ct, T1 param1, T2 param2)
        {
            try
            {
                var available = AppSettings.ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ValidationException(LocalizationKeys.InternetUnavailable));

                return await func(param1, param2, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ct.IsCancellationRequested)
                    return new OperationResult<TResult>(new OperationCanceledException());

                var available = AppSettings.ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ValidationException(LocalizationKeys.InternetUnavailable));

                await AppSettings.Logger.ErrorAsync(ex).ConfigureAwait(false);

                return new OperationResult<TResult>(ex);
            }
        }


        protected static async Task<Exception> TryRunTaskAsync(Func<CancellationToken, Task<Exception>> func, CancellationToken ct)
        {
            try
            {
                var available = AppSettings.ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new ValidationException(LocalizationKeys.InternetUnavailable);

                return await func(ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ct.IsCancellationRequested)
                    return new OperationCanceledException();

                var available = AppSettings.ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new ValidationException(LocalizationKeys.InternetUnavailable);

                await AppSettings.Logger.ErrorAsync(ex).ConfigureAwait(false);

                return ex;
            }
        }

        protected static async Task<Exception> TryRunTaskAsync<T1>(Func<T1, CancellationToken, Task<Exception>> func, CancellationToken ct, T1 param1)
        {
            try
            {
                var available = AppSettings.ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new ValidationException(LocalizationKeys.InternetUnavailable);

                return await func(param1, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ct.IsCancellationRequested)
                    return new OperationCanceledException();

                var available = AppSettings.ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new ValidationException(LocalizationKeys.InternetUnavailable);

                await AppSettings.Logger.ErrorAsync(ex).ConfigureAwait(false);

                return ex;
            }
        }

        protected static async Task<Exception> TryRunTaskAsync<T1, T2>(Func<T1, T2, CancellationToken, Task<Exception>> func, CancellationToken ct, T1 param1, T2 param2)
        {
            try
            {
                var available = AppSettings.ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new ValidationException(LocalizationKeys.InternetUnavailable);

                return await func(param1, param2, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ct.IsCancellationRequested)
                    return new OperationCanceledException();

                var available = AppSettings.ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new ValidationException(LocalizationKeys.InternetUnavailable);

                await AppSettings.Logger.ErrorAsync(ex).ConfigureAwait(false);

                return ex;
            }
        }

        #endregion

        public void TasksCancel()
        {
            if (OnDisposeCts != null && !OnDisposeCts.IsCancellationRequested)
                OnDisposeCts.Cancel();

            OnDisposeCts = new CancellationTokenSource();
        }
    }
}
