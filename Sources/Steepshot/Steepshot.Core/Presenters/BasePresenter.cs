using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Cryptography.ECDSA;
using Ditch;
using Steepshot.Core.Authority;
using Steepshot.Core.HttpClient;
using Steepshot.Core.Utils;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Services;

namespace Steepshot.Core.Presenters
{
    public abstract class BasePresenter
    {
        protected static readonly ISteepshotApiClient Api;
        private static readonly Dictionary<string, double> CurencyConvertationDic;
        private static readonly CultureInfo CultureInfo;
        private static IConnectionService _connectionService;
        protected static IConnectionService ConnectionService => _connectionService ?? (_connectionService = AppSettings.Container.Resolve<IConnectionService>());
        public static bool ShouldUpdateProfile;
        public static event Action<string> OnAllert;

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
        private static Timer ReconectTimer;

        static BasePresenter()
        {
            CultureInfo = CultureInfo.InvariantCulture;
            User = new User();
            User.Load();
            Chain = User.Chain;
            //TODO:KOA: endpoint for CurencyConvertation needed
            CurencyConvertationDic = new Dictionary<string, double> { { "GBG", 2.4645 }, { "SBD", 1 } };

            Api = new DitchApi();
            TryConnect(Chain, AppSettings.IsDev);
            // static constructor initialization.
            Task.Run(() =>
            {
                var init = new Secp256k1Manager();
            });
        }

        private static async Task TryConnect(KnownChains chain, bool isDev)
        {
            if (ReconectTimer != null)
            {
                ReconectTimer.Dispose();
                ReconectTimer = null;
            }

            var isConnected = ConnectionService.IsConnectionAvailable();
            isConnected = await Api.Connect(chain, isDev, isConnected);
            if (!isConnected)
            {
                OnAllert?.Invoke(Localization.Errors.EnableConnectToBlockchain);
                ReconectTimer = new Timer(TryReconect, null, 0, 5000);
            }
        }

        private static void TryReconect(object state)
        {
            var isConnected = ConnectionService.IsConnectionAvailable();
            if (!isConnected)
            {
                OnAllert?.Invoke(Localization.Errors.EnableConnectToBlockchain);
                return;
            }

            ReconectTimer.Change(int.MaxValue, 5000);
            isConnected = Api.TryReconnectChain(Chain);
            if (!isConnected)
            {
                OnAllert?.Invoke(Localization.Errors.EnableConnectToBlockchain);
                ReconectTimer.Change(5000, 5000);
            }
            else
            {
                OnAllert?.Invoke(string.Empty);
                ReconectTimer.Dispose();
            }
        }

        public static async Task SwitchChain(bool isDev)
        {
            if (AppSettings.IsDev == isDev)
                return;

            AppSettings.IsDev = isDev;
            await TryConnect(Chain, isDev);
        }

        public static async Task SwitchChain(UserInfo userInfo)
        {
            if (Chain == userInfo.Chain)
                return;

            User.SwitchUser(userInfo);

            Chain = userInfo.Chain;
            await TryConnect(userInfo.Chain, AppSettings.IsDev);
        }

        public static async Task SwitchChain(KnownChains chain)
        {
            if (Chain == chain)
                return;

            Chain = chain;
            await TryConnect(chain, AppSettings.IsDev);
        }

        public static string ToFormatedCurrencyString(Money value, string postfix = null)
        {
            var dVal = value.ToDouble();
            if (!string.IsNullOrEmpty(value.Currency) && CurencyConvertationDic.ContainsKey(value.Currency))
                dVal *= CurencyConvertationDic[value.Currency];
            return $"{Currency} {dVal.ToString("F", CultureInfo)}{(string.IsNullOrEmpty(postfix) ? string.Empty : " ")}{postfix}";
        }

        #region TryRunTask

        protected async Task<OperationResult<TResult>> TryRunTask<TResult>(Func<CancellationToken, Task<OperationResult<TResult>>> func, CancellationToken ct)
        {
            try
            {
                var available = ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult> { Errors = new List<string> { Localization.Errors.InternetUnavailable } };

                return await func(ct);
            }
            catch (OperationCanceledException)
            {
                // to do nothing
            }
            catch (ApplicationExceptionBase ex)
            {
                var opr = new OperationResult<TResult>();
                opr.Errors.Add(ex.Message);
                return opr;
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
            return null;
        }

        protected async Task<OperationResult<TResult>> TryRunTask<T1, TResult>(Func<CancellationToken, T1, Task<OperationResult<TResult>>> func, CancellationToken ct, T1 param1)
        {
            try
            {
                var available = ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult> { Errors = new List<string> { Localization.Errors.InternetUnavailable } };

                return await func(ct, param1);
            }
            catch (OperationCanceledException)
            {
                // to do nothing
            }
            catch (ApplicationExceptionBase ex)
            {
                var opr = new OperationResult<TResult>();
                opr.Errors.Add(ex.Message);
                return opr;
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
            return null;
        }

        protected async Task<OperationResult<TResult>> TryRunTask<T1, T2, TResult>(Func<CancellationToken, T1, T2, Task<OperationResult<TResult>>> func, CancellationToken ct, T1 param1, T2 param2)
        {
            try
            {
                var available = ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult> { Errors = new List<string> { Localization.Errors.InternetUnavailable } };

                return await func(ct, param1, param2);
            }
            catch (OperationCanceledException)
            {
                // to do nothing
            }
            catch (ApplicationExceptionBase ex)
            {
                var opr = new OperationResult<TResult>();
                opr.Errors.Add(ex.Message);
                return opr;
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
            return null;
        }

        protected async Task<List<string>> TryRunTask(Func<CancellationToken, Task<List<string>>> func, CancellationToken ct)
        {
            try
            {
                var available = ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new List<string> { Localization.Errors.InternetUnavailable };

                return await func(ct);
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
            return null;
        }

        protected async Task<List<string>> TryRunTask<T1>(Func<CancellationToken, T1, Task<List<string>>> func, CancellationToken ct, T1 param1)
        {
            try
            {
                var available = ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new List<string> { Localization.Errors.InternetUnavailable };

                return await func(ct, param1);
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
            return null;
        }

        protected async Task<List<string>> TryRunTask<T1, T2>(Func<CancellationToken, T1, T2, Task<List<string>>> func, CancellationToken ct, T1 param1, T2 param2)
        {
            try
            {
                var available = ConnectionService.IsConnectionAvailable();
                if (!available)
                    return new List<string> { Localization.Errors.InternetUnavailable };

                return await func(ct, param1, param2);
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
            return null;
        }

        #endregion
    }
}
