using System;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Base;

namespace Steepshot.Fragment
{
    public class FollowingFragment : BaseFragment
    {
        public FollowingFragment() { }

        readonly FeedFragment _parent;
        public FollowingFragment(FeedFragment parent)
        {
            _parent = parent;
        }

        [InjectOnClick(Resource.Id.btn_new)]
        public void OnNewClick(object sender, EventArgs e)
        {
            _parent.CustomTag = null;
            _parent.OnSearchPosts(((AppCompatButton)sender).Text, Core.Models.Requests.PostType.New);
            _parent.HideFollowing();
        }

        [InjectOnClick(Resource.Id.btn_hot)]
        public void OnHotClick(object sender, EventArgs e)
        {
            _parent.CustomTag = null;
            _parent.OnSearchPosts(((AppCompatButton)sender).Text, Core.Models.Requests.PostType.Hot);
            _parent.HideFollowing();
        }

        [InjectOnClick(Resource.Id.btn_trending)]
        public void OnTrendingClick(object sender, EventArgs e)
        {
            _parent.CustomTag = null;
            _parent.OnSearchPosts(((AppCompatButton)sender).Text, Core.Models.Requests.PostType.Top);
            _parent.HideFollowing();
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