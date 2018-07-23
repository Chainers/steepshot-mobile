using System;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Clients;
using Steepshot.Core.Errors;
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


        public async Task<OperationResult<object>> TrySubscribeForPushes(PushNotificationsModel model)
        {
            return await TryRunTask<PushNotificationsModel, object>(Api.SubscribeForPushes, CancellationToken.None, model);
        }


        #region TryRunTask

        protected static async Task<OperationResult<TResult>> TryRunTask<TResult>(Func<CancellationToken, Task<OperationResult<TResult>>> func, CancellationToken ct)
        {
            try
            {
                var available = AppSettings.ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ValidationError(LocalizationKeys.InternetUnavailable));

                return await func(ct);
            }
            catch (Exception ex)
            {
                if (ct.IsCancellationRequested)
                    return new OperationResult<TResult>(new OperationCanceledException());

                var available = AppSettings.ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ValidationError(LocalizationKeys.InternetUnavailable));

                return new OperationResult<TResult>(ex);
            }
        }

        protected static async Task<OperationResult<TResult>> TryRunTask<T1, TResult>(Func<T1, CancellationToken, Task<OperationResult<TResult>>> func, CancellationToken ct, T1 param1)
        {
            try
            {
                var available = AppSettings.ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ValidationError(LocalizationKeys.InternetUnavailable));

                return await func(param1, ct);
            }
            catch (Exception ex)
            {
                if (ct.IsCancellationRequested)
                    return new OperationResult<TResult>(new OperationCanceledException());

                var available = AppSettings.ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ValidationError(LocalizationKeys.InternetUnavailable));

                return new OperationResult<TResult>(ex);
            }
        }

        protected static async Task<OperationResult<TResult>> TryRunTask<T1, T2, TResult>(Func<T1, T2, CancellationToken, Task<OperationResult<TResult>>> func, CancellationToken ct, T1 param1, T2 param2)
        {
            try
            {
                var available = AppSettings.ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ValidationError(LocalizationKeys.InternetUnavailable));

                return await func(param1, param2, ct);
            }
            catch (Exception ex)
            {
                if (ct.IsCancellationRequested)
                    return new OperationResult<TResult>(new OperationCanceledException());

                var available = AppSettings.ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ValidationError(LocalizationKeys.InternetUnavailable));

                return new OperationResult<TResult>(ex);
            }
        }


        protected static async Task<Exception> TryRunTask(Func<CancellationToken, Task<Exception>> func, CancellationToken ct)
        {
            try
            {
                var available = AppSettings.ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new ValidationError(LocalizationKeys.InternetUnavailable);

                return await func(ct);
            }
            catch (Exception ex)
            {
                if (ct.IsCancellationRequested)
                    return new OperationCanceledException();

                var available = AppSettings.ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new ValidationError(LocalizationKeys.InternetUnavailable);

                return ex;
            }
        }

        protected static async Task<Exception> TryRunTask<T1>(Func<T1, CancellationToken, Task<Exception>> func, CancellationToken ct, T1 param1)
        {
            try
            {
                var available = AppSettings.ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new ValidationError(LocalizationKeys.InternetUnavailable);

                return await func(param1, ct);
            }
            catch (Exception ex)
            {
                if (ct.IsCancellationRequested)
                    return new OperationCanceledException();

                var available = AppSettings.ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new ValidationError(LocalizationKeys.InternetUnavailable);

                return ex;
            }
        }

        protected static async Task<Exception> TryRunTask<T1, T2>(Func<T1, T2, CancellationToken, Task<Exception>> func, CancellationToken ct, T1 param1, T2 param2)
        {
            try
            {
                var available = AppSettings.ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new ValidationError(LocalizationKeys.InternetUnavailable);

                return await func(param1, param2, ct);
            }
            catch (Exception ex)
            {
                if (ct.IsCancellationRequested)
                    return new OperationCanceledException();

                var available = AppSettings.ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new ValidationError(LocalizationKeys.InternetUnavailable);

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
