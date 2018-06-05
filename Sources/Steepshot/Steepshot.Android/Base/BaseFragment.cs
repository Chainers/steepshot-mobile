using Android.Content;
using Android.OS;
using Android.Views;
using Steepshot.Activity;
using Steepshot.CustomViews;
using Steepshot.Fragment;

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

        protected void AutoLinkAction(AutoLinkType type, string link)
        {
            if (string.IsNullOrEmpty(link))
                return;

            switch (type)
            {
                case AutoLinkType.Hashtag:
                    Activity.Intent.PutExtra(SearchFragment.SearchExtra, link);
                    ((BaseActivity)Activity).OpenNewContentFragment(new PreSearchFragment());
                    break;
                case AutoLinkType.Mention:
                    ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(link));
                    break;
                case AutoLinkType.Url:
                    var intent = new Intent(Intent.ActionView);
                    intent.SetData(Android.Net.Uri.Parse(link));
                    StartActivity(intent);
                    break;
            }
        }
    }
}
