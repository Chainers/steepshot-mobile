using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Steepshot.Base;
using Steepshot.Fragment;
using Steepshot.Interfaces;
using System.Linq;

namespace Steepshot.Activity
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait, LaunchMode = LaunchMode.SingleTask)]
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
            var fragments = CurrentHostFragment?.ChildFragmentManager?.Fragments;
            if (fragments?.Count > 0)
            {
                var lastFragment = fragments.Last();
                if (lastFragment is BaseFragment baseFrg && baseFrg.OnBackPressed())
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
