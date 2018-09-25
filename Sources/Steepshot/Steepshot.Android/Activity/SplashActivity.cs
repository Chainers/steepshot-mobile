using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Utils;
using Android.Content;
using Android.Runtime;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;
using Steepshot.Fragment;
using Steepshot.Services;
using static Steepshot.Core.Utils.AppSettings;

namespace Steepshot.Activity
{
    [Activity(Label = Constants.Steepshot, MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait, NoHistory = true, Theme = "@style/SplashTheme")]
    [IntentFilter(new[] { Intent.ActionSend }, Categories = new[] { Intent.CategoryDefault }, DataMimeType = "image/*")]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable }, DataSchemes = new[] { "https", "http" }, DataHosts = new[] { "alpha.steepshot.io", "qa.alpha.steepshot.io" }, DataPathPrefixes = new[] { "/post", "/@" })]
    public sealed class SplashActivity : BaseActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            AppDomain.CurrentDomain.UnhandledException -= OnCurrentDomainOnUnhandledExceptionAsync;
            TaskScheduler.UnobservedTaskException -= OnTaskSchedulerOnUnobservedTaskException;
            AndroidEnvironment.UnhandledExceptionRaiser -= OnUnhandledExceptionRaiser;

            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainOnUnhandledExceptionAsync;
            TaskScheduler.UnobservedTaskException += OnTaskSchedulerOnUnobservedTaskException;
            AndroidEnvironment.UnhandledExceptionRaiser += OnUnhandledExceptionRaiser;

            GAService.Instance.InitializeGAService(this);

            switch (Intent.Action)
            {
                case Intent.ActionSend:
                    {
                        if (User.HasPostingPermission)
                        {
                            var intent = new Intent(this, typeof(RootActivity));
                            intent.PutExtra(RootActivity.SharingPhotoData, (IParcelable)Intent.GetParcelableExtra(Intent.ExtraStream));
                            intent.SetFlags(ActivityFlags.ReorderToFront | ActivityFlags.NewTask);
                            StartActivity(intent);
                        }
                        else
                        {
                            StartActivity(typeof(PreSignInActivity));
                        }
                        return;
                    }
                case Intent.ActionView:
                    {
                        var intent = new Intent(this, User.HasPostingPermission ? typeof(RootActivity) : typeof(GuestActivity));
                        intent.PutExtra(AppLinkingExtra, Intent?.Data?.Path);
                        intent.SetFlags(ActivityFlags.ReorderToFront | ActivityFlags.NewTask);
                        StartActivity(intent);
                        return;
                    }
                case Intent.ActionMain:
                    {
                        if (AppSettings.Temp.ContainsKey(PostCreateFragment.PostCreateGalleryTemp))
                        {
                            var intent = new Intent(this, typeof(RootActivity));
                            intent.PutExtra(RootActivity.PostCreateResumeExtra, true);
                            intent.SetFlags(ActivityFlags.ReorderToFront | ActivityFlags.NewTask);
                            StartActivity(intent);
                            return;
                        }
                        break;
                    }
            }
            StartActivity(User.HasPostingPermission ? typeof(RootActivity) : typeof(GuestActivity));
        }

        private async void OnTaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            await Logger.ErrorAsync(e.Exception);

            this.ShowAlert(LocalizationKeys.UnexpectedError, Android.Widget.ToastLength.Short);
        }

        private async void OnCurrentDomainOnUnhandledExceptionAsync(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex != null)
                ex = new Exception(e.ExceptionObject.ToString());

            if (e.IsTerminating)
                await Logger.FatalAsync(ex);
            else
                await Logger.ErrorAsync(ex);

            this.ShowAlert(ex, Android.Widget.ToastLength.Short);
        }

        private async void OnUnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            await Logger.ErrorAsync(e.Exception);

            this.ShowAlert(e.Exception, Android.Widget.ToastLength.Short);
        }
    }
}
