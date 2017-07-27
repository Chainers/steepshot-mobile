using System;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Base;
using Steepshot.Presenter;

using Steepshot.View;

namespace Steepshot.Fragment
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
			parent.OnSearchPosts(((AppCompatButton)sender).Text, Steepshot.Core.Models.Requests.PostType.New);
            parent.HideFollowing();
		}

		[InjectOnClick(Resource.Id.btn_hot)]
		public void OnHotClick(object sender, EventArgs e)
		{
			parent.CustomTag = null;
			parent.OnSearchPosts(((AppCompatButton)sender).Text, Steepshot.Core.Models.Requests.PostType.Hot);
            parent.HideFollowing();
        }

		[InjectOnClick(Resource.Id.btn_trending)]
		public void OnTrendingClick(object sender, EventArgs e)
		{
			parent.CustomTag = null;
			parent.OnSearchPosts(((AppCompatButton)sender).Text, Steepshot.Core.Models.Requests.PostType.Top);
            parent.HideFollowing();
        }

		public override Android.Views.View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
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
