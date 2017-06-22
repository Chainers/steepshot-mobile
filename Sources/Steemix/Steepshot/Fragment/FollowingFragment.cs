using System;
using Android.OS;
using Android.Views;
using Com.Lilarcor.Cheeseknife;
using Android.Support.V7.Widget;

namespace Steepshot
{
	public class FollowingFragment : BaseFragment, FollowingView
	{
		FollowingPresenter presenter;

        public FollowingFragment() { }

        FeedFragment parent;
        public FollowingFragment(FeedFragment parent)
        {
            this.parent = parent;
        }

		[InjectOnClick(Resource.Id.btn_new)]
		public void OnNewClick(object sender, EventArgs e)
		{
			parent.CustomTag = null;
			parent.OnSearchPosts(((AppCompatButton)sender).Text, Sweetshot.Library.Models.Requests.PostType.New);
            parent.HideFollowing();
		}

		[InjectOnClick(Resource.Id.btn_hot)]
		public void OnHotClick(object sender, EventArgs e)
		{
			parent.CustomTag = null;
			parent.OnSearchPosts(((AppCompatButton)sender).Text, Sweetshot.Library.Models.Requests.PostType.Hot);
            parent.HideFollowing();
        }

		[InjectOnClick(Resource.Id.btn_trending)]
		public void OnTrendingClick(object sender, EventArgs e)
		{
			parent.CustomTag = null;
			parent.OnSearchPosts(((AppCompatButton)sender).Text, Sweetshot.Library.Models.Requests.PostType.Top);
            parent.HideFollowing();
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

		protected override void CreatePresenter()
		{
			presenter = new FollowingPresenter(this);
		}
	}
}
