using Android.Content;
using Android.Support.V4.App;

namespace Steepshot
{
	public class PagerAdapter : FragmentPagerAdapter
	{
		public int[] tabIcos = new int[] {
			Resource.Drawable.ic_home,
			Resource.Drawable.ic_browse,
			Resource.Drawable.ic_camera_new,
			Resource.Drawable.ic_profile_new
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
					return new FeedFragment(true);
				case 1:
					return new FeedFragment();
				case 2:
					return new PhotoFragment();
				case 3:
					return new ProfileFragment(UserPrincipal.Instance.CurrentUser.Login);
			}
			return null;
		}
	}
}