using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Autofac;
using Square.Picasso;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Authority;
using Steepshot.Core.Presenters;
using Steepshot.Core.Services;
using Steepshot.Core.Utils;
using Steepshot.Services;
using Steepshot.Utils;
using Android.Content;

namespace Steepshot.Activity
{
    [Activity(Label = Constants.Steepshot, MainLauncher = true, Icon = "@mipmap/launch_icon", ScreenOrientation = ScreenOrientation.Portrait, NoHistory = true)]
    [IntentFilter(new[] { Intent.ActionSend }, Categories = new[] { Intent.CategoryDefault }, Icon = "@drawable/logo_login", DataMimeType = "image/*")]
    public sealed class SplashActivity : BaseActivity
    {
        public static LruCache Cache;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (AppSettings.Container == null)
                Construct();

            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnTaskSchedulerOnUnobservedTaskException;

            if (Intent.ActionSend.Equals(Intent.Action) && Intent.Type != null)
            {
                Intent intent;
                if (BasePresenter.User.IsAuthenticated)
                {
                    intent = new Intent(Application.Context, typeof(PostDescriptionActivity));
                    var uri = (Android.Net.Uri)Intent.GetParcelableExtra(Intent.ExtraStream);
                    intent.PutExtra(PostDescriptionActivity.PhotoExtraPath, uri.ToString());
                }
                else
                {
                    intent = new Intent(this, typeof(PreSignInActivity));
                }
                StartActivity(intent);
            }
            else
            {
                StartActivity(BasePresenter.User.IsAuthenticated ? typeof(RootActivity) : typeof(GuestActivity));
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            AppDomain.CurrentDomain.UnhandledException -= OnCurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException -= OnTaskSchedulerOnUnobservedTaskException;
        }

        private void OnTaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            AppSettings.Reporter.SendCrash(e.Exception);
            this.ShowAlert(Localization.Errors.UnexpectedError, Android.Widget.ToastLength.Short);
        }

        private void OnCurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex != null)
                ex = new Exception(e.ExceptionObject.ToString());
            AppSettings.Reporter.SendCrash(ex);
            this.ShowAlert(Localization.Errors.UnexpectedError, Android.Widget.ToastLength.Short);
        }

        private void Construct()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(new AppInfo()).As<IAppInfo>().SingleInstance();
            builder.RegisterType<DataProvider>().As<IDataProvider>().SingleInstance();
            builder.RegisterInstance(new SaverService()).As<ISaverService>().SingleInstance();
            builder.RegisterInstance(new ConnectionService()).As<IConnectionService>().SingleInstance();
#if DEBUG
            builder.RegisterType<StubReporterService>().As<IReporterService>().SingleInstance();
#else
            builder.RegisterType<ReporterService>().As<IReporterService>().SingleInstance();
#endif

            var d = new Picasso.Builder(this);
            Cache = new LruCache(this);
            d.MemoryCache(Cache);
            Picasso.SetSingletonInstance(d.Build());

            AppSettings.Container = builder.Build();
        }
    }
}
