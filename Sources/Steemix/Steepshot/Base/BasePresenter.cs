using System;
using Sweetshot.Library.HttpClient;

namespace Steepshot
{
    public class BasePresenter
    {
        private static SteepshotApiClient _apiClient;

        protected static SteepshotApiClient Api
        {
            get
            {
                if (_apiClient == null)
                    SwitchChain();
                return _apiClient;
            }
        }

        protected BaseView view;
        public BasePresenter(BaseView view)
        {
            this.view = view;
        }

        public static void SwitchChain()
        {
            string serverUrl;
            if (User.Chain == KnownChains.Steem)
                serverUrl = User.IsDev ? "https://qa.steepshot.org/api/v1/" : "https://steepshot.org/api/v1/";
            else
                serverUrl = User.IsDev ? "https://qa.golos.steepshot.org/api/v1/" : "https://golos.steepshot.org/api/v1/";

            _apiClient = new SteepshotApiClient(serverUrl);
        }
    }
}
