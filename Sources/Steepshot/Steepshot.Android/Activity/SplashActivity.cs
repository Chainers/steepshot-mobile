using Android.App;
using Android.Content.PM;
using Android.OS;
using Steepshot.Base;
using Steepshot.Core;
using Android.Content;
using Steepshot.Services;

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

           
            GAService.Instance.InitializeGAService(this);

            switch (Intent.Action)
            {
                case Intent.ActionSend:
                    {
                        if (App.User.HasPostingPermission)
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
                        var intent = new Intent(this, App.User.HasPostingPermission ? typeof(RootActivity) : typeof(GuestActivity));
                        intent.PutExtra(AppLinkingExtra, Intent?.Data?.Path);
                        intent.SetFlags(ActivityFlags.ReorderToFront | ActivityFlags.NewTask);
                        StartActivity(intent);
                        return;
                    }
            }
            StartActivity(App.User.HasPostingPermission ? typeof(RootActivity) : typeof(GuestActivity));
        }
    }
}
