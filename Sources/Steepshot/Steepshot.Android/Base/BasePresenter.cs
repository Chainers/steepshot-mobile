using Steepshot.Core;
using Steepshot.Core.Authority;
using Steepshot.Core.HttpClient;
using Steepshot.Core.Utils;
using Sweetshot.Library.HttpClient;

namespace Steepshot.Base
{
    public class BasePresenter
    {
        private static ISteepshotApiClient _apiClient;
        public static string AppVersion { get; set; }
        public static string Currency => Chain == KnownChains.Steem ? "$" : "₽";

        public static bool ShouldUpdateProfile;
        public static User User { get; set; }
        public static KnownChains Chain { get; set; }

        protected IBaseView View;

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
        }

        public BasePresenter(IBaseView view)
        {
            View = view;
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
            if (isDev)
            {
                _apiClient = new DitchApi(chain, isDev);
            }
            else
            {
                _apiClient = new SteepshotApiClient(chain, isDev);
            }
        }
    }
}