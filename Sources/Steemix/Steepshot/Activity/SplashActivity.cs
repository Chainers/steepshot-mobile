using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using HockeyApp.Android;
using Android.Widget;

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

            CrashManager.Register(this, "fc38d51000bc469a8451c722528d4c55");

            Toast.MakeText(this, string.Format("Alpha release. Version {0}",
                PackageManager.GetPackageInfo(PackageName,0).VersionName),ToastLength.Long).Show();

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
