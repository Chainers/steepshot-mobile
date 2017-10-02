using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Fragment;

namespace Steepshot.Activity
{
    [Activity(Label = Constants.Steepshot, ScreenOrientation = ScreenOrientation.Portrait)]
    public class RootActivity : BaseActivity
    {
        private Adapter.PagerAdapter _adapter;
        private TabLayout.Tab prevTab;

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
            InitTabs();

            _tabLayout.TabSelected += (sender, e) => 
            {
                if (e.Tab.Position == 2)
                {
                    prevTab.Select();
                    var intent = new Intent(this, typeof(CameraActivity));
                    StartActivity(intent);
                }
                else
                {
                    OnTabSelected(e.Tab.Position);
                    prevTab = e.Tab;
                }
                
            };
        }

        private void InitTabs()
        {
            for (var i = 0; i < _adapter.TabIcos.Length; i++)
            {
                var tab = _tabLayout.NewTab();
                if (i == 0)
                    prevTab = tab;
                _tabLayout.AddTab(tab);
                tab.SetIcon(ContextCompat.GetDrawable(this, _adapter.TabIcos[i]));
            }
            OnTabSelected(0);
            _viewPager.OffscreenPageLimit = 3;
        }

        public void OnTabSelected(int position)
        {
            _viewPager.SetCurrentItem(position, false);
            for (var i = 0; i < _tabLayout.TabCount; i++)
            {
                var tab = _tabLayout.GetTabAt(i);
                tab?.Icon.SetColorFilter(i == position ? Color.Black : Color.LightGray, PorterDuff.Mode.SrcIn);
            }
        }

        public override void OpenNewContentFragment(Android.Support.V4.App.Fragment frag)
        {
            CurrentHostFragment = (HostFragment)_adapter.GetItem(_viewPager.CurrentItem);
            base.OpenNewContentFragment(frag);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cheeseknife.Reset(this);
        }
    }
}
