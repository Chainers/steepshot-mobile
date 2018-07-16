using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Cryptography.ECDSA;
using Steepshot.Core.Authorization;
using Steepshot.Core.Errors;
using Steepshot.Core.HttpClient;
using Steepshot.Core.Utils;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Services;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Presenters
{
    public abstract class BasePresenter
    {
        private static readonly CultureInfo CultureInfo;
        public static readonly SteepshotApiClient Api;
        private static readonly Timer LazyLoadTimer;
        private static readonly object CtsSync;
        private static CancellationTokenSource _reconecTokenSource;
        private static IConnectionService _connectionService;

        protected CancellationTokenSource OnDisposeCts;

        public static ProfileUpdateType ProfileUpdateType = ProfileUpdateType.None;
        public static event Action<LocalizationKeys> OnAllert;


        public static IConnectionService ConnectionService => _connectionService ?? (_connectionService = AppSettings.ConnectionService);

        private static string Currency
        {
            get
            {
                return Chain == KnownChains.Steem ? "$" : "₽";
            }
        }

        public static KnownChains Chain { get; private set; }


        static BasePresenter()
        {
            CtsSync = new object();
            CultureInfo = CultureInfo.InvariantCulture;

            Chain = AppSettings.User.Chain;

            Api = new SteepshotApiClient();

            Api.InitConnector(Chain, AppSettings.IsDev);
            LazyLoadTimer = new Timer(Callback, null, 9000, Int32.MaxValue);
        }

        protected BasePresenter()
        {
            OnDisposeCts = new CancellationTokenSource();
        }


        private static void Callback(object state)
        {
            var ts = GetReconectToken();
            TryRunTask(TryСonect, ts);
            // static constructor initialization.
            var init = new Secp256K1Manager();
            AppSettings.LocalizationManager.Update(Api.Gateway);
            LazyLoadTimer.Dispose();
        }

        private static Task<Exception> TryСonect(CancellationToken token)
        {
            return Task.Run(async () =>
            {
                await AppSettings.ConfigManager.Update(Api.Gateway, Chain, token);
                do
                {
                    token.ThrowIfCancellationRequested();

                    var isConnected = Api.TryReconnectChain(token);
                    if (!isConnected)
                        OnAllert?.Invoke(LocalizationKeys.EnableConnectToBlockchain);
                    else
                        return (Exception)null;

                    await Task.Delay(5000, token);

                } while (true);
            }, token);
        }

        public static async Task SwitchChain(bool isDev)
        {
            if (AppSettings.IsDev == isDev)
                return;

            AppSettings.IsDev = isDev;

            var ts = GetReconectToken();

            Api.InitConnector(Chain, isDev);
            await TryRunTask(TryСonect, ts);
        }

        public static async Task SwitchChain(UserInfo userInfo)
        {
            if (Chain == userInfo.Chain)
                return;

            AppSettings.User.SwitchUser(userInfo);

            Chain = userInfo.Chain;

            var ts = GetReconectToken();
            Api.InitConnector(userInfo.Chain, AppSettings.IsDev);
            await TryRunTask(TryСonect, ts);
        }

        public static async Task SwitchChain(KnownChains chain)
        {
            if (Chain == chain)
                return;

            Chain = chain;

            var ts = GetReconectToken();
            Api.InitConnector(chain, AppSettings.IsDev);
            await TryRunTask(TryСonect, ts);
        }

        public static string ToFormatedCurrencyString(double value)
        {
            return $"{Currency} {value.ToString("F", CultureInfo)}";
        }

        private static CancellationToken GetReconectToken()
        {
            CancellationToken ts;
            lock (CtsSync)
            {
                if (_reconecTokenSource != null && !_reconecTokenSource.IsCancellationRequested)
                {
                    _reconecTokenSource.Cancel();
                }
                _reconecTokenSource = new CancellationTokenSource();
                ts = _reconecTokenSource.Token;
            }

            return ts;
        }


        public static async Task<OperationResult<object>> TrySubscribeForPushes(PushNotificationsModel model)
        {
            return await TryRunTask<PushNotificationsModel, object>(Api.SubscribeForPushes, CancellationToken.None, model);
        }


        #region TryRunTask

        protected static async Task<OperationResult<TResult>> TryRunTask<TResult>(Func<CancellationToken, Task<OperationResult<TResult>>> func, CancellationToken ct)
        {
            try
            {
                var available = ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ValidationError(LocalizationKeys.InternetUnavailable));

                return await func(ct);
            }
            catch (Exception ex)
            {
                if (ct.IsCancellationRequested)
                    return new OperationResult<TResult>(new OperationCanceledException());

                var available = ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ValidationError(LocalizationKeys.InternetUnavailable));

                return new OperationResult<TResult>(ex);
            }
        }

        protected static async Task<OperationResult<TResult>> TryRunTask<T1, TResult>(Func<T1, CancellationToken, Task<OperationResult<TResult>>> func, CancellationToken ct, T1 param1)
        {
            try
            {
                var available = ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ValidationError(LocalizationKeys.InternetUnavailable));

                return await func(param1, ct);
            }
            catch (Exception ex)
            {
                if (ct.IsCancellationRequested)
                    return new OperationResult<TResult>(new OperationCanceledException());

                var available = ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ValidationError(LocalizationKeys.InternetUnavailable));

                return new OperationResult<TResult>(ex);
            }
        }

        protected static async Task<OperationResult<TResult>> TryRunTask<T1, T2, TResult>(Func<T1, T2, CancellationToken, Task<OperationResult<TResult>>> func, CancellationToken ct, T1 param1, T2 param2)
        {
            try
            {
                var available = ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ValidationError(LocalizationKeys.InternetUnavailable));

                return await func(param1, param2, ct);
            }
            catch (Exception ex)
            {
                if (ct.IsCancellationRequested)
                    return new OperationResult<TResult>(new OperationCanceledException());

                var available = ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ValidationError(LocalizationKeys.InternetUnavailable));

                return new OperationResult<TResult>(ex);
            }
        }


        protected static async Task<Exception> TryRunTask(Func<CancellationToken, Task<Exception>> func, CancellationToken ct)
        {
            try
            {
                var available = ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new ValidationError(LocalizationKeys.InternetUnavailable);

                return await func(ct);
            }
            catch (Exception ex)
            {
                if (ct.IsCancellationRequested)
                    return new OperationCanceledException();

                var available = ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new ValidationError(LocalizationKeys.InternetUnavailable);

                return ex;
            }
        }

        protected static async Task<Exception> TryRunTask<T1>(Func<T1, CancellationToken, Task<Exception>> func, CancellationToken ct, T1 param1)
        {
            try
            {
                var available = ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new ValidationError(LocalizationKeys.InternetUnavailable);

                return await func(param1, ct);
            }
            catch (Exception ex)
            {
                if (ct.IsCancellationRequested)
                    return new OperationCanceledException();

                var available = ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new ValidationError(LocalizationKeys.InternetUnavailable);

                return ex;
            }
        }

        protected static async Task<Exception> TryRunTask<T1, T2>(Func<T1, T2, CancellationToken, Task<Exception>> func, CancellationToken ct, T1 param1, T2 param2)
        {
            try
            {
                var available = ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new ValidationError(LocalizationKeys.InternetUnavailable);

                return await func(param1, param2, ct);
            }
            catch (Exception ex)
            {
                if (ct.IsCancellationRequested)
                    return new OperationCanceledException();

                var available = ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new ValidationError(LocalizationKeys.InternetUnavailable);

                return ex;
            }
        }

        #endregion

        public void TasksCancel(bool andDispose = false)
        {
            if (!OnDisposeCts.IsCancellationRequested)
                OnDisposeCts.Cancel();

            OnDisposeCts = new CancellationTokenSource();
        }
    }
}
