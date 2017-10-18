using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Cryptography.ECDSA;
using Ditch;
using Steepshot.Core.Authority;
using Steepshot.Core.HttpClient;
using Steepshot.Core.Utils;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Presenters
{
    public abstract class BasePresenter
    {
        protected static readonly ISteepshotApiClient Api;
        private static readonly Dictionary<string, double> CurencyConvertationDic;
        private static readonly CultureInfo CultureInfo;
        public static bool ShouldUpdateProfile;

        public static string AppVersion { get; set; }
        public static string Currency => Chain == KnownChains.Steem ? "$" : "₽";
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

#if DEBUG
            //_apiClient = new ApiPositiveStub();
            Api = new DitchApi();
            Api.Connect(Chain, AppSettings.IsDev);
#else
            _apiClient = new DitchApi();
#endif
            // static constructor initialization.
            var init = new Secp256k1Manager();
        }

        public static void SwitchChain(bool isDev)
        {
            if (AppSettings.IsDev == isDev)
                return;

            AppSettings.IsDev = isDev;
            Api.Connect(Chain, isDev);
        }

        public static void SwitchChain(UserInfo userInfo)
        {
            if (Chain == userInfo.Chain)
                return;

            User.SwitchUser(userInfo);

            Chain = userInfo.Chain;
            Api.Connect(userInfo.Chain, AppSettings.IsDev);
        }

        public static void SwitchChain(KnownChains chain)
        {
            if (Chain == chain)
                return;

            Chain = chain;
            Api.Connect(chain, AppSettings.IsDev);
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
