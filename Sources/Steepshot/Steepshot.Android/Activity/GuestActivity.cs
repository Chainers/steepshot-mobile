using Android.App;
using Android.OS;
using Steepshot.Base;
using Steepshot.Fragment;
using Steepshot.Interfaces;

namespace Steepshot.Activity
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public sealed class GuestActivity : BaseActivity, IClearable
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var fragmentTransaction = SupportFragmentManager.BeginTransaction();
            CurrentHostFragment = HostFragment.NewInstance(new PreSearchFragment(true));
            CurrentHostFragment.UserVisibleHint = true;
            fragmentTransaction.Add(Android.Resource.Id.Content, CurrentHostFragment);
            fragmentTransaction.Commit();
        }

        public override void OnBackPressed()
        {
            if (CurrentHostFragment == null || !CurrentHostFragment.HandleBackPressed(SupportFragmentManager))
                MinimizeApp();
        }

        public void SelectTabWithClearing(int position)
        {
            CurrentHostFragment?.Clear();
        }
    }
}
