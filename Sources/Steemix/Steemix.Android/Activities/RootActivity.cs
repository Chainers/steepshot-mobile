using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Com.Lilarcor.Cheeseknife;
using Steemix.Droid.ViewModels;
using PagerAdapter = Steemix.Droid.Adapter.PagerAdapter;

namespace Steemix.Droid.Activities
{
	[Activity(Label = "SteepShot",ScreenOrientation = ScreenOrientation.Portrait)]
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
					tab.SetIcon(ContextCompat.GetDrawable(this,Adapter.tabIcos[i]));
				}
			}

			viewPager.AddOnPageChangeListener(this);
			tabLayout.GetTabAt(0).Icon.SetColorFilter(Color.Black, PorterDuff.Mode.SrcIn);
			viewPager.SetCurrentItem(0, false);
			viewPager.OffscreenPageLimit = 2;
		}

		public void OnPageScrollStateChanged(int state)
		{
		}

		private void CheckLogin()
		{ 
			if (!UserPrincipal.IsAuthenticated && viewPager.CurrentItem>0)
			{
				Intent loginItent = new Intent(this, typeof(SignInActivity));
				StartActivity(loginItent);
			}
		}

		public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
		{
			
		}

		public void OnPageSelected(int position)
		{
			for (int i = 0; i < tabLayout.TabCount; i++)
			{
				TabLayout.Tab tab = tabLayout.GetTabAt(i);
				if (null != tab)
				{
					if (i == position)
					{
						tab.Icon.SetColorFilter(Color.Black,PorterDuff.Mode.SrcIn);
					}
					else
					{
						tab.Icon.SetColorFilter(Color.LightGray, PorterDuff.Mode.SrcIn);
					}
				}
			}
		}
	}
}
