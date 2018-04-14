using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;
using Android.Content;
using Android.Runtime;
using Steepshot.Core.Localization;
using Steepshot.Fragment;
using Steepshot.Services;

namespace Steepshot.Activity
{
    [Activity(Label = Constants.Steepshot, MainLauncher = true, LaunchMode = LaunchMode.SingleTask, ScreenOrientation = ScreenOrientation.Portrait, NoHistory = true, Theme = "@style/SplashTheme")]
    [IntentFilter(new[] { Intent.ActionSend }, Categories = new[] { Intent.CategoryDefault }, Icon = "@mipmap/ic_launch_icon", DataMimeType = "image/*")]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable }, DataSchemes = new[] { "https", "http" }, DataHosts = new[] { "alpha.steepshot.io", "qa.alpha.steepshot.io" }, DataPathPrefixes = new[] { "/post", "/@" })]
    public sealed class SplashActivity : BaseActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            AppDomain.CurrentDomain.UnhandledException -= OnCurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException -= OnTaskSchedulerOnUnobservedTaskException;
            AndroidEnvironment.UnhandledExceptionRaiser -= OnUnhandledExceptionRaiser;

            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnTaskSchedulerOnUnobservedTaskException;
            AndroidEnvironment.UnhandledExceptionRaiser += OnUnhandledExceptionRaiser;

            GAService.Instance.InitializeGAService(this);

            switch (Intent.Action)
            {
                case Intent.ActionSend:
                    {
                        if (BasePresenter.User.IsAuthenticated)
                        {
                            var uri = (Android.Net.Uri)Intent.GetParcelableExtra(Intent.ExtraStream);
                            var fragmentTransaction = SupportFragmentManager.BeginTransaction();
                            var galleryModel = new GalleryMediaModel
                            {
                                Path = BitmapUtils.GetRealPathFromURI(uri, this)
                            };
                            CurrentHostFragment = HostFragment.NewInstance(new PostCreateFragment(galleryModel));
                            fragmentTransaction.Add(Android.Resource.Id.Content, CurrentHostFragment);
                            fragmentTransaction.Commit();
                        }
                        else
                        {
                            StartActivity(typeof(PreSignInActivity));
                        }
                        return;
                    }
                case Intent.ActionView:
                    {
                        var intent = new Intent(this, BasePresenter.User.IsAuthenticated ? typeof(RootActivity) : typeof(GuestActivity));
                        intent.PutExtra(AppLinkingExtra, Intent?.Data?.ToString());
                        StartActivity(intent);
                        return;
                    }
            }
            StartActivity(BasePresenter.User.IsAuthenticated ? typeof(RootActivity) : typeof(GuestActivity));
        }

        private void OnTaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            AppSettings.Reporter.SendCrash(e.Exception);
            this.ShowAlert(LocalizationKeys.UnexpectedError, Android.Widget.ToastLength.Short);
        }

        private void OnCurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex != null)
                ex = new Exception(e.ExceptionObject.ToString());
            AppSettings.Reporter.SendCrash(ex);
            this.ShowAlert(LocalizationKeys.UnexpectedError, Android.Widget.ToastLength.Short);
        }

        private void OnUnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            AppSettings.Reporter.SendCrash(e.Exception);
            this.ShowAlert(LocalizationKeys.UnexpectedError, Android.Widget.ToastLength.Short);
        }
    }
}
