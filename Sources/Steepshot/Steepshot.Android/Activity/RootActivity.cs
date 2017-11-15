using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Base;
using Steepshot.Core.Presenters;
using Steepshot.Fragment;
using Steepshot.Utils;

namespace Steepshot.Activity
{
    [Activity(Label = Core.Constants.Steepshot, ScreenOrientation = ScreenOrientation.Portrait)]
    public sealed class RootActivity : BaseActivity
    {
        private Adapter.PagerAdapter _adapter;
        private TabLayout.Tab _prevTab;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.view_pager)] private CustomViewPager _viewPager;
        [InjectView(Resource.Id.tab_layout)] private TabLayout _tabLayout;
#pragma warning restore 0649


        protected override void OnCreate(Bundle savedInstanceState)
        {
            if (BasePresenter.User.IsAuthenticated && !BasePresenter.User.IsNeedRewards)
                BasePresenter.User.IsNeedRewards = true; // for android users set true by default

            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_tab_host);
            Cheeseknife.Inject(this);
            _adapter = new Adapter.PagerAdapter(SupportFragmentManager);
            _viewPager.Adapter = _adapter;
            InitTabs();

            _tabLayout.TabSelected += OnTabLayoutOnTabSelected;
        }

        public override void OpenNewContentFragment(Android.Support.V4.App.Fragment frag)
        {
            CurrentHostFragment = _adapter.GetItem(_viewPager.CurrentItem) as HostFragment;
            base.OpenNewContentFragment(frag);
        }

        public override void OnBackPressed()
        {
            if (CurrentHostFragment == null || !CurrentHostFragment.HandleBackPressed(SupportFragmentManager))
            {
                var intent = new Intent(Intent.ActionMain);
                intent.AddCategory(Intent.CategoryHome);
                intent.SetFlags(ActivityFlags.NewTask);
                StartActivity(intent);
                Finish();
            }
            else
                base.OnBackPressed();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cheeseknife.Reset(this);
        }

        protected override void OnResume()
        {
            base.OnResume();
            if(BasePresenter.ShouldUpdateProfile)
            {
                OnTabSelected(_adapter.Count - 1);
            }
        }

        private void OnTabLayoutOnTabSelected(object sender, TabLayout.TabSelectedEventArgs e)
        {
            if (e.Tab.Position == 2)
            {
                _prevTab.Select();
                var intent = new Intent(this, typeof(CameraActivity));
                StartActivity(intent);
            }
            else
            {
                OnTabSelected(e.Tab.Position);
                _prevTab = e.Tab;
            }
        }

        private void InitTabs()
        {
            for (var i = 0; i < _adapter.TabIconsInactive.Length; i++)
            {
                var tab = _tabLayout.NewTab();
                if (i == 0)
                    _prevTab = tab;
                _tabLayout.AddTab(tab);
                tab.SetIcon(ContextCompat.GetDrawable(this, _adapter.TabIconsInactive[i]));
            }
            OnTabSelected(0);
            _viewPager.OffscreenPageLimit = _adapter.Count - 1;
        }

        private void OnTabSelected(int position)
        {
            _viewPager.SetCurrentItem(position, false);
            for (var i = 0; i < _tabLayout.TabCount; i++)
            {
                var tab = _tabLayout.GetTabAt(i);
                tab?.SetIcon(i == position
                             ? ContextCompat.GetDrawable(this, _adapter.TabIconsActive[i])
                             : ContextCompat.GetDrawable(this, _adapter.TabIconsInactive[i]));
            }
        }
    }
}
