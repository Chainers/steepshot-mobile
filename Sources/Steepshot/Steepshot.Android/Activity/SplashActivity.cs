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
using Android.Content;

namespace Steepshot.Activity
{
    [Activity(Label = Constants.Steepshot, MainLauncher = true, Icon = "@mipmap/launch_icon", ScreenOrientation = ScreenOrientation.Portrait, NoHistory = true)]
    [IntentFilter(new[] { Intent.ActionSend }, Categories = new[] { Intent.CategoryDefault }, Icon = "@drawable/logo_login", DataMimeType = "image/*")]
    public class SplashActivity : BaseActivity
    {
        private static LruCache Cache;

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
            //TODO:KOA: There had to be WIF check
            /*
            if (!_presenter.IsGuest)
            {
                isKeyValid = (await _presenter.TrySignIn(BasePresenter.User.Login, BasePresenter.User.UserInfo.PostingKey)).Success;
                if (!isKeyValid)
                {
                    BasePresenter.User.UserInfo.PostingKey = null;
                    Toast.MakeText(this, Localization.Errors.WrongPrivateKey, ToastLength.Long);
                }
            }*/
            if (Intent.ActionSend.Equals(Intent.Action) && Intent.Type != null)
            {
                Intent intent = null;
                if (BasePresenter.User.IsAuthenticated && isKeyValid)
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
                StartActivity(BasePresenter.User.IsAuthenticated && isKeyValid ? typeof(RootActivity) : typeof(GuestActivity));
            }
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

            Picasso.Builder d = new Picasso.Builder(this);
            Cache = new LruCache(this);
            d.MemoryCache(Cache);
            Picasso.SetSingletonInstance(d.Build());

            AppSettings.Container = builder.Build();
        }
    }
}
