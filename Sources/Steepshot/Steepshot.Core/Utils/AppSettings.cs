using Autofac;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Services;

namespace Steepshot.Core.Utils
{
    public static class AppSettings
    {
        public static ProfileUpdateType ProfileUpdateType = ProfileUpdateType.None;

        public static IContainer Container { get; set; }

        private static ILogService _log;
        public static ILogService Logger => _log ?? (_log = Container.Resolve<ILogService>());

        private static ISaverService _saverService;
        public static ISaverService SaverService => _saverService ?? (_saverService = Container.Resolve<ISaverService>());

        private static IAppInfo _appInfo;
        public static IAppInfo AppInfo => _appInfo ?? (_appInfo = Container.Resolve<IAppInfo>());

        private static IConnectionService _connectionService;
        public static IConnectionService ConnectionService => _connectionService ?? (_connectionService = Container.Resolve<IConnectionService>());

        private static UserManager _dataProvider;
        public static UserManager DataProvider => _dataProvider ?? (_dataProvider = Container.Resolve<UserManager>());

        private static IAssetHelper _assetHelper;
        public static IAssetHelper AssetHelper => _assetHelper ?? (_assetHelper = Container.Resolve<IAssetHelper>());

        private static LocalizationManager _localizationManager;
        public static LocalizationManager LocalizationManager => _localizationManager ?? (_localizationManager = Container.Resolve<LocalizationManager>());

        private static ConfigManager _configManager;
        public static ConfigManager ConfigManager => _configManager ?? (_configManager = Container.Resolve<ConfigManager>());

        private static User _user;
        public static User User
        {
            get
            {
                if (_user == null)
                {
                    _user = new User();
                    _user.Load();
                }
                return _user;
            }
        }

        public static bool IsDev
        {
            get => SaverService.Get<bool>("isdev");
            set => SaverService.Save("isdev", value);
        }

        public static string VersionName
        {
            get => SaverService.Get<char>("lastversionname").ToString();
            set => SaverService.Save("lastversionname", value);
        }
    }
}
