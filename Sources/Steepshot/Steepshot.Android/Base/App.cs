using Android.App;
using Android.Runtime;
using Autofac;
using Square.Picasso;
using Steepshot.Core.Localization;
using Steepshot.Core.Sentry;
using Steepshot.Core.Services;
using Steepshot.Core.Utils;
using Steepshot.Services;
using Steepshot.Utils;
using System;
using Steepshot.Core;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;

namespace Steepshot.Base
{
    [Application]
    public class App : Application
    {
        public static LruCache Cache;
        public static ExtendedHttpClient HttpClient;
        public static SteepshotApiClient SteemClient;
        public static SteepshotApiClient GolosClient;

        public static KnownChains MainChain { get; set; } = KnownChains.Steem;


        public App(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }


        public override void OnCreate()
        {
            base.OnCreate();
            InitIoC(Context.Assets);
            InitPicassoCache();
            InitClients();
        }

        private void InitPicassoCache()
        {
            if (Cache == null)
            {
                Cache = new LruCache(this);
                var d = new Picasso.Builder(this);
                d.MemoryCache(Cache);
                Picasso.SetSingletonInstance(d.Build());
            }
        }

        private void InitIoC(Android.Content.Res.AssetManager assetManagerssets)
        {
            if (AppSettings.Container == null)
            {
                var builder = new ContainerBuilder();
                var saverService = new SaverService();
                var dataProvider = new UserManager(saverService);
                var appInfo = new AppInfo();
                var assetsHelper = new AssetHelper(assetManagerssets);
                var connectionService = new ConnectionService();

                var localizationManager = new LocalizationManager(saverService, assetsHelper);
                var configManager = new ConfigManager(saverService, assetsHelper);

                builder.RegisterInstance(assetsHelper).As<IAssetHelper>().SingleInstance();
                builder.RegisterInstance(appInfo).As<IAppInfo>().SingleInstance();
                builder.RegisterInstance(saverService).As<ISaverService>().SingleInstance();
                builder.RegisterInstance(dataProvider).As<UserManager>().SingleInstance();
                builder.RegisterInstance(connectionService).As<IConnectionService>().SingleInstance();
                builder.RegisterInstance(localizationManager).As<LocalizationManager>().SingleInstance();
                builder.RegisterInstance(configManager).As<ConfigManager>().SingleInstance();
                var configInfo = assetsHelper.GetConfigInfo();
                var reporterService = new ReporterService(appInfo, configInfo.RavenClientDsn);
                builder.RegisterInstance(reporterService).As<IReporterService>().SingleInstance();
                AppSettings.Container = builder.Build();
            }
        }

        private void InitClients()
        {
            MainChain = AppSettings.User.Chain;

            if (HttpClient == null)
            {
                HttpClient = new ExtendedHttpClient();
                SteemClient = new SteepshotApiClient(HttpClient, KnownChains.Steem);
                GolosClient = new SteepshotApiClient(HttpClient, KnownChains.Golos);

                AppSettings.LocalizationManager.Update(HttpClient);
            }
        }
    }
}