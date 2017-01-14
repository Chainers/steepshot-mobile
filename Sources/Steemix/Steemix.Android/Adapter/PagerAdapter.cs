using Android.Content;
using Android.Support.V4.App;

namespace Steemix.Droid
{
	public class PagerAdapter : FragmentPagerAdapter
	{
		public int[] tabIcos = new int[] {
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
	}
}