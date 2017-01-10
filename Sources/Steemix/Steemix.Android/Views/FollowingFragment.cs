using System;
using Android.Support.V4.App;
using Android.Views;
using Android.OS;
using Com.Lilarcor.Cheeseknife;
using Steemix.Droid.Activity;

namespace Steemix.Droid
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
