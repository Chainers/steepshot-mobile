using System;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Sweetshot.Library.Models.Requests;


namespace Steepshot
{
	public class FeedFragment : BaseFragment, FeedView
	{
		FeedPresenter presenter;

		public static int SearchRequestCode = 1336;

		[InjectView(Resource.Id.feed_list)]
		RecyclerView FeedList;

		[InjectView(Resource.Id.loading_spinner)]
		ProgressBar Bar;

		FeedAdapter FeedAdapter;

		[InjectView(Resource.Id.pop_up_arrow)]
		ImageView arrow;

		[InjectOnClick(Resource.Id.btn_search)]
		public void OnSearch(object sender, EventArgs e)
		{
			Intent searchIntent = new Intent(this.Activity, typeof(SearchActivity));
			StartActivityForResult(searchIntent, SearchRequestCode);
		}

		[InjectView(Resource.Id.btn_login)]
		Button Logout;

		[InjectOnClick(Resource.Id.btn_login)]
		public void OnLogout(object sender, EventArgs e)
		{
			presenter.Logout();
			UserPrincipal.Instance.DeleteUser();
			Intent i = new Intent(Context, typeof(GuestActivity));
			i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
			StartActivity(i);
		}

		public async void OnSearchPosts(string title, PostType type)
		{
			Title.Text = title;
			presenter.ClearPosts();
			Bar.Visibility = ViewStates.Visible;
			await presenter.GetTopPosts(20, type, true);
			Bar.Visibility = ViewStates.Gone;
		}

		[InjectView(Resource.Id.Title)]
		public TextView Title;

		[InjectOnClick(Resource.Id.Title)]
		public void OnTitleClick(object sender, EventArgs e)
		{
			if (ChildFragmentManager.FindFragmentByTag(FollowingFragmentId) == null)
				ShowFollowing();
			else
				HideFollowing();
		}

		public const string FollowingFragmentId = "FollowingFragment";

		public async override void OnActivityResult(int requestCode, int resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
			try
			{
				if (requestCode == SearchRequestCode)
				{
					var b = data.GetBundleExtra("SEARCH");

					var s = b.GetString("SEARCH");
					Title.Text = s;
					presenter.ClearPosts();
					Bar.Visibility = ViewStates.Visible;
					await presenter.GetSearchedPosts(s);
					Bar.Visibility = ViewStates.Gone;
				}
			}
			catch
			{ }
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var v = inflater.Inflate(Resource.Layout.lyt_feed, null);
			Cheeseknife.Inject(this, v);
			return v;
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);
			Logout.Visibility = ViewStates.Gone;
			Title.Text = "Trending";
			FeedAdapter = new FeedAdapter(Context, presenter.Posts);
			FeedList.SetAdapter(FeedAdapter);
			FeedList.SetLayoutManager(new LinearLayoutManager(Android.App.Application.Context));
			FeedList.AddOnScrollListener(new FeedsScrollListener(presenter));
			FeedAdapter.LikeAction += FeedAdapter_LikeAction;
			FeedAdapter.UserAction += FeedAdapter_UserAction;
			FeedAdapter.CommentAction += FeedAdapter_CommentAction;
			FeedAdapter.PhotoClick += PhotoClick;
			presenter.ViewLoad();
		}

		public void PhotoClick(int position)
		{
			Intent intent = new Intent(this.Context, typeof(PostPreviewActivity));
			intent.PutExtra("PhotoURL", presenter.Posts[position].Body);
			StartActivity(intent);
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
				//int pos = ((LinearLayoutManager)recyclerView.GetLayoutManager()).FindLastCompletelyVisibleItemPosition();
				var pos = ((LinearLayoutManager)recyclerView.GetLayoutManager()).FindLastVisibleItemPosition();
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

        public override void OnDestroyView()
        {
            base.OnDestroyView();
            Cheeseknife.Reset(this);
        }

        void FeedAdapter_CommentAction(int position)
        {
            Intent intent = new Intent(this.Context, typeof(CommentsActivity));
			intent.PutExtra("uid", presenter.Posts[position].Url);
            this.Context.StartActivity(intent);
        }

        void FeedAdapter_UserAction(int position)
		{
			Intent intent = new Intent(this.Context, typeof(ProfileActivity));
			intent.PutExtra("ID", presenter.Posts[position].Author);
			this.Context.StartActivity(intent);
		}

		async void FeedAdapter_LikeAction(int position)
        {
            if (UserPrincipal.Instance.IsAuthenticated)
            {
				var response = await presenter.Vote(presenter.Posts[position]);

                if (response.Success)
                {
                    presenter.Posts[position].Vote = !presenter.Posts[position].Vote;

                    presenter.Posts[position].NetVotes = (presenter.Posts[position].Vote) ?
                        presenter.Posts[position].NetVotes + 1 :
                        presenter.Posts[position].NetVotes - 1;
                }
                else
                {
                    Toast.MakeText(Context, response.Errors[0], ToastLength.Long).Show();
                }
                FeedAdapter.NotifyDataSetChanged();
            }
            else
            {
                var intent = new Intent(Context, typeof(SignInActivity));
                StartActivity(intent);
            }
        }

        public override void OnResume()
        {
            base.OnResume();
            FeedAdapter.NotifyDataSetChanged();
            presenter.Posts.CollectionChanged += Posts_CollectionChanged;

			if (presenter.Posts.Count > 0)
            {
                Bar.Visibility = ViewStates.Gone;
            }
        }

        public override void OnPause()
        {
			presenter.Posts.CollectionChanged -= Posts_CollectionChanged;
            base.OnPause();
        }

        void Posts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ((RootActivity)Context).RunOnUiThread(() =>
            {
                if (Bar.Visibility == ViewStates.Visible)
                    Bar.Visibility = ViewStates.Gone;
                FeedAdapter.NotifyDataSetChanged();
            });
        }

        public void ShowFollowing()
        {
            arrow.StartAnimation(AnimationUtils.LoadAnimation(Context, Resource.Animation.rotate180));
            ChildFragmentManager.BeginTransaction()
                                .SetCustomAnimations(Resource.Animation.up_down, Resource.Animation.down_up, Resource.Animation.up_down, Resource.Animation.down_up)
                                .Add(Resource.Id.fragment_container, new FollowingFragment(this), FollowingFragmentId)
                                .Commit();
        }

        public void HideFollowing()
        {
            arrow.StartAnimation(AnimationUtils.LoadAnimation(Context, Resource.Animation.rotate0));
            ChildFragmentManager.BeginTransaction()
                                .Remove(ChildFragmentManager.FindFragmentByTag(FollowingFragmentId))
                                .Commit();
        }

		protected override void CreatePresenter()
		{
			presenter = new FeedPresenter(this);
		}
	}
}
