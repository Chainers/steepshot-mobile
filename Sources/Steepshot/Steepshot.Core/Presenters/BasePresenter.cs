using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Ditch;
using Steepshot.Core.Authority;
using Steepshot.Core.HttpClient;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public abstract class BasePresenter
    {
        private static ISteepshotApiClient _apiClient;
        public static string AppVersion { get; set; }
        public static string Currency => Chain == KnownChains.Steem ? "$" : "₽";
        private static readonly Dictionary<string, double> CurencyConvertationDic;
        private static readonly CultureInfo CultureInfo = CultureInfo.InvariantCulture;
        public static bool ShouldUpdateProfile;
        public static User User { get; set; }
        public static KnownChains Chain { get; set; }

        protected static ISteepshotApiClient Api
        {
            get
            {
                if (_apiClient == null)
                    SwitchChain(Chain);
                return _apiClient;
            }
        }

        static BasePresenter()
        {
            User = new User();
            User.Load();
            Chain = User.Chain;
            //TODO:KOA: endpoint for CurencyConvertation needed
            CurencyConvertationDic = new Dictionary<string, double> { { "GBG", 2.4645 }, { "SBD", 1 } };
        }

        public static void SwitchChain(bool isDev)
        {
            if (AppSettings.IsDev == isDev && _apiClient != null)
                return;

            AppSettings.IsDev = isDev;

            InitApiClient(Chain, isDev);
        }

        public static void SwitchChain(UserInfo userInfo)
        {
            if (Chain == userInfo.Chain && _apiClient != null)
                return;

            User.SwitchUser(userInfo);

            Chain = userInfo.Chain;
            InitApiClient(userInfo.Chain, AppSettings.IsDev);
        }

        public static void SwitchChain(KnownChains chain)
        {
            if (Chain == chain && _apiClient != null)
                return;

            Chain = chain;
            InitApiClient(chain, AppSettings.IsDev);
        }

        private static void InitApiClient(KnownChains chain, bool isDev)
        {
#if DEBUG
            //_apiClient = new ApiPositiveStub();
            _apiClient = new DitchApi(chain, isDev);
#else
            _apiClient = new DitchApi(chain, isDev);
#endif
        }

        public static string ToFormatedCurrencyString(Money value, string postfix = null)
        {
            var dVal = value.ToDouble();
            if (!string.IsNullOrEmpty(value.Currency) && CurencyConvertationDic.ContainsKey(value.Currency))
                dVal *= CurencyConvertationDic[value.Currency];
            return $"{Currency} {dVal.ToString("F", CultureInfo)}{(string.IsNullOrEmpty(postfix) ? string.Empty : " ")}{postfix}";
        }


        protected async Task<TResult> TryRunTask<T, TResult>(Func<T, Task<TResult>> func, T parameters)
        {
            try
            {
                return await func(parameters);
            }
            catch (OperationCanceledException)
            {
                // to do nothing
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
            return default(TResult);
        }


        protected async Task<TResult> TryRunTask<TResult>(Func<Task<TResult>> func)
        {
            try
            {
                return await func();
            }
            catch (OperationCanceledException)
            {
                // to do nothing
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
            return default(TResult);
        }
    }
}
