using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Presenters;
using Steepshot.Fragment;

namespace Steepshot.Activity
{
    [Activity(Label = Constants.Steepshot, ScreenOrientation = ScreenOrientation.Portrait)]
    public class RootActivity : BaseActivity, ViewPager.IOnPageChangeListener
    {
        private Adapter.PagerAdapter _adapter;
        public string VoterUrl;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.view_pager)] ViewPager _viewPager;
        [InjectView(Resource.Id.tab_layout)] TabLayout _tabLayout;
#pragma warning restore 0649

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_tab_host);
            Cheeseknife.Inject(this);
            _adapter = new Adapter.PagerAdapter(SupportFragmentManager);
            _viewPager.Adapter = _adapter;
            _tabLayout.SetupWithViewPager(_viewPager);
            InitTabs();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cheeseknife.Reset(this);
        }

        private void InitTabs()
        {
            for (var i = 0; i < _tabLayout.TabCount; i++)
            {
                var tab = _tabLayout.GetTabAt(i);
                tab?.SetIcon(ContextCompat.GetDrawable(this, _adapter.TabIconsInactive[i]));
            }

            _viewPager.AddOnPageChangeListener(this);
            OnPageSelected(0);
            _viewPager.SetCurrentItem(0, false);
            _viewPager.OffscreenPageLimit = 3;
        }

        private void CheckLogin()
        {
            if (!BasePresenter.User.IsAuthenticated && _viewPager.CurrentItem > 0)
            {
                var loginItent = new Intent(this, typeof(SignInActivity));
                StartActivity(loginItent);
            }
        }

        public void OnPageSelected(int position)
        {
            for (var i = 0; i < _tabLayout.TabCount; i++)
            {
                var tab = _tabLayout.GetTabAt(i);
                tab?.SetIcon(i == position
                             ? ContextCompat.GetDrawable(this, _adapter.TabIconsActive[i])
                             : ContextCompat.GetDrawable(this, _adapter.TabIconsInactive[i]));
            }
        }

        public override void OpenNewContentFragment(Android.Support.V4.App.Fragment frag)
        {
            CurrentHostFragment = (HostFragment)_adapter.GetItem(_viewPager.CurrentItem);
            base.OpenNewContentFragment(frag);
        }

        public void OnPageScrollStateChanged(int state)
        {

        }

        public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
        {

        }
    }
}
