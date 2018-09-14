using System.Collections.Generic;
using Autofac;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
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


        #region Settings

        private const string AppSettingsKey = "AppSettings";

        private static AppSettingsModel _appSettingsModel;
        public static AppSettingsModel Settings => _appSettingsModel ?? (_appSettingsModel = SaverService.Get<AppSettingsModel>(AppSettingsKey) ?? new AppSettingsModel());

        public static void SaveSettings()
        {
            SaverService.Save(AppSettingsKey, _appSettingsModel);
        }

        #endregion

        #region Temp

        private const string AppTempKey = "AppTemp";

        private static Dictionary<string, string> _temp;
        public static Dictionary<string, string> Temp => _temp ?? (_temp = SaverService.Get<Dictionary<string, string>>(AppTempKey) ?? new Dictionary<string, string>());

        public static void SaveTemp()
        {
            SaverService.Save(AppTempKey, _temp);
        }

        #endregion

        #region Navigation

        private const string NavigationKey = "Navigation";

        private static Navigation _navigation;
        public static Navigation Navigation => _navigation ?? (_navigation = SaverService.Get<Navigation>(NavigationKey) ?? new Navigation());

        public static void SaveNavigation()
        {
            SaverService.Save(NavigationKey, _navigation);
        }

        public static void SetTabSettings(string tabKey, TabOptions value)
        {
            if (Navigation.TabSettings.ContainsKey(tabKey))
                Navigation.TabSettings[tabKey] = value;
            else
                Navigation.TabSettings.Add(tabKey, value);
        }

        public static TabOptions GetTabSettings(string tabKey)
        {
            if (!Navigation.TabSettings.ContainsKey(tabKey))
                Navigation.TabSettings.Add(tabKey, new TabOptions());

            return Navigation.TabSettings[tabKey];
        }

        public static int SelectedTab
        {
            get => Navigation.SelectedTab;
            set
            {
                Navigation.SelectedTab = value;
                SaveNavigation();
            }
        }

        #endregion
    }
}
