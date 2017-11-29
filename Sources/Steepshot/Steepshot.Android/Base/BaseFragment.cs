using Android.Views;
using Steepshot.Activity;

namespace Steepshot.Base
{
    public abstract class BaseFragment : Android.Support.V4.App.Fragment
    {
        protected bool IsInitialized;
        protected View InflatedView;

        public override void OnViewCreated(View view, Android.OS.Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            IsInitialized = true;
        }

        protected void ToggleTabBar(bool shouldHide = false)
        {
            if (Activity is RootActivity activity)
                activity._tabLayout.Visibility = shouldHide ? ViewStates.Gone : ViewStates.Visible;
        }

        public override void OnCreate(Android.OS.Bundle savedInstanceState)
        {
            BaseActivity.InitIoC();
            base.OnCreate(savedInstanceState);
        }

        public override void OnDetach()
        {
            IsInitialized = false;
            base.OnDetach();
        }
    }
}
