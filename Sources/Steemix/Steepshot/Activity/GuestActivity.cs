using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Sweetshot.Library.Models.Requests;

namespace Steepshot
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class GuestActivity : BaseActivity, FeedView
    {
        public static int SearchRequestCode = 1336;
        FeedPresenter presenter;

        [InjectView(Resource.Id.feed_list)]
        public RecyclerView FeedList;

        [InjectView(Resource.Id.loading_spinner)]
        ProgressBar Bar;

        FeedAdapter FeedAdapter;

        [InjectView(Resource.Id.pop_up_arrow)]
        ImageView arrow;

        [InjectView(Resource.Id.btn_login)]
        Button Login;

        [InjectView(Resource.Id.Title)]
        TextView title;

        [InjectView(Resource.Id.btn_search)]
        ImageButton Search;

        [InjectOnClick(Resource.Id.btn_login)]
        public void OnLoginClick(object sender, EventArgs e)
        {
            OpenLogin();
        }

        [InjectOnClick(Resource.Id.Title)]
        public void OnTitleClick(object sender, EventArgs e)
        {
            if (SupportFragmentManager.FindFragmentByTag(FollowingFragmentId) == null)
                ShowFollowing();
            else
                HideFollowing();
        }

        public void ShowFollowing()
        {
            arrow.StartAnimation(AnimationUtils.LoadAnimation(this, Resource.Animation.rotate180));
            SupportFragmentManager.BeginTransaction()
                                .SetCustomAnimations(Resource.Animation.up_down, Resource.Animation.down_up, Resource.Animation.up_down, Resource.Animation.down_up)
                                .Add(Resource.Id.fragment_container, new FollowingFragment(), FollowingFragmentId)
                                .Commit();
        }

        public void HideFollowing()
        {
            arrow.StartAnimation(AnimationUtils.LoadAnimation(this, Resource.Animation.rotate0));
            SupportFragmentManager.BeginTransaction()
                                .Remove(SupportFragmentManager.FindFragmentByTag(FollowingFragmentId))
                                .Commit();
        }

        [InjectOnClick(Resource.Id.btn_search)]
        public void OnSearch(object sender, EventArgs e)
        {
            Intent searchIntent = new Intent(this, typeof(SearchActivity));
            StartActivityForResult(searchIntent, SearchRequestCode);
        }

        public async void OnSearchPosts(string _title, PostType type)
        {
            title.Text = _title;
            presenter.ClearPosts();
            Bar.Visibility = ViewStates.Visible;
            await presenter.GetTopPosts(20, type, true);
            Bar.Visibility = ViewStates.Gone;
        }

        public const string FollowingFragmentId = "FollowingFragment";

        protected override async void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            try
            {
                if (requestCode == SearchRequestCode)
                {
                    var b = data.GetBundleExtra("SEARCH");

                    var s = b.GetString("SEARCH");
                    title.Text = s;
                    presenter.ClearPosts();
                    Bar.Visibility = ViewStates.Visible;
                    await presenter.GetSearchedPosts(s);
                    Bar.Visibility = ViewStates.Gone;
                }
            }
            catch
            { }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.lyt_feed);
            Cheeseknife.Inject(this);

			FeedAdapter = new FeedAdapter(this, presenter.Posts);
            FeedList.SetAdapter(FeedAdapter);
            FeedList.SetLayoutManager(new LinearLayoutManager(this));
            FeedList.AddOnScrollListener(new FeedsScrollListener(presenter));
            FeedAdapter.LikeAction += FeedAdapter_LikeAction;
            FeedAdapter.PhotoClick += PhotoClick;
            FeedAdapter.CommentAction += FeedAdapter_CommentAction;
            FeedAdapter.UserAction += FeedAdapter_UserAction;
            Login.Visibility = ViewStates.Visible;

			var refresher = FindViewById<SwipeRefreshLayout>(Resource.Id.feed_refresher);
			refresher.Refresh += async delegate
				{
					presenter.ClearPosts();
					await presenter.GetTopPosts(20, presenter.GetCurrentType());
					refresher.Refreshing = false;
				};
        }

        public void PhotoClick(int position)
        {
            Intent intent = new Intent(this, typeof(PostPreviewActivity));
            intent.PutExtra("PhotoURL", presenter.Posts[position].Body);
            StartActivity(intent);
        }

        void FeedAdapter_CommentAction(int position)
        {
            Intent intent = new Intent(this, typeof(CommentsActivity));
            intent.PutExtra("uid", presenter.Posts[position].Url);
            this.StartActivity(intent);
        }

        void FeedAdapter_UserAction(int position)
        {
            Intent intent = new Intent(this, typeof(ProfileActivity));
            intent.PutExtra("ID", presenter.Posts[position].Author);
            this.StartActivity(intent);
        }

        private class FeedsScrollListener : RecyclerView.OnScrollListener
        {
            FeedPresenter presenter;
            public FeedsScrollListener(FeedPresenter presenter)
            {
                this.presenter = presenter;
            }
            int prevPos = 0;
            public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
            {
                int pos = ((LinearLayoutManager)recyclerView.GetLayoutManager()).FindLastCompletelyVisibleItemPosition();
                if (pos > prevPos && pos != prevPos)
                {
                    if (pos == recyclerView.GetAdapter().ItemCount - 1)
                    {
                        if (pos < ((FeedAdapter)recyclerView.GetAdapter()).ItemCount)
                        {
                            Task.Run(() => presenter.GetTopPosts(20, presenter.GetCurrentType()));
                            prevPos = pos;
                        }
                    }
                }
            }

            public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
            {

            }
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
		{
			base.OnPostCreate(savedInstanceState);
			presenter.ViewLoad();
		}

        void FeedAdapter_LikeAction(int position)
        {
             OpenLogin();
        }

        private void OpenLogin()
        {
            Intent intent = new Intent(this, typeof(PreSignInActivity));
            StartActivity(intent);
        }

        protected override void OnResume()
        {
            base.OnResume();
            FeedAdapter.NotifyDataSetChanged();
			presenter.Posts.CollectionChanged += Posts_CollectionChanged;
            if (presenter.Posts != null && presenter.Posts.Count>0)
                Bar.Visibility = ViewStates.Gone;
        }

        protected override void OnPause()
        {
			presenter.Posts.CollectionChanged -= Posts_CollectionChanged;
            base.OnPause();
        }

        void Posts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                if (Bar.Visibility == ViewStates.Visible)
                    Bar.Visibility = ViewStates.Gone;
                FeedAdapter.NotifyDataSetChanged();
            });
        }

		protected override void CreatePresenter()
		{
			presenter = new FeedPresenter(this);
		}
	}
}
