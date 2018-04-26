using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Steepshot.Base;
using Steepshot.Fragment;
using Steepshot.Interfaces;

namespace Steepshot.Activity
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, LaunchMode = LaunchMode.SingleTask)]
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

        protected override void OnNewIntent(Intent intent)
        {
            HandleLink(intent);
            base.OnNewIntent(intent);
        }

        public override void OnBackPressed()
        {
            if (CurrentHostFragment != null)
            {
                var fragments = CurrentHostFragment.ChildFragmentManager.Fragments;
                if (fragments[fragments.Count - 1] is ICanOpenPost fragment)
                    if (fragment.ClosePost())
                        return;
            }

            if (CurrentHostFragment == null || !CurrentHostFragment.HandleBackPressed(SupportFragmentManager))
                MinimizeApp();
        }

        public void SelectTabWithClearing(int position)
        {
            CurrentHostFragment?.Clear();
        }
    }
}
