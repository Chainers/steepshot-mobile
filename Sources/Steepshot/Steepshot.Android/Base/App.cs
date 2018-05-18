using Android.App;
using Android.Runtime;
using Autofac;
using Square.Picasso;
using Steepshot.Core.Authority;
using Steepshot.Core.Localization;
using Steepshot.Core.Sentry;
using Steepshot.Core.Services;
using Steepshot.Core.Utils;
using Steepshot.Services;
using Steepshot.Utils;
using System;

namespace Steepshot.Base
{
    [Application]
    public class App : Application
    {
        public App(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        protected static LruCache Cache;
        public override void OnCreate()
        {
            base.OnCreate();
            InitIoC(Assets);
            InitPicassoCache();
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

        public static void InitIoC(Android.Content.Res.AssetManager assetManagerssets)
        {
            if (AppSettings.Container == null)
            {
                var builder = new ContainerBuilder();
                var saverService = new SaverService();
                var dataProvider = new DataProvider(saverService);
                var appInfo = new AppInfo();
                var assetsHelper = new AssetsHelper(assetManagerssets);
                var connectionService = new ConnectionService();

                var localization = dataProvider.SelectLocalization("en-us") ?? assetsHelper.GetLocalization("en-us");
                var localizationManager = new LocalizationManager(localization);

                builder.RegisterInstance(assetsHelper).As<IAssetsHelper>().SingleInstance();
                builder.RegisterInstance(appInfo).As<IAppInfo>().SingleInstance();
                builder.RegisterInstance(saverService).As<ISaverService>().SingleInstance();
                builder.RegisterInstance(dataProvider).As<IDataProvider>().SingleInstance();
                builder.RegisterInstance(connectionService).As<IConnectionService>().SingleInstance();
                builder.RegisterInstance(connectionService).As<IConnectionService>().SingleInstance();
                builder.RegisterInstance(localizationManager).As<LocalizationManager>().SingleInstance();
                var configInfo = assetsHelper.GetConfigInfo();
                var reporterService = new ReporterService(appInfo, configInfo.RavenClientDsn);
                builder.RegisterInstance(reporterService).As<IReporterService>().SingleInstance();
                AppSettings.Container = builder.Build();
            }
        }
    }
}