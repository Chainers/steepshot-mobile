using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Ninject.Modules;
using Steepshot.Base;
using Steepshot.Core.Services;
using Steepshot.Core.Utils;
using Steepshot.Presenter;
using Steepshot.Services;
using Steepshot.View;

namespace Steepshot.Activity
{
    [Activity(Label = "Steepshot", MainLauncher = true, Icon = "@mipmap/launch_icon", ScreenOrientation = ScreenOrientation.Portrait, NoHistory = true)]
    public class SplashActivity : BaseActivity, ISplashView
    {
        SplashPresenter _presenter;

        protected override void CreatePresenter()
        {
            _presenter = new SplashPresenter(this);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

			var kernel = new Ninject.StandardKernel(new Bindings());
            AppSettings.Container = kernel;

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Reporter.SendCrash((Exception)e.ExceptionObject, BasePresenter.User.Login);
            };

            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
				Reporter.SendCrash(e.Exception, BasePresenter.User.Login);
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

	public class Bindings : NinjectModule
	{
		public override void Load()
		{
            Bind<IAppInfo>().To<AppInfo>().InSingletonScope();
		}
	}
}
