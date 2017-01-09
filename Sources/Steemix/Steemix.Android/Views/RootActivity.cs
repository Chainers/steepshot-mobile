using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Com.Lilarcor.Cheeseknife;
using Steemix.Android.Activity;

namespace Steemix.Android
{
	[Activity(Label = "SteepShot", MainLauncher = true, Icon = "@mipmap/ic_launcher", ScreenOrientation = ScreenOrientation.Portrait)]
	public class RootActivity : BaseActivity<TabHostViewModel>,ViewPager.IOnPageChangeListener
	{
		[InjectView(Resource.Id.view_pager)]
		ViewPager viewPager;

		[InjectView(Resource.Id.tab_layout)]
		TabLayout tabLayout;

		PagerAdapter Adapter;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.lyt_tab_host);
			Cheeseknife.Inject(this);

			Adapter = new PagerAdapter(SupportFragmentManager, this);
			viewPager.Adapter = Adapter;
			tabLayout.SetupWithViewPager(viewPager);
			InitTabs();
		}

		private void InitTabs()
		{
			for (int i = 0; i < tabLayout.TabCount; i++)
			{
				TabLayout.Tab tab = tabLayout.GetTabAt(i);
				if (null != tab)
				{
					tab.SetCustomView(Adapter.GetTabView(i));
				}
			}

			viewPager.AddOnPageChangeListener(this);
		}

		public void OnPageScrollStateChanged(int state)
		{
			
		}

		public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
		{
			
		}

		public void OnPageSelected(int position)
		{
			
		}
	}
}
