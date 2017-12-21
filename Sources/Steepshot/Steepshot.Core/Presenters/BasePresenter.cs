using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Cryptography.ECDSA;
using Ditch.Core;
using Steepshot.Core.Authority;
using Steepshot.Core.Errors;
using Steepshot.Core.HttpClient;
using Steepshot.Core.Utils;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Services;

namespace Steepshot.Core.Presenters
{
    public abstract class BasePresenter
    {
        private static readonly Dictionary<string, double> CurencyConvertationDic;
        private static readonly CultureInfo CultureInfo;
        protected static readonly ISteepshotApiClient Api;

        private static readonly object CtsSync = new object();
        private static CancellationTokenSource _reconecTokenSource;
        private static IConnectionService _connectionService;

        public static IConnectionService ConnectionService => _connectionService ?? (_connectionService = AppSettings.ConnectionService);
        public static ProfileUpdateType ProfileUpdateType = ProfileUpdateType.None;
        public static event Action<string> OnAllert;

        protected CancellationTokenSource OnDisposeCts;

        public static string Currency
        {
            get
            {
                if (AppSettings.AppInfo.GetPlatform() == "iOS")
                    return "SBD";
                return Chain == KnownChains.Steem ? "$" : "₽";
            }
        }
        public static User User { get; set; }
        public static KnownChains Chain { get; set; }

        static BasePresenter()
        {
            CultureInfo = CultureInfo.InvariantCulture;
            User = new User();
            User.Load();
            Chain = User.Chain;
            //TODO:KOA: endpoint for CurencyConvertation needed
            CurencyConvertationDic = new Dictionary<string, double> { { "GBG", 2.4645 }, { "SBD", 1 } };

            Api = new SteepshotApiClient();

            var ts = GetReconectToken();
            Api.InitConnector(Chain, AppSettings.IsDev, ts);
            TryRunTask(TryСonect, ts);
            // static constructor initialization.
            Task.Run(() =>
            {
                var init = new Secp256k1Manager();
            });
        }

        protected BasePresenter()
        {
            OnDisposeCts = new CancellationTokenSource();
        }


        private static Task<ErrorBase> TryСonect(CancellationToken token)
        {
            return Task.Run(async () =>
            {
                do
                {
                    token.ThrowIfCancellationRequested();

                    var isConnected = Api.TryReconnectChain(token);
                    if (!isConnected)
                        OnAllert?.Invoke(Localization.Errors.EnableConnectToBlockchain);
                    else
                        return (ErrorBase)null;

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

            Api.InitConnector(Chain, isDev, ts);
            await TryRunTask(TryСonect, ts);
        }

        public static async Task SwitchChain(UserInfo userInfo)
        {
            if (Chain == userInfo.Chain)
                return;

            User.SwitchUser(userInfo);

            Chain = userInfo.Chain;

            var ts = GetReconectToken();
            Api.InitConnector(userInfo.Chain, AppSettings.IsDev, ts);
            await TryRunTask(TryСonect, ts);
        }

        public static async Task SwitchChain(KnownChains chain)
        {
            if (Chain == chain)
                return;

            Chain = chain;

            var ts = GetReconectToken();
            Api.InitConnector(chain, AppSettings.IsDev, ts);
            await TryRunTask(TryСonect, ts);
        }

        public static string ToFormatedCurrencyString(Asset value, string postfix = null)
        {
            var dVal = value.ToDouble();
            if (!string.IsNullOrEmpty(value.Currency) && CurencyConvertationDic.ContainsKey(value.Currency))
                dVal *= CurencyConvertationDic[value.Currency];
            return $"{Currency} {dVal.ToString("F", CultureInfo)}{(string.IsNullOrEmpty(postfix) ? string.Empty : " ")}{postfix}";
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

        #region TryRunTask

        protected static async Task<OperationResult<TResult>> TryRunTask<TResult>(Func<CancellationToken, Task<OperationResult<TResult>>> func, CancellationToken ct)
        {
            try
            {
                var available = ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ApplicationError(Localization.Errors.InternetUnavailable));

                return await func(ct);
            }
            catch (OperationCanceledException)
            {
                return new OperationResult<TResult>(new TaskCanceledError());
            }
            catch (ApplicationExceptionBase ex)
            {
                return new OperationResult<TResult>(new ApplicationError(ex.Message));
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
                return new OperationResult<TResult>(new ApplicationError(Localization.Errors.UnknownError));
            }
        }

        protected static async Task<OperationResult<TResult>> TryRunTask<T1, TResult>(Func<CancellationToken, T1, Task<OperationResult<TResult>>> func, CancellationToken ct, T1 param1)
        {
            try
            {
                var available = ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ApplicationError(Localization.Errors.InternetUnavailable));

                return await func(ct, param1);
            }
            catch (OperationCanceledException)
            {
                return new OperationResult<TResult>(new TaskCanceledError());
            }
            catch (ApplicationExceptionBase ex)
            {
                return new OperationResult<TResult>(new ApplicationError(ex.Message));
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
                return new OperationResult<TResult>(new ApplicationError(Localization.Errors.UnknownError));
            }
        }

        protected static async Task<OperationResult<TResult>> TryRunTask<T1, T2, TResult>(Func<CancellationToken, T1, T2, Task<OperationResult<TResult>>> func, CancellationToken ct, T1 param1, T2 param2)
        {
            try
            {
                var available = ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ApplicationError(Localization.Errors.InternetUnavailable));

                return await func(ct, param1, param2);
            }
            catch (OperationCanceledException)
            {
                return new OperationResult<TResult>(new TaskCanceledError());
            }
            catch (ApplicationExceptionBase ex)
            {
                return new OperationResult<TResult>(new ApplicationError(ex.Message));
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
                return new OperationResult<TResult>(new ApplicationError(Localization.Errors.UnknownError));
            }
        }


        protected static async Task<ErrorBase> TryRunTask(Func<CancellationToken, Task<ErrorBase>> func, CancellationToken ct)
        {
            try
            {
                var available = ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new ApplicationError(Localization.Errors.InternetUnavailable);

                return await func(ct);
            }
            catch (OperationCanceledException)
            {
                return new TaskCanceledError();
            }
            catch (ApplicationExceptionBase ex)
            {
                return new ApplicationError(ex.Message);
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
                return new ApplicationError(Localization.Errors.UnknownError);
            }
        }

        protected static async Task<ErrorBase> TryRunTask<T1>(Func<CancellationToken, T1, Task<ErrorBase>> func, CancellationToken ct, T1 param1)
        {
            try
            {
                var available = ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new ApplicationError(Localization.Errors.InternetUnavailable);

                return await func(ct, param1);
            }
            catch (OperationCanceledException)
            {
                return new TaskCanceledError();
            }
            catch (ApplicationExceptionBase ex)
            {
                return new ApplicationError(ex.Message);
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
                return new ApplicationError(Localization.Errors.UnknownError);
            }
        }

        protected static async Task<ErrorBase> TryRunTask<T1, T2>(Func<CancellationToken, T1, T2, Task<ErrorBase>> func, CancellationToken ct, T1 param1, T2 param2)
        {
            try
            {
                var available = ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new ApplicationError(Localization.Errors.InternetUnavailable);

                return await func(ct, param1, param2);
            }
            catch (OperationCanceledException)
            {
                return new TaskCanceledError();
            }
            catch (ApplicationExceptionBase ex)
            {
                return new ApplicationError(ex.Message);
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
                return new ApplicationError(Localization.Errors.UnknownError);
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
