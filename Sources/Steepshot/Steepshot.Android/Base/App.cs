using Android.App;
using Android.Runtime;
using Autofac;
using Square.Picasso;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Com.OneSignal;
using Com.OneSignal.Abstractions;
using Newtonsoft.Json;
using Steepshot.Activity;
using Steepshot.Core;
using Steepshot.Core.Authorization;
using Steepshot.Core.Extensions;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Base
{
    [Application]
    public class App : Application
    {
        public static ProfileUpdateType ProfileUpdateType = ProfileUpdateType.None;

        public static Autofac.IContainer Container { get; private set; }
        public static ILogService Logger { get; private set; }
        public static LocalizationManager Localization { get; private set; }
        public static User User { get; private set; }
        public static SettingsManager SettingsManager { get; private set; }
        public static NavigationManager NavigationManager { get; private set; }
        public static IAppInfo AppInfo { get; private set; }
        public static KnownChains MainChain { get; set; }


        public static Square.Picasso.LruCache Cache;

        public App(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            AppDomain.CurrentDomain.UnhandledException -= OnCurrentDomainOnUnhandledExceptionAsync;
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainOnUnhandledExceptionAsync;

            TaskScheduler.UnobservedTaskException -= OnTaskSchedulerOnUnobservedTaskException;
            TaskScheduler.UnobservedTaskException += OnTaskSchedulerOnUnobservedTaskException;

            AndroidEnvironment.UnhandledExceptionRaiser -= OnUnhandledExceptionRaiser;
            AndroidEnvironment.UnhandledExceptionRaiser += OnUnhandledExceptionRaiser;


            InitIoC(Context.Assets);

            User = Container.GetUser();
            User.Load();
            MainChain = User.Chain;
            Logger = Container.GetLogger();
            Localization = Container.GetLocalizationManager();
            SettingsManager = Container.GetSettingsManager();
            NavigationManager = Container.GetNavigationManager();
            AppInfo = Container.GetAppInfo();

            InitPicassoCache();

            InitPushes();


            Localization.UpdateAsync(CancellationToken.None);
        }

        private void InitPushes()
        {
            OneSignal.Current.StartInit(User.Chain == KnownChains.Steem ? Constants.OneSignalSteemAppId : Constants.OneSignalGolosAppId)
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
                    Logger.ErrorAsync(e);
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
            if (Container == null)
            {
                var builder = new ContainerBuilder();

                builder.RegisterInstance(assetManagerssets)
                    .As<Android.Content.Res.AssetManager>()
                    .SingleInstance();
                builder.RegisterModule<Steepshot.Utils.IocModule>();

                Container = builder.Build();
            }
        }

        private async void OnTaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            await Logger.ErrorAsync(e.Exception);
            this.ShowAlert(LocalizationKeys.UnexpectedError, Android.Widget.ToastLength.Short);
        }

        private async void OnCurrentDomainOnUnhandledExceptionAsync(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex != null)
                ex = new Exception(e.ExceptionObject.ToString());

            if (e.IsTerminating)
                await Logger.FatalAsync(ex);
            else
                await Logger.ErrorAsync(ex);

            this.ShowAlert(ex, Android.Widget.ToastLength.Short);
        }

        private async void OnUnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            await Logger.ErrorAsync(e.Exception);

            this.ShowAlert(e.Exception, Android.Widget.ToastLength.Short);
        }
    }
}