using Android.App;
using Android.Runtime;
using Autofac;
using Square.Picasso;
using Steepshot.Core.Localization;
using Steepshot.Core.Services;
using Steepshot.Core.Utils;
using Steepshot.Services;
using Steepshot.Utils;
using System;
using System.Linq;
using Android.Content;
using Com.OneSignal;
using Com.OneSignal.Abstractions;
using Newtonsoft.Json;
using Steepshot.Activity;
using Steepshot.Core;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Sentry;

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

            InitPushes();
            AppSettings.LocalizationManager.Update(HttpClient);
        }

        private void InitPushes()
        {
            OneSignal.Current.StartInit(AppSettings.User.Chain == KnownChains.Steem ? Constants.OneSignalSteemAppId : Constants.OneSignalGolosAppId)
                .InFocusDisplaying(OSInFocusDisplayOption.None)
                .HandleNotificationOpened(OneSignalNotificationOpened)
                .EndInit();
        }

        private void OneSignalNotificationOpened(OSNotificationOpenedResult result)
        {
            var additionalData = result?.notification?.payload?.additionalData;
            if (additionalData?.Any() ?? false)
            {
                try
                {
                    var data = JsonConvert.SerializeObject(additionalData.ToDictionary(x => x.Key, x => x.Value.ToString()));
                    var intent = new Intent(this, typeof(RootActivity));
                    intent.PutExtra(RootActivity.NotificationData, data);
                    intent.SetFlags(ActivityFlags.ReorderToFront | ActivityFlags.NewTask);
                    StartActivity(intent);
                }
                catch (Exception e)
                {
                    AppSettings.Logger.ErrorAsync(e);
                }
            }
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
                HttpClient = new ExtendedHttpClient();

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
                var reporterService = new LogService(HttpClient, appInfo, configInfo.RavenClientDsn);
                builder.RegisterInstance(reporterService).As<ILogService>().SingleInstance();
                AppSettings.Container = builder.Build();

                MainChain = AppSettings.User.Chain;
                SteemClient = new SteepshotApiClient(HttpClient, KnownChains.Steem);
                GolosClient = new SteepshotApiClient(HttpClient, KnownChains.Golos);
            }
        }
    }
}