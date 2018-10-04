using Android.App;
using Android.Runtime;
using Autofac;
using Square.Picasso;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;
using Steepshot.Services;
using Steepshot.Utils;
using System;
using System.Linq;
using System.Threading;
using Android.Content;
using Com.OneSignal;
using Com.OneSignal.Abstractions;
using Newtonsoft.Json;
using Steepshot.Activity;
using Steepshot.Core;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Sentry;

namespace Steepshot.Base
{
    [Application]
    public class App : Application
    {
        public static Square.Picasso.LruCache Cache;

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
            AppSettings.UpdateLocalizationAsync();
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
                Cache = new Square.Picasso.LruCache(this);
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
                builder.RegisterInstance(assetManagerssets).As<Android.Content.Res.AssetManager>().SingleInstance();

                builder.RegisterType<SaverService>().As<ISaverService>().SingleInstance();
                builder.RegisterType<AppInfo>().As<IAppInfo>().SingleInstance();
                builder.RegisterType<ConnectionService>().As<IConnectionService>().SingleInstance();
                builder.RegisterType<AssetHelper>().As<IAssetHelper>().SingleInstance();
                builder.RegisterType<UserManager>().As<UserManager>().SingleInstance();
                builder.RegisterType<User>().As<User>().SingleInstance();
                builder.RegisterType<ConfigManager>().As<ConfigManager>().SingleInstance();
                builder.RegisterType<ExtendedHttpClient>().As<ExtendedHttpClient>().SingleInstance();
                builder.RegisterType<LogService>().As<ILogService>().SingleInstance();
                builder.RegisterType<LocalizationManager>().As<LocalizationManager>().SingleInstance();
                builder.RegisterType<SteepshotClient>().As<SteepshotClient>().SingleInstance();
                
                AppSettings.RegisterPresenter(builder);
                AppSettings.RegisterFacade(builder);
                AppSettings.Container = builder.Build();
            }
        }
    }
}