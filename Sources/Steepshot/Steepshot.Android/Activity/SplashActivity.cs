using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Autofac;
using Square.Picasso;
using Steepshot.Base;
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
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Construct();

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Reporter.SendCrash((Exception)e.ExceptionObject, BasePresenter.User.Login);
            };

            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                Reporter.SendCrash(e.Exception, BasePresenter.User.Login);
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

            builder.RegisterInstance(new AppInfo()).As<IAppInfo>();
            builder.RegisterType<DataProvider>().As<IDataProvider>();
            builder.RegisterInstance(new SaverService()).As<ISaverService>();
            builder.RegisterInstance(new ConnectionService()).As<IConnectionService>();

            Picasso.Builder d = new Picasso.Builder(this);
            Cache = new LruCache(this);
            d.MemoryCache(Cache);
            Picasso.SetSingletonInstance(d.Build());

            AppSettings.Container = builder.Build();
        }
    }
}
