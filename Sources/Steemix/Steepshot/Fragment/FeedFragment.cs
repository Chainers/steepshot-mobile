using System;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Widget;
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
		private FeedPresenter presenter;
		private FeedAdapter FeedAdapter;
		public static int SearchRequestCode = 1336;
		public const string FollowingFragmentId = "FollowingFragment";
		public string CustomTag
		{
			get { return presenter.Tag; }
			set { presenter.Tag = value; }
		}
		private bool _isFeed;

#pragma warning disable 0649, 4014
		[InjectView(Resource.Id.feed_list)] RecyclerView FeedList;
		[InjectView(Resource.Id.loading_spinner)] ProgressBar Bar;
		[InjectView(Resource.Id.pop_up_arrow)] ImageView arrow;
		[InjectView(Resource.Id.btn_login)] Button Login;
		[InjectView(Resource.Id.Title)] public TextView Title;
		[InjectView(Resource.Id.feed_refresher)] SwipeRefreshLayout refresher;
		[InjectView(Resource.Id.btn_search)] ImageButton search;
#pragma warning restore 0649

		public FeedFragment(bool isFeed = false)
		{
			_isFeed = isFeed;
		}

		[InjectOnClick(Resource.Id.btn_search)]
		public void OnSearch(object sender, EventArgs e)
		{
			((BaseActivity)Activity).OpenNewContentFragment(new SearchFragment());
		}

		[InjectOnClick(Resource.Id.btn_login)]
		public void OnLogin(object sender, EventArgs e)
		{
			OpenLogin();
		}

		[InjectOnClick(Resource.Id.Title)]
		public void OnTitleClick(object sender, EventArgs e)
		{
			if (_isFeed)
				return;
			if (ChildFragmentManager.FindFragmentByTag(FollowingFragmentId) == null)
				ShowFollowing();
			else
				HideFollowing();
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			if (!_isInitialized)
			{
				v = inflater.Inflate(Resource.Layout.lyt_feed, null);
				Cheeseknife.Inject(this, v);
			}
			return v;
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			try
			{
				var s = Activity.Intent.GetStringExtra("SEARCH");
				if (s != null && s != CustomTag && Bar != null)
				{
					Title.Text = s;
					CustomTag = s;
					presenter.ClearPosts();
					Bar.Visibility = ViewStates.Visible;
					presenter.GetSearchedPosts();
				}
			}
			catch (Exception ex)
			{
				Reporter.SendCrash(ex);
			}
			if (_isInitialized)
				return;
			base.OnViewCreated(view, savedInstanceState);
			if (User.IsAuthenticated)
				Login.Visibility = ViewStates.Gone;

			if (_isFeed)
			{
				Title.Text = "Feed";
				arrow.Visibility = ViewStates.Gone;
				search.Visibility = ViewStates.Gone;
			}
			else
				Title.Text = "Trending";

			FeedAdapter = new FeedAdapter(Context, presenter.Posts);
			FeedList.SetAdapter(FeedAdapter);
			FeedList.SetLayoutManager(new LinearLayoutManager(Android.App.Application.Context));
			FeedList.AddOnScrollListener(new FeedsScrollListener(presenter));
			FeedAdapter.LikeAction += FeedAdapter_LikeAction;
			FeedAdapter.UserAction += FeedAdapter_UserAction;
			FeedAdapter.CommentAction += FeedAdapter_CommentAction;
			FeedAdapter.VotersClick += FeedAdapter_VotersAction;
			FeedAdapter.PhotoClick += PhotoClick;
			presenter.ViewLoad();
			refresher.Refresh += async delegate
				{
					presenter.ClearPosts();
					if (string.IsNullOrEmpty(CustomTag))
						await presenter.GetTopPosts(presenter.GetCurrentType());
					else
						await presenter.GetSearchedPosts();
					refresher.Refreshing = false;
				};
		}

		public void OnSearchPosts(string title, PostType type)
		{
			Title.Text = title;
			presenter.ClearPosts();
			Bar.Visibility = ViewStates.Visible;
			presenter.GetTopPosts(type, true);
		}

		public void PhotoClick(int position)
		{
			Intent intent = new Intent(this.Context, typeof(PostPreviewActivity));
			intent.PutExtra("PhotoURL", presenter.Posts[position].Body);
			StartActivity(intent);
		}

		void FeedAdapter_CommentAction(int position)
		{
			Intent intent = new Intent(this.Context, typeof(CommentsActivity));
			intent.PutExtra("uid", presenter.Posts[position].Url);
			this.Context.StartActivity(intent);
		}

		void FeedAdapter_VotersAction(int position)
		{
			Activity.Intent.PutExtra("url", presenter.Posts[position].Url);
			((BaseActivity)Activity).OpenNewContentFragment(new VotersFragment());
		}

		void FeedAdapter_UserAction(int position)
		{
			((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(presenter.Posts[position].Author));
		}

		async void FeedAdapter_LikeAction(int position)
		{
			try
			{
				if (User.IsAuthenticated)
				{
					var response = await presenter.Vote(presenter.Posts[position]);

					if (response.Success)
					{
						if (presenter.Posts.Count >= position)
						{
							presenter.Posts[position].Vote = !presenter.Posts[position].Vote;

							presenter.Posts[position].NetVotes = (presenter.Posts[position].Vote) ?
								presenter.Posts[position].NetVotes + 1 :
								presenter.Posts[position].NetVotes - 1;
							presenter.Posts[position].TotalPayoutReward = response.Result.NewTotalPayoutReward;
						}
					}
					else
					{
						Toast.MakeText(Context, response.Errors[0], ToastLength.Long).Show();
					}
					FeedAdapter?.NotifyDataSetChanged();
				}
				else
				{
					OpenLogin();
				}
			}
			catch (Exception ex)
			{
				Reporter.SendCrash(ex);
			}
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
			presenter = new FeedPresenter(this, _isFeed);
			presenter.PostsLoaded += OnPostLoaded;
			presenter.PostsCleared += OnPostCleared;
		}

		private void OnPostLoaded()
		{
			Activity.RunOnUiThread(() =>
				{
					if (Bar != null)
						Bar.Visibility = ViewStates.Gone;
					FeedAdapter?.NotifyDataSetChanged();
				});
		}

		private void OnPostCleared()
		{
			Activity.RunOnUiThread(() =>
				{
					FeedAdapter?.NotifyDataSetChanged();
				});
		}

		private void OpenLogin()
		{
			Intent intent = new Intent(Activity, typeof(PreSignInActivity));
			StartActivity(intent);
		}

		public override void OnDetach()
		{
			base.OnDetach();
			presenter.PostsLoaded -= OnPostLoaded;
			presenter.PostsCleared -= OnPostCleared;
			Cheeseknife.Reset(this);
		}

		private class FeedsScrollListener : RecyclerView.OnScrollListener
		{
			FeedPresenter presenter;
			int prevPos = 0;

			public FeedsScrollListener(FeedPresenter presenter)
			{
				this.presenter = presenter;
				this.presenter.PostsCleared += () =>
				{
					prevPos = 0;
				};
			}

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
							if (string.IsNullOrEmpty(presenter.Tag))
							{
								Task.Run(() => presenter.GetTopPosts(presenter.GetCurrentType()));
							}
							else
							{
								Task.Run(() => presenter.GetSearchedPosts());
							}
							prevPos = pos;
						}
					}
				}
			}
		}
	}
}
