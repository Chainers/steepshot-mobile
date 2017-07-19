using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using System.Threading.Tasks;

namespace Steepshot
{
	[Activity(Label = "Steepshot", MainLauncher = true, Icon = "@mipmap/launch_icon", ScreenOrientation = ScreenOrientation.Portrait, NoHistory = true)]
	public class SplashActivity : BaseActivity, SplashView
	{
		SplashPresenter presenter;

		protected override void CreatePresenter()
		{
			presenter = new SplashPresenter(this);
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			//CrashManager.Register(this, "fc38d51000bc469a8451c722528d4c55");
			//Toast.MakeText(this, string.Format("Alpha release. Version {0}",
			User.AppVersion = PackageManager.GetPackageInfo(PackageName, 0).VersionName;
			//var _dir = new Java.IO.File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures), "SteepShot");
			//Picasso p = new Picasso.Builder(this).Downloader(new OkHttpDownloader(_dir, 1073741824)).Build();
			//Picasso.SetSingletonInstance(p);

			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				Reporter.SendCrash((Exception)e.ExceptionObject);
			};

			TaskScheduler.UnobservedTaskException += (sender, e) =>
			{
				Reporter.SendCrash(e.Exception);
			};

            if (presenter.IsGuest)
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
