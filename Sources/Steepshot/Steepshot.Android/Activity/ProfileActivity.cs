using Android.App;
using Android.OS;
using Steepshot.Base;
using Steepshot.Fragment;

namespace Steepshot.Activity
{
    [Activity(Label = "ProfileActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public sealed class ProfileActivity : BaseActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var fragmentTransaction = SupportFragmentManager.BeginTransaction();
            fragmentTransaction.Add(Android.Resource.Id.Content, new ProfileFragment(Intent.GetStringExtra("ID")));
            fragmentTransaction.Commit();
        }
    }
}
