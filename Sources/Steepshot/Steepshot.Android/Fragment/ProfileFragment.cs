using System;
using System.Globalization;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Refractored.Controls;
using Square.Picasso;
using Steepshot.Activity;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;
using Steepshot.Presenter;

using Steepshot.Utils;
using Steepshot.View;

namespace Steepshot.Fragment
{
	public class ProfileFragment : BaseFragment, IUserProfileView
	{
		UserProfilePresenter _presenter;
		private string _profileId;
		private UserProfileResponse _profile;

#pragma warning disable 0649, 4014
		[InjectView(Resource.Id.btn_back)]ImageButton _backButton;
		[InjectView(Resource.Id.profile_name)]TextView _profileName;
		[InjectView(Resource.Id.joined_text)]TextView _joinedText;
		[InjectView(Resource.Id.profile_image)]CircleImageView _profileImage;
		[InjectView(Resource.Id.cost_btn)]Button _costButton;
        [InjectView(Resource.Id.follow_cont)]RelativeLayout _followCont;
        [InjectView(Resource.Id.follow_btn)]Button _followBtn;
		[InjectView(Resource.Id.description)]TextView _description;
		[InjectView(Resource.Id.place)]TextView _place;
		[InjectView(Resource.Id.site)]TextView _site;
		[InjectView(Resource.Id.btn_switcher)]ImageButton _switcher;
		[InjectView(Resource.Id.photos_count)]TextView _photosCount;
		[InjectView(Resource.Id.following_count)]TextView _followingCount;
		[InjectView(Resource.Id.followers_count)]TextView _followersCount;
		[InjectView(Resource.Id.posts_list)]RecyclerView _postsList;
		[InjectView(Resource.Id.loading_spinner)]ProgressBar _spinner;
		[InjectView(Resource.Id.cl_profile)]CoordinatorLayout _content;
		[InjectView(Resource.Id.refresher)] SwipeRefreshLayout _refresher;
		[InjectView(Resource.Id.follow_spinner)] ProgressBar _followSpinner;
		[InjectView(Resource.Id.btn_settings)] ImageButton _settings;
#pragma warning restore 0649

		public ProfileFragment(string profileId)
		{
			_profileId = profileId;
		}

		public override Android.Views.View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			if (!IsInitialized)
			{
				V = inflater.Inflate(Resource.Layout.lyt_fragment_profile, null);
				Cheeseknife.Inject(this, V);
			}
			return V;
		}

		public override void OnViewCreated(Android.Views.View view, Bundle savedInstanceState)
		{
			if (IsInitialized)
				return;
			base.OnViewCreated(view, savedInstanceState);
			_backButton.Visibility = ViewStates.Gone;
			if (_profileId == BasePresenter.User.Login)
				_followCont.Visibility = ViewStates.Gone;
			else
				_settings.Visibility = ViewStates.Gone;

			_postsList.SetLayoutManager(new GridLayoutManager(Context, 3));
			_postsList.AddItemDecoration(new GridItemdecoration(2, 3));
			_postsList.AddOnScrollListener(new FeedsScrollListener(_presenter));
			_postsList.SetAdapter(GridAdapter);

			_refresher.Refresh += async delegate
			{
				await UpdateProfile();
				_refresher.Refreshing = false;
			};
            LoadProfile();
		}

		[InjectOnClick(Resource.Id.btn_settings)]
		public void OnSettingsClick(object sender, EventArgs e)
		{
			var intent = new Intent(Context, typeof(SettingsActivity));
			StartActivity(intent);
		}

		[InjectOnClick(Resource.Id.follow_btn)]
		public async void OnFollowClick(object sender, EventArgs e)
		{
			try
			{
				_followSpinner.Visibility = ViewStates.Visible;
				_followBtn.Visibility = ViewStates.Invisible;
				var resp = await _presenter.Follow(_profile.HasFollowed);
				if (_followBtn == null)
					return;
				if (resp.Errors.Count == 0)
				{
					_followBtn.Text = (resp.Result.IsFollowed) ? GetString(
						Resource.String.text_unfollow) : GetString(Resource.String.text_follow);
				}
				else
				{
					Toast.MakeText(Activity, resp.Errors[0], ToastLength.Long).Show();
				}
				_followSpinner.Visibility = ViewStates.Invisible;
				_followBtn.Visibility = ViewStates.Visible;
			}
			catch (Exception ex)
			{
				Reporter.SendCrash(ex, BasePresenter.User.Login, BasePresenter.AppVersion);
			}
		}

        [InjectOnClick(Resource.Id.following_btn)]
		public void OnFollowingClick(object sender, EventArgs e)
		{
			Activity.Intent.PutExtra("isFollowers", true);
			Activity.Intent.PutExtra("username", _profileId);
			((BaseActivity)Activity).OpenNewContentFragment(new FollowersFragment());
		}

		[InjectOnClick(Resource.Id.followers_btn)]
		public void OnFollowersClick(object sender, EventArgs e)
		{
			Activity.Intent.PutExtra("isFollowers", true);
			Activity.Intent.PutExtra("username", _profileId);
			((BaseActivity)Activity).OpenNewContentFragment(new FollowersFragment());
		}

        FeedAdapter _feedAdapter;
        FeedAdapter FeedAdapter
        {
            get
            {
                if (_feedAdapter == null)
                {
                    _feedAdapter = new FeedAdapter(Context, _presenter.UserPosts);
                    _feedAdapter.PhotoClick += OnClick;
                    _feedAdapter.LikeAction += FeedAdapter_LikeAction;
                    _feedAdapter.UserAction += FeedAdapter_UserAction;
                    _feedAdapter.CommentAction += FeedAdapter_CommentAction;
					_feedAdapter.VotersClick += FeedAdapter_VotersAction;
                }
                return _feedAdapter;
            }
        }

        PostsGridAdapter _gridAdapter;
        PostsGridAdapter GridAdapter
        {
            get
            {
                if (_gridAdapter == null)
                {
                    _gridAdapter = new PostsGridAdapter(Context, _presenter.UserPosts);
                    _gridAdapter.Click += OnClick;
                }
                return _gridAdapter;
            }
        }

		public override bool CustomUserVisibleHint
		{
			get
			{
				return base.CustomUserVisibleHint;
			}
			set
			{
				if (value && BasePresenter.ShouldUpdateProfile)
				{
                    UpdateProfile();
				    BasePresenter.ShouldUpdateProfile = false;
				}
				UserVisibleHint = value;
			}
		}

        public void OnClick(int position)
        {
            Intent intent = new Intent(Context,typeof(PostPreviewActivity));
            intent.PutExtra("PhotoURL", _presenter.UserPosts[position].Body);
            StartActivity(intent);
        }

		public async Task UpdateProfile()
		{
			_presenter.ClearPosts();
			await LoadProfile(true);
		}

        [InjectOnClick(Resource.Id.btn_switcher)]
		public void OnSwitcherClick(object sender, EventArgs e)
		{
			if (_postsList.GetLayoutManager() is GridLayoutManager)
			{
				_switcher.SetImageResource(Resource.Drawable.ic_grid_new);
				_postsList.SetLayoutManager(new LinearLayoutManager(Context));
				_postsList.SetAdapter(FeedAdapter);
			}
			else
			{ 
				_switcher.SetImageResource(Resource.Drawable.ic_list);
				_postsList.SetLayoutManager(new GridLayoutManager(Context, 3));
				_postsList.AddItemDecoration(new GridItemdecoration(2, 3));
				_postsList.SetAdapter(GridAdapter);
			}
		}

		private async Task LoadProfile(bool needRefresh = false)
		{
			_presenter.GetUserPosts(needRefresh);
			_profile = await _presenter.GetUserInfo(_profileId, needRefresh);
            if (_profile != null)
            {
				var culture = new CultureInfo("en-US");
				_joinedText.Text = $"Joined {_profile.Created.ToString("Y", culture)}";
				if (!string.IsNullOrEmpty(_profile.Location))
					_place.Text = _profile.Location;
				else
					_place.Visibility = ViewStates.Gone;
				
				if (!string.IsNullOrEmpty(_profile.About))
					_description.Text = _profile.About;
				else
					_description.Visibility = ViewStates.Gone;

				_profileName.Text = string.IsNullOrEmpty(_profile.Name) ? _profile.Username : _profile.Name;
                if(!string.IsNullOrEmpty(_profile.ProfileImage))
					Picasso.With(Context).Load(_profile.ProfileImage).Placeholder(Resource.Drawable.ic_user_placeholder).Resize(_profileImage.Width, 0).Into(_profileImage);
                else
                    Picasso.With(Context).Load(Resource.Drawable.ic_user_placeholder).Resize(_profileImage.Width, 0).Into(_profileImage);
				_costButton.Text = (string.Format(GetString(Resource.String.cost_param_on_balance), _profile.EstimatedBalance, BasePresenter.Currency));
                _photosCount.Text = _profile.PostCount.ToString();
                _site.Text = _profile.Website;
                if(!string.IsNullOrEmpty(_profile.Location))
                    _place.Text = _profile.Location.Trim();
                _followingCount.Text = _profile.FollowingCount.ToString();
                _followersCount.Text = _profile.FollowersCount.ToString();
                _spinner.Visibility = ViewStates.Gone;
                _content.Visibility = ViewStates.Visible;
				_followBtn.Text = (_profile.HasFollowed == 0) ? GetString(Resource.String.text_follow) : GetString(Resource.String.text_unfollow);
            }
            else
            {
				Reporter.SendCrash("Profile data = null(Profile fragment)", BasePresenter.User.Login, BasePresenter.AppVersion);
                Toast.MakeText(Context, "Profile loading error. Try relaunch app", ToastLength.Short).Show();
            }
		}

		public override void OnDetach()
		{
			base.OnDetach();
			Cheeseknife.Reset(this);
		}

		protected override void CreatePresenter()
		{
			_presenter = new UserProfilePresenter(this, _profileId);
			_presenter.PostsLoaded += () =>
			{
				Activity.RunOnUiThread(() =>
				{
					_postsList?.GetAdapter()?.NotifyDataSetChanged();
				});
			};
			_presenter.PostsCleared += () =>
			{
				Activity.RunOnUiThread(() =>
				{
					_postsList?.GetAdapter()?.NotifyDataSetChanged();
				});
			};
		}

        void FeedAdapter_CommentAction(int position)
        {
            Intent intent = new Intent(Context, typeof(CommentsActivity));
            intent.PutExtra("uid", _presenter.UserPosts[position].Url);
            Context.StartActivity(intent);
        }

		void FeedAdapter_VotersAction(int position)
		{
			Activity.Intent.PutExtra("url", _presenter.UserPosts[position].Url);
			((BaseActivity)Activity).OpenNewContentFragment(new VotersFragment());
		}

        void FeedAdapter_UserAction(int position)
        {
			if (_profileId != _presenter.UserPosts[position].Author)
				((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(_presenter.UserPosts[position].Author));
        }

        async void FeedAdapter_LikeAction(int position)
        {
			try
			{
				if (BasePresenter.User.IsAuthenticated)
				{
					var response = await _presenter.Vote(_presenter.UserPosts[position]);

					if (response.Success)
					{
						_presenter.UserPosts[position].Vote = !_presenter.UserPosts[position].Vote;
						if (response.Result.IsVoted)
							_presenter.UserPosts[position].NetVotes++;
						else
							_presenter.UserPosts[position].NetVotes--;
						_presenter.UserPosts[position].TotalPayoutReward = response.Result.NewTotalPayoutReward;
						_postsList?.GetAdapter()?.NotifyDataSetChanged();
					}
					else
					{
						//TODO:KOA Show error
					}
				}
				else
				{
					var intent = new Intent(Context, typeof(SignInActivity));
					StartActivity(intent);
				}
			}
			catch (Exception ex)
			{
				Reporter.SendCrash(ex, BasePresenter.User.Login, BasePresenter.AppVersion);
			}
        }

		private class FeedsScrollListener : RecyclerView.OnScrollListener
		{
			UserProfilePresenter _presenter;
			int _prevPos = 0;
			public FeedsScrollListener(UserProfilePresenter presenter)
			{
				_presenter = presenter;
				presenter.PostsCleared += () =>
				{
					_prevPos = 0;
				};
			}

			public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
			{
				int pos = ((LinearLayoutManager)recyclerView.GetLayoutManager()).FindLastVisibleItemPosition();
				if (pos > _prevPos && pos != _prevPos)
				{
					if (pos == recyclerView.GetAdapter().ItemCount - 1)
					{
						if (pos < (recyclerView.GetAdapter()).ItemCount)
						{
							_presenter.GetUserPosts();
							_prevPos = pos;
						}
					}
				}
			}
		}
    }
}
