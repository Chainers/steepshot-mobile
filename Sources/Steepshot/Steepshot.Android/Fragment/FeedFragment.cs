using System;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Activity;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;
using Steepshot.Presenter;

using Steepshot.View;

namespace Steepshot.Fragment
{
	public class FeedFragment : BaseFragment, IFeedView
	{
		private FeedPresenter _presenter;
		private FeedAdapter _feedAdapter;
		public static int SearchRequestCode = 1336;
		public const string FollowingFragmentId = "FollowingFragment";
		public string CustomTag
		{
			get { return _presenter.Tag; }
			set { _presenter.Tag = value; }
		}
		private bool _isFeed;

#pragma warning disable 0649, 4014
		[InjectView(Resource.Id.feed_list)] RecyclerView _feedList;
		[InjectView(Resource.Id.loading_spinner)] ProgressBar _bar;
		[InjectView(Resource.Id.pop_up_arrow)] ImageView _arrow;
		[InjectView(Resource.Id.btn_login)] Button _login;
		[InjectView(Resource.Id.Title)] public TextView Title;
		[InjectView(Resource.Id.feed_refresher)] SwipeRefreshLayout _refresher;
		[InjectView(Resource.Id.btn_search)] ImageButton _search;
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

		public override Android.Views.View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			if (!IsInitialized)
			{
				V = inflater.Inflate(Resource.Layout.lyt_feed, null);
				Cheeseknife.Inject(this, V);
			}
			return V;
		}

		public override void OnViewCreated(Android.Views.View view, Bundle savedInstanceState)
		{
			try
			{
				var s = Activity.Intent.GetStringExtra("SEARCH");
				if (s != null && s != CustomTag && _bar != null)
				{
					Title.Text = s;
					CustomTag = s;
					_presenter.ClearPosts();
					_bar.Visibility = ViewStates.Visible;
					_presenter.GetSearchedPosts();
				}
			}
			catch (Exception ex)
			{
				Reporter.SendCrash(ex, BasePresenter.User.Login, BasePresenter.AppVersion);
			}
			if (IsInitialized)
				return;
			base.OnViewCreated(view, savedInstanceState);
			if (BasePresenter.User.IsAuthenticated)
				_login.Visibility = ViewStates.Gone;

			if (_isFeed)
			{
				Title.Text = "Feed";
				_arrow.Visibility = ViewStates.Gone;
				_search.Visibility = ViewStates.Gone;
			}
			else
				Title.Text = "Trending";

			_feedAdapter = new FeedAdapter(Context, _presenter.Posts);
			_feedList.SetAdapter(_feedAdapter);
			_feedList.SetLayoutManager(new LinearLayoutManager(Android.App.Application.Context));
			_feedList.AddOnScrollListener(new FeedsScrollListener(_presenter));
			_feedAdapter.LikeAction += FeedAdapter_LikeAction;
			_feedAdapter.UserAction += FeedAdapter_UserAction;
			_feedAdapter.CommentAction += FeedAdapter_CommentAction;
			_feedAdapter.VotersClick += FeedAdapter_VotersAction;
			_feedAdapter.PhotoClick += PhotoClick;
			_presenter.ViewLoad();
			_refresher.Refresh += async delegate
				{
					_presenter.ClearPosts();
					if (string.IsNullOrEmpty(CustomTag))
						await _presenter.GetTopPosts(_presenter.GetCurrentType());
					else
						await _presenter.GetSearchedPosts();
					_refresher.Refreshing = false;
				};
		}

		public void OnSearchPosts(string title, PostType type)
		{
			Title.Text = title;
			_presenter.ClearPosts();
			_bar.Visibility = ViewStates.Visible;
			_presenter.GetTopPosts(type, true);
		}

		public void PhotoClick(int position)
		{
			Intent intent = new Intent(Context, typeof(PostPreviewActivity));
			intent.PutExtra("PhotoURL", _presenter.Posts[position].Body);
			StartActivity(intent);
		}

		void FeedAdapter_CommentAction(int position)
		{
			Intent intent = new Intent(Context, typeof(CommentsActivity));
			intent.PutExtra("uid", _presenter.Posts[position].Url);
			Context.StartActivity(intent);
		}

		void FeedAdapter_VotersAction(int position)
		{
			Activity.Intent.PutExtra("url", _presenter.Posts[position].Url);
			((BaseActivity)Activity).OpenNewContentFragment(new VotersFragment());
		}

		void FeedAdapter_UserAction(int position)
		{
			((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(_presenter.Posts[position].Author));
		}

		async void FeedAdapter_LikeAction(int position)
		{
			try
			{
				if (BasePresenter.User.IsAuthenticated)
				{
					var response = await _presenter.Vote(_presenter.Posts[position]);

					if (response.Success)
					{
						if (_presenter.Posts.Count >= position)
						{
							_presenter.Posts[position].Vote = !_presenter.Posts[position].Vote;

							_presenter.Posts[position].NetVotes = (_presenter.Posts[position].Vote) ?
								_presenter.Posts[position].NetVotes + 1 :
								_presenter.Posts[position].NetVotes - 1;
							_presenter.Posts[position].TotalPayoutReward = response.Result.NewTotalPayoutReward;
						}
					}
					else
					{
						Toast.MakeText(Context, response.Errors[0], ToastLength.Long).Show();
					}
					_feedAdapter?.NotifyDataSetChanged();
				}
				else
				{
					OpenLogin();
				}
			}
			catch (Exception ex)
			{
				Reporter.SendCrash(ex, BasePresenter.User.Login, BasePresenter.AppVersion);
			}
		}

		public void ShowFollowing()
		{
			_arrow.StartAnimation(AnimationUtils.LoadAnimation(Context, Resource.Animation.rotate180));
			ChildFragmentManager.BeginTransaction()
								.SetCustomAnimations(Resource.Animation.up_down, Resource.Animation.down_up, Resource.Animation.up_down, Resource.Animation.down_up)
								.Add(Resource.Id.fragment_container, new FollowingFragment(this), FollowingFragmentId)
								.Commit();
		}

		public void HideFollowing()
		{
			_arrow.StartAnimation(AnimationUtils.LoadAnimation(Context, Resource.Animation.rotate0));
			ChildFragmentManager.BeginTransaction()
								.Remove(ChildFragmentManager.FindFragmentByTag(FollowingFragmentId))
								.Commit();
		}

		protected override void CreatePresenter()
		{
			_presenter = new FeedPresenter(this, _isFeed);
			_presenter.PostsLoaded += OnPostLoaded;
			_presenter.PostsCleared += OnPostCleared;
		}

		private void OnPostLoaded()
		{
			Activity.RunOnUiThread(() =>
				{
					if (_bar != null)
						_bar.Visibility = ViewStates.Gone;
					_feedAdapter?.NotifyDataSetChanged();
				});
		}

		private void OnPostCleared()
		{
			Activity.RunOnUiThread(() =>
				{
					_feedAdapter?.NotifyDataSetChanged();
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
			_presenter.PostsLoaded -= OnPostLoaded;
			_presenter.PostsCleared -= OnPostCleared;
			Cheeseknife.Reset(this);
		}

		private class FeedsScrollListener : RecyclerView.OnScrollListener
		{
			FeedPresenter _presenter;
			int _prevPos = 0;

			public FeedsScrollListener(FeedPresenter presenter)
			{
				_presenter = presenter;
				_presenter.PostsCleared += () =>
				{
					_prevPos = 0;
				};
			}

			public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
			{
				//int pos = ((LinearLayoutManager)recyclerView.GetLayoutManager()).FindLastCompletelyVisibleItemPosition();
				var pos = ((LinearLayoutManager)recyclerView.GetLayoutManager()).FindLastVisibleItemPosition();
				if (pos > _prevPos && pos != _prevPos)
				{
					if (pos == recyclerView.GetAdapter().ItemCount - 1)
					{
						if (pos < ((FeedAdapter)recyclerView.GetAdapter()).ItemCount)
						{
							if (string.IsNullOrEmpty(_presenter.Tag))
							{
								Task.Run(() => _presenter.GetTopPosts(_presenter.GetCurrentType()));
							}
							else
							{
								Task.Run(() => _presenter.GetSearchedPosts());
							}
							_prevPos = pos;
						}
					}
				}
			}
		}
	}
}
