using System;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Com.Lilarcor.Cheeseknife;

namespace Steemix.Droid.Fragments
{
	public class FollowingFragment : Fragment
	{
		[InjectOnClick(Resource.Id.btn_new)]
		public void OnNewClick(object sender, EventArgs e)
		{
			//(Activity as FeedActivity).HideFollowing();
		}

		[InjectOnClick(Resource.Id.btn_hot)]
		public void OnHotClick(object sender, EventArgs e)
		{
			//(Activity as FeedActivity).HideFollowing();
		}

		[InjectOnClick(Resource.Id.btn_trending)]
		public void OnTrendingClick(object sender, EventArgs e)
		{
			//(Activity as FeedActivity).HideFollowing();
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var v = inflater.Inflate(Resource.Layout.lyt_following, null);
			Cheeseknife.Inject(this, v);
			return v;
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();
			Cheeseknife.Reset(this);
		}
	}
}
