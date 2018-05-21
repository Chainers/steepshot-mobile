using Android.OS;
using Android.Views;
using Steepshot.Activity;

namespace Steepshot.Base
{
    public abstract class BaseFragment : Android.Support.V4.App.Fragment
    {
        protected bool IsInitialized;
        protected View InflatedView;

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            IsInitialized = true;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            App.InitIoC(Context.Assets);
            base.OnCreate(savedInstanceState);
        }

        protected void ToggleTabBar(bool shouldHide = false)
        {
            if (Activity is RootActivity activity)
                activity._tabLayout.Visibility = shouldHide ? ViewStates.Gone : ViewStates.Visible;
        }

        public override void OnDetach()
        {
            IsInitialized = false;
            base.OnDetach();
        }

        public virtual bool OnBackPressed() => false;
    }
}
