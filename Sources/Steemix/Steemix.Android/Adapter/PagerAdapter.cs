using System;
using Android.Content;
using Android.Support.V4.App;
using Android.Support.V4.Content.Res;
using Android.Views;
using Android.Widget;

namespace Steemix.Android
{
	public class PagerAdapter : FragmentPagerAdapter
	{
		int[] tabIcos = new int[] {
			Resource.Drawable.ic_feed,
			Resource.Drawable.ic_camera,
			Resource.Drawable.ic_profile
		};
		Context context;

		public PagerAdapter(FragmentManager fm, Context context) : base(fm)
		{
			this.context = context;
		}

		public override int Count
		{
			get
			{
				return tabIcos.Length;
			}
		}

		public override Fragment GetItem(int position)
		{
			switch (position)
			{
				case 0:
					return new FeedFragment();
				case 1:
					return new PhotoFragment();
				case 2:
					return new ProfileFragment();
			}
			return null;
		}

		public View GetTabView(int position)
		{
			View tab = LayoutInflater.From(context).Inflate(Resource.Layout.tab_main, null);
			ImageView imageView = (ImageView)tab.FindViewById(Resource.Id.tab_image);
			imageView.SetImageDrawable(ResourcesCompat.GetDrawable(context.Resources, tabIcos[position], null));
			return tab;
		}
	}
}