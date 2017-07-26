using System.Linq;
using Steepshot.Core;
using Sweetshot.Library.HttpClient;
using Steepshot.Core.Authority;
using Steepshot.Data;

namespace Steepshot
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
            string serverUrl;
            if (Chain == KnownChains.Steem)
                serverUrl = isDev ? "https://qa.steepshot.org/api/v1/" : "https://steepshot.org/api/v1/";
            else
                serverUrl = isDev ? "https://qa.golos.steepshot.org/api/v1/" : "https://golos.steepshot.org/api/v1/";

            _apiClient = new SteepshotApiClient(serverUrl);
        }

        public static void SwitchChain(KnownChains chain)
        {
            if (Chain == chain && _apiClient != null)
                return;

            Chain = chain;
            string serverUrl;
            if (chain == KnownChains.Steem)
                serverUrl = User.IsDev ? "https://qa.steepshot.org/api/v1/" : "https://steepshot.org/api/v1/";
            else
                serverUrl = User.IsDev ? "https://qa.golos.steepshot.org/api/v1/" : "https://golos.steepshot.org/api/v1/";

            _apiClient = new SteepshotApiClient(serverUrl);
        }
    }
}