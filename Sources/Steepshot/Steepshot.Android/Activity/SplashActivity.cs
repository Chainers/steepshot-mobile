using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Utils;
using Android.Content;
using Android.Runtime;
using Com.OneSignal;
using Com.OneSignal.Abstractions;
using Newtonsoft.Json;
using Steepshot.Core.Localization;
using Steepshot.Fragment;
using Steepshot.Services;
using static Steepshot.Core.Utils.AppSettings;

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

            AppDomain.CurrentDomain.UnhandledException -= OnCurrentDomainOnUnhandledExceptionAsync;
            TaskScheduler.UnobservedTaskException -= OnTaskSchedulerOnUnobservedTaskException;
            AndroidEnvironment.UnhandledExceptionRaiser -= OnUnhandledExceptionRaiser;

            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainOnUnhandledExceptionAsync;
            TaskScheduler.UnobservedTaskException += OnTaskSchedulerOnUnobservedTaskException;
            AndroidEnvironment.UnhandledExceptionRaiser += OnUnhandledExceptionRaiser;

            GAService.Instance.InitializeGAService(this);
            InitPushes();

            switch (Intent.Action)
            {
                case Intent.ActionSend:
                    {
                        if (User.HasPostingPermission)
                        {
                            var uri = (Android.Net.Uri)Intent.GetParcelableExtra(Intent.ExtraStream);
                            var fragmentTransaction = SupportFragmentManager.BeginTransaction();
                            var galleryModel = new GalleryMediaModel
                            {
                                Path = BitmapUtils.GetUriRealPath(this, uri)
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
                        var intent = new Intent(this, User.HasPostingPermission ? typeof(RootActivity) : typeof(GuestActivity));
                        intent.PutExtra(AppLinkingExtra, Intent?.Data?.Path);
                        StartActivity(intent);
                        return;
                    }
            }
            StartActivity(User.HasPostingPermission ? typeof(RootActivity) : typeof(GuestActivity));
        }

        private void InitPushes()
        {
            OneSignal.Current.StartInit("77fa644f-3280-4e87-9f14-1f0c7ddf8ca5")
                .InFocusDisplaying(OSInFocusDisplayOption.None)
                .HandleNotificationOpened(OneSignalNotificationOpened)
                .EndInit();
        }

        private void OneSignalNotificationOpened(OSNotificationOpenedResult result)
        {
            var additionalData = result?.notification?.payload?.additionalData;
            if (additionalData?.Any() ?? false)
            {
                try
                {
                    var data = JsonConvert.SerializeObject(additionalData.ToDictionary(x => x.Key, x => x.Value.ToString()));
                    RunOnUiThread(() =>
                    {
                        var intent = new Intent(this, typeof(RootActivity));
                        intent.PutExtra(RootActivity.NotificationData, data);
                        StartActivity(intent);
                    });
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }

        private async void OnTaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            await Logger.Error(e.Exception);

            this.ShowAlert(LocalizationKeys.UnexpectedError, Android.Widget.ToastLength.Short);
        }

        private async void OnCurrentDomainOnUnhandledExceptionAsync(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex != null)
                ex = new Exception(e.ExceptionObject.ToString());

            if (e.IsTerminating)
                await Logger.Fatal(ex);
            else
                await Logger.Error(ex);

            this.ShowAlert(ex, Android.Widget.ToastLength.Short);
        }

        private async void OnUnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            await Logger.Error(e.Exception);

            this.ShowAlert(e.Exception, Android.Widget.ToastLength.Short);
        }
    }
}
