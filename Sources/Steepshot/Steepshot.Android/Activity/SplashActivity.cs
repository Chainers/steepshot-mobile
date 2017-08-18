using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Autofac;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Authority;
using Steepshot.Core.Presenters;
using Steepshot.Core.Services;
using Steepshot.Core.Utils;
using Steepshot.Services;

namespace Steepshot.Activity
{
    [Activity(Label = "Steepshot", MainLauncher = true, Icon = "@mipmap/launch_icon", ScreenOrientation = ScreenOrientation.Portrait, NoHistory = true)]
    public class SplashActivity : BaseActivity
    {
        SplashPresenter _presenter;

        protected override void CreatePresenter()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(new AppInfo()).As<IAppInfo>();
            builder.RegisterType<Core.Authority.DataProvider>().As<IDataProvider>();
            builder.RegisterInstance(new SaverService()).As<ISaverService>();

            AppSettings.Container = builder.Build();
            _presenter = new SplashPresenter();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Reporter.SendCrash((Exception)e.ExceptionObject, Base.BasePresenter.User.Login);
            };

            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                Reporter.SendCrash(e.Exception, Base.BasePresenter.User.Login);
            };

            if (_presenter.IsGuest)
            {
                StartActivity(typeof(GuestActivity));
            }
            else
            {
                StartActivity(typeof(RootActivity));
            }
        }
    }
}
