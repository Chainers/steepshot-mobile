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

namespace Steepshot.Activity
{
    [Activity(Label = Constants.Steepshot, MainLauncher = true, Icon = "@mipmap/launch_icon", ScreenOrientation = ScreenOrientation.Portrait, NoHistory = true)]
    public class SplashActivity : BaseActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (AppSettings.Container == null)
                Construct();

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                AppSettings.Reporter.SendCrash((Exception)e.ExceptionObject);
            };

            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                AppSettings.Reporter.SendCrash(e.Exception);
            };

            bool isKeyValid = true;
            /*
            if (!_presenter.IsGuest)
            {
                isKeyValid = (await _presenter.SignIn(BasePresenter.User.Login, BasePresenter.User.UserInfo.PostingKey)).Success;
                if (!isKeyValid)
                {
                    BasePresenter.User.UserInfo.PostingKey = null;
                    Toast.MakeText(this, Localization.Errors.WrongPrivateKey, ToastLength.Long);
                }
            }*/
            StartActivity(BasePresenter.User.IsAuthenticated && isKeyValid ? typeof(RootActivity) : typeof(GuestActivity));
        }

        private void Construct()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(new AppInfo()).As<IAppInfo>().SingleInstance();
            builder.RegisterType<DataProvider>().As<IDataProvider>().SingleInstance();
            builder.RegisterInstance(new SaverService()).As<ISaverService>().SingleInstance();
            builder.RegisterInstance(new ConnectionService()).As<IConnectionService>().SingleInstance();
            builder.RegisterType<ReporterService>().As<IReporterService>().SingleInstance();

            Picasso.Builder d = new Picasso.Builder(this);
            Cache = new LruCache(this);
            d.MemoryCache(Cache);
            Picasso.SetSingletonInstance(d.Build());

            AppSettings.Container = builder.Build();
        }
    }
}
