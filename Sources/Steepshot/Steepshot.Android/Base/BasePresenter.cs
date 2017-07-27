using Steepshot.Core;
using Steepshot.Core.Authority;
using Steepshot.Core.HttpClient;
using Steepshot.Data;
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

        protected BaseView view;

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
            User = new User(new DataProvider());
            User.Load();
            Chain = User.Chain;
        }

        public BasePresenter(BaseView view)
        {
            this.view = view;
        }

        public static void SwitchChain(bool isDev)
        {
            if (User.IsDev == isDev && _apiClient != null)
                return;

            User.IsDev = isDev;

            _apiClient = new SteepshotApiClient(Chain, isDev);
        }

        public static void SwitchChain(KnownChains chain)
        {
            if (Chain == chain && _apiClient != null)
                return;

            Chain = chain;

            _apiClient = new SteepshotApiClient(chain, User.IsDev);
        }
    }
}