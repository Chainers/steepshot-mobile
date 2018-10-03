using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Facades;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;

namespace Steepshot.Core.Utils
{
    public static class AppSettings
    {
        public static ProfileUpdateType ProfileUpdateType = ProfileUpdateType.None;

        public static IContainer Container { get; set; }

        public static ILogService Logger => Container.Resolve<ILogService>();

        public static ISaverService SaverService => Container.Resolve<ISaverService>();

        public static IAppInfo AppInfo => Container.Resolve<IAppInfo>();

        public static IConnectionService ConnectionService => Container.Resolve<IConnectionService>();

        public static UserManager DataProvider => Container.Resolve<UserManager>();

        public static IAssetHelper AssetHelper => Container.Resolve<IAssetHelper>();

        public static LocalizationManager LocalizationManager => Container.Resolve<LocalizationManager>();

        public static ConfigManager ConfigManager => Container.Resolve<ConfigManager>();

        private static User _user;
        public static User User
        {
            get
            {
                if (_user == null)
                {
                    _user = Container.Resolve<User>();
                    _user.Load();
                }
                return _user;
            }
        }

        public static ExtendedHttpClient ExtendedHttpClient => Container.Resolve<ExtendedHttpClient>();

        private static SteepshotClient _steepshotClient;
        public static SteepshotClient SteepshotClient => _steepshotClient ?? (_steepshotClient = new SteepshotClient(ExtendedHttpClient));

        private static SteepshotApiClient _steepshotSteemClient;
        public static SteepshotApiClient SteepshotSteemClient => _steepshotSteemClient ?? (_steepshotSteemClient = new SteepshotApiClient(ExtendedHttpClient, Logger, User.IsDev ? Constants.SteemUrlQa : Constants.SteemUrl));

        private static SteepshotApiClient _steepshotGolosClient;
        public static SteepshotApiClient SteepshoGolostClient => _steepshotGolosClient ?? (_steepshotGolosClient = new SteepshotApiClient(ExtendedHttpClient, Logger, User.IsDev ? Constants.GolosUrlQa : Constants.GolosUrl));

        private static SteemClient _steemClient;
        public static SteemClient SteemClient => _steemClient ?? (_steemClient = new SteemClient(ExtendedHttpClient, Logger, ConfigManager));

        private static GolosClient _golosClient;
        public static GolosClient GolosClient => _golosClient ?? (_golosClient = new GolosClient(ExtendedHttpClient, Logger, ConfigManager));


        private static KnownChains? _mainChain;
        public static KnownChains MainChain
        {
            get => (_mainChain ?? (_mainChain = User.Chain)).Value;
            set => _mainChain = value;
        }

        public static Task UpdateLocalizationAsync()
        {
            return LocalizationManager.UpdateAsync(ExtendedHttpClient, CancellationToken.None);
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

        public static void SetDev(bool isDev)
        {
            User.IsDev = isDev;
            User.IsDev = isDev;
            _steepshotSteemClient = null;
            _steepshotGolosClient = null;
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

        public static void RegisterPresenter(ContainerBuilder builder)
        {
            builder.RegisterType<CommentsPresenter>();
            builder.RegisterType<CreateAccountPresenter>();
            builder.RegisterType<FeedPresenter>();
            builder.RegisterType<PostDescriptionPresenter>();
            builder.RegisterType<PreSearchPresenter>();
            builder.RegisterType<PreSignInPresenter>();
            builder.RegisterType<PreSignInPresenter>();
            builder.RegisterType<PromotePresenter>();
            builder.RegisterType<SinglePostPresenter>();
            builder.RegisterType<TagsPresenter>();
            builder.RegisterType<TransferPresenter>();
            builder.RegisterType<UserFriendPresenter>();
            builder.RegisterType<UserProfilePresenter>();
            builder.RegisterType<WalletPresenter>();
        }

        public static void RegisterFacade(ContainerBuilder builder)
        {
            builder.RegisterType<SearchFacade>();
            builder.RegisterType<TransferFacade>();
            builder.RegisterType<TagPickerFacade>();
        }

        public static T GetPresenter<T>(KnownChains chains)
        {
            var args = new Parameter[2];

            if (chains == KnownChains.Golos)
            {
                args[0] = new TypedParameter(typeof(BaseDitchClient), GolosClient);
                args[1] = new TypedParameter(typeof(SteepshotApiClient), SteepshoGolostClient);
            }
            else
            {
                args[0] = new TypedParameter(typeof(BaseDitchClient), SteemClient);
                args[1] = new TypedParameter(typeof(SteepshotApiClient), SteepshotSteemClient);
            }

            return Container.Resolve<T>(args);
        }

        public static T GetFacade<T>(KnownChains chains, Parameter parameter)
        {
            var args = new[]
            {
                new ResolvedParameter((pi, ctx) => pi.Name.EndsWith("Presenter"),(pi, ctx) => GetPresenter(chains, pi.ParameterType)),
                parameter
            };
            return Container.Resolve<T>(args);
        }

        public static T GetFacade<T>(KnownChains chains)
        {
            var args = new ResolvedParameter((pi, ctx) => pi.Name.EndsWith("Presenter"), (pi, ctx) => GetPresenter(chains, pi.ParameterType));
            return Container.Resolve<T>(args);
        }

        private static object GetPresenter(KnownChains chains, Type type)
        {
            var args = new Parameter[2];

            if (chains == KnownChains.Golos)
            {
                args[0] = new TypedParameter(typeof(BaseDitchClient), GolosClient);
                args[1] = new TypedParameter(typeof(SteepshotApiClient), SteepshoGolostClient);
            }
            else
            {
                args[0] = new TypedParameter(typeof(BaseDitchClient), SteemClient);
                args[1] = new TypedParameter(typeof(SteepshotApiClient), SteepshotSteemClient);
            }

            return Container.Resolve(type, args);
        }
    }
}
