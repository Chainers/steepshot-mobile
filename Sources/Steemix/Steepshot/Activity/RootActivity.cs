using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Com.Lilarcor.Cheeseknife;

namespace Steepshot
{
	[Activity(Label = "SteepShot",ScreenOrientation = ScreenOrientation.Portrait)]
	public class RootActivity : BaseActivity, ViewPager.IOnPageChangeListener, RootView
	{
		RootPresenter presenter;

#pragma warning disable 0649, 4014
		[InjectView(Resource.Id.view_pager)] ViewPager viewPager;
		[InjectView(Resource.Id.tab_layout)] TabLayout tabLayout;
#pragma warning restore 0649

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

		protected override void OnDestroy()
		{
			base.OnDestroy();
			Cheeseknife.Reset(this);
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
			OnPageSelected(0);
			viewPager.SetCurrentItem(0, false);
			viewPager.OffscreenPageLimit = 2;
		}

		private void CheckLogin()
		{ 
			if (!UserPrincipal.Instance.IsAuthenticated && viewPager.CurrentItem>0)
			{
				Intent loginItent = new Intent(this, typeof(SignInActivity));
				StartActivity(loginItent);
			}
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

		protected override void CreatePresenter()
		{
			presenter = new RootPresenter(this);
		}

		public void OnPageScrollStateChanged(int state)
		{
			
		}

		public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
		{
			
		}
	}
}
