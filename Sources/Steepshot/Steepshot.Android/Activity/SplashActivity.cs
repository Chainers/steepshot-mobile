using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Steepshot.Base;
using Steepshot.Core.Utils;
using Steepshot.Presenter;
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

            //CrashManager.Register(this, "fc38d51000bc469a8451c722528d4c55");
            //Toast.MakeText(this, string.Format("Alpha release. Version {0}",
            BasePresenter.AppVersion = PackageManager.GetPackageInfo(PackageName, 0).VersionName;
            //var _dir = new Java.IO.File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures), "Steepshot");
            //Picasso p = new Picasso.Builder(this).Downloader(new OkHttpDownloader(_dir, 1073741824)).Build();
            //Picasso.SetSingletonInstance(p);

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Reporter.SendCrash((Exception)e.ExceptionObject, BasePresenter.User.Login, BasePresenter.AppVersion);
            };

            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                Reporter.SendCrash(e.Exception, BasePresenter.User.Login, BasePresenter.AppVersion);
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
