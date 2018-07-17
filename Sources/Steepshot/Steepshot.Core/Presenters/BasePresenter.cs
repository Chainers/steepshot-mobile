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
        //private static readonly object CtsSync = new object();
        //private static CancellationTokenSource _reconecTokenSource;


        public SteepshotApiClient Api;

        protected CancellationTokenSource OnDisposeCts;
        

        public void SetClient(SteepshotApiClient steepshotApiClient)
        {
            TasksCancel();
            Api = steepshotApiClient;
        }

        //private static Task<Exception> TryСonect(CancellationToken token)
        //{
        //    return Task.Run(async () =>
        //    {
        //        await AppSettings.ConfigManager.Update(Api, token);
        //        do
        //        {
        //            token.ThrowIfCancellationRequested();

        //            var isConnected = Api.TryReconnectChain(token);
        //            if (!isConnected)
        //                OnAllert?.Invoke(LocalizationKeys.EnableConnectToBlockchain);
        //            else
        //                return (Exception)null;

        //            await Task.Delay(5000, token);

        //        } while (true);
        //    }, token);
        //}

        //public static async Task SwitchChain(bool isDev)
        //{
        //    if (AppSettings.IsDev == isDev)
        //        return;

        //    AppSettings.IsDev = isDev;

        //    SteemClient.SetDev(isDev);
        //}

        //public static async Task SwitchChain(UserInfo userInfo)
        //{
        //    if (Chain == userInfo.Chain)
        //        return;

        //    AppSettings.User.SwitchUser(userInfo);

        //    Chain = userInfo.Chain;

        //    var ts = GetReconectToken();
        //    Api.InitConnector(userInfo.Chain, AppSettings.IsDev);
        //    await TryRunTask(TryСonect, ts);
        //}

        //public static async Task SwitchChain(KnownChains chain)
        //{
        //    if (Chain == chain)
        //        return;

        //    Chain = chain;

        //    var ts = GetReconectToken();
        //    Api.InitConnector(chain, AppSettings.IsDev);
        //    await TryRunTask(TryСonect, ts);
        //}

        //private static CancellationToken GetReconectToken()
        //{
        //    CancellationToken ts;
        //    lock (CtsSync)
        //    {
        //        if (_reconecTokenSource != null && !_reconecTokenSource.IsCancellationRequested)
        //        {
        //            _reconecTokenSource.Cancel();
        //        }
        //        _reconecTokenSource = new CancellationTokenSource();
        //        ts = _reconecTokenSource.Token;
        //    }

        //    return ts;
        //}


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
