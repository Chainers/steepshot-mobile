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
using Sweetshot.Library.Models.Responses;

namespace Steepshot
{
	public class ProfileFragment : BaseFragment, UserProfileView
	{
		UserProfilePresenter presenter;
		private string _profileId;
		private UserProfileResponse _profile;

#pragma warning disable 0649, 4014
		[InjectView(Resource.Id.btn_back)]ImageButton backButton;
		[InjectView(Resource.Id.profile_name)]TextView ProfileName;
		[InjectView(Resource.Id.joined_text)]TextView JoinedText;
		[InjectView(Resource.Id.profile_image)]CircleImageView ProfileImage;
		[InjectView(Resource.Id.cost_btn)]Button CostButton;
        [InjectView(Resource.Id.follow_cont)]RelativeLayout FollowCont;
        [InjectView(Resource.Id.follow_btn)]Button FollowBtn;
		[InjectView(Resource.Id.description)]TextView Description;
		[InjectView(Resource.Id.place)]TextView Place;
		[InjectView(Resource.Id.site)]TextView Site;
		[InjectView(Resource.Id.btn_switcher)]ImageButton Switcher;
		[InjectView(Resource.Id.photos_count)]TextView PhotosCount;
		[InjectView(Resource.Id.following_count)]TextView FollowingCount;
		[InjectView(Resource.Id.followers_count)]TextView FollowersCount;
		[InjectView(Resource.Id.posts_list)]RecyclerView PostsList;
		[InjectView(Resource.Id.loading_spinner)]ProgressBar spinner;
		[InjectView(Resource.Id.cl_profile)]CoordinatorLayout Content;
		[InjectView(Resource.Id.refresher)] SwipeRefreshLayout refresher;
		[InjectView(Resource.Id.follow_spinner)] ProgressBar FollowSpinner;
		[InjectView(Resource.Id.btn_settings)] ImageButton Settings;
#pragma warning restore 0649

		public ProfileFragment(string profileId)
		{
			_profileId = profileId;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			if (!_isInitialized)
			{
				v = inflater.Inflate(Resource.Layout.lyt_fragment_profile, null);
				Cheeseknife.Inject(this, v);
			}
			return v;
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			if (_isInitialized)
				return;
			base.OnViewCreated(view, savedInstanceState);
			backButton.Visibility = ViewStates.Gone;
			if (_profileId == BasePresenter.User.Login)
				FollowCont.Visibility = ViewStates.Gone;
			else
				Settings.Visibility = ViewStates.Gone;

			PostsList.SetLayoutManager(new GridLayoutManager(Context, 3));
			PostsList.AddItemDecoration(new GridItemdecoration(2, 3));
			PostsList.AddOnScrollListener(new FeedsScrollListener(presenter));
			PostsList.SetAdapter(GridAdapter);

			refresher.Refresh += async delegate
			{
				await UpdateProfile();
				refresher.Refreshing = false;
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
				FollowSpinner.Visibility = ViewStates.Visible;
				FollowBtn.Visibility = ViewStates.Invisible;
				var resp = await presenter.Follow(_profile.HasFollowed);
				if (FollowBtn == null)
					return;
				if (resp.Errors.Count == 0)
				{
					FollowBtn.Text = (resp.Result.IsFollowed) ? GetString(
						Resource.String.text_unfollow) : GetString(Resource.String.text_follow);
				}
				else
				{
					Toast.MakeText(Activity, resp.Errors[0], ToastLength.Long).Show();
				}
				FollowSpinner.Visibility = ViewStates.Invisible;
				FollowBtn.Visibility = ViewStates.Visible;
			}
			catch (Exception ex)
			{
				Reporter.SendCrash(ex);
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
                    _feedAdapter = new FeedAdapter(Context, presenter.UserPosts);
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
                    _gridAdapter = new PostsGridAdapter(Context, presenter.UserPosts);
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
				base.UserVisibleHint = value;
			}
		}

        public void OnClick(int position)
        {
            Intent intent = new Intent(this.Context,typeof(PostPreviewActivity));
            intent.PutExtra("PhotoURL", presenter.UserPosts[position].Body);
            StartActivity(intent);
        }

		public async Task UpdateProfile()
		{
			presenter.ClearPosts();
			await LoadProfile(true);
		}

        [InjectOnClick(Resource.Id.btn_switcher)]
		public void OnSwitcherClick(object sender, EventArgs e)
		{
			if (PostsList.GetLayoutManager() is GridLayoutManager)
			{
				Switcher.SetImageResource(Resource.Drawable.ic_grid_new);
				PostsList.SetLayoutManager(new LinearLayoutManager(Context));
				PostsList.SetAdapter(FeedAdapter);
			}
			else
			{ 
				Switcher.SetImageResource(Resource.Drawable.ic_list);
				PostsList.SetLayoutManager(new GridLayoutManager(Context, 3));
				PostsList.AddItemDecoration(new GridItemdecoration(2, 3));
				PostsList.SetAdapter(GridAdapter);
			}
		}

		private async Task LoadProfile(bool needRefresh = false)
		{
			presenter.GetUserPosts(needRefresh);
			_profile = await presenter.GetUserInfo(_profileId, needRefresh);
            if (_profile != null)
            {
				var culture = new CultureInfo("en-US");
				JoinedText.Text = $"Joined {_profile.Created.ToString("Y", culture)}";
				if (!string.IsNullOrEmpty(_profile.Location))
					Place.Text = _profile.Location;
				else
					Place.Visibility = ViewStates.Gone;
				
				if (!string.IsNullOrEmpty(_profile.About))
					Description.Text = _profile.About;
				else
					Description.Visibility = ViewStates.Gone;

				ProfileName.Text = string.IsNullOrEmpty(_profile.Name) ? _profile.Username : _profile.Name;
                if(!string.IsNullOrEmpty(_profile.ProfileImage))
					Picasso.With(this.Context).Load(_profile.ProfileImage).Placeholder(Resource.Drawable.ic_user_placeholder).Resize(ProfileImage.Width, 0).Into(ProfileImage);
                else
                    Picasso.With(this.Context).Load(Resource.Drawable.ic_user_placeholder).Resize(ProfileImage.Width, 0).Into(ProfileImage);
				CostButton.Text = (string.Format(GetString(Resource.String.cost_param_on_balance), _profile.EstimatedBalance, BasePresenter.Currency));
                PhotosCount.Text = _profile.PostCount.ToString();
                Site.Text = _profile.Website;
                if(!string.IsNullOrEmpty(_profile.Location))
                    Place.Text = _profile.Location.Trim();
                FollowingCount.Text = _profile.FollowingCount.ToString();
                FollowersCount.Text = _profile.FollowersCount.ToString();
                spinner.Visibility = ViewStates.Gone;
                Content.Visibility = ViewStates.Visible;
				FollowBtn.Text = (_profile.HasFollowed == 0) ? GetString(Resource.String.text_follow) : GetString(Resource.String.text_unfollow);
            }
            else
            {
				Reporter.SendCrash("Profile data = null(Profile fragment)");
                Toast.MakeText(this.Context, "Profile loading error. Try relaunch app", ToastLength.Short).Show();
            }
		}

		public override void OnDetach()
		{
			base.OnDetach();
			Cheeseknife.Reset(this);
		}

		protected override void CreatePresenter()
		{
			presenter = new UserProfilePresenter(this, _profileId);
			presenter.PostsLoaded += () =>
			{
				Activity.RunOnUiThread(() =>
				{
					PostsList?.GetAdapter()?.NotifyDataSetChanged();
				});
			};
			presenter.PostsCleared += () =>
			{
				Activity.RunOnUiThread(() =>
				{
					PostsList?.GetAdapter()?.NotifyDataSetChanged();
				});
			};
		}

        void FeedAdapter_CommentAction(int position)
        {
            Intent intent = new Intent(this.Context, typeof(CommentsActivity));
            intent.PutExtra("uid", presenter.UserPosts[position].Url);
            this.Context.StartActivity(intent);
        }

		void FeedAdapter_VotersAction(int position)
		{
			Activity.Intent.PutExtra("url", presenter.UserPosts[position].Url);
			((BaseActivity)Activity).OpenNewContentFragment(new VotersFragment());
		}

        void FeedAdapter_UserAction(int position)
        {
			if (_profileId != presenter.UserPosts[position].Author)
				((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(presenter.UserPosts[position].Author));
        }

        async void FeedAdapter_LikeAction(int position)
        {
			try
			{
				if (BasePresenter.User.IsAuthenticated)
				{
					var response = await presenter.Vote(presenter.UserPosts[position]);

					if (response.Success)
					{
						presenter.UserPosts[position].Vote = !presenter.UserPosts[position].Vote;
						if (response.Result.IsVoted)
							presenter.UserPosts[position].NetVotes++;
						else
							presenter.UserPosts[position].NetVotes--;
						presenter.UserPosts[position].TotalPayoutReward = response.Result.NewTotalPayoutReward;
						PostsList?.GetAdapter()?.NotifyDataSetChanged();
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
				Reporter.SendCrash(ex);
			}
        }

		private class FeedsScrollListener : RecyclerView.OnScrollListener
		{
			UserProfilePresenter presenter;
			int prevPos = 0;
			public FeedsScrollListener(UserProfilePresenter presenter)
			{
				this.presenter = presenter;
				presenter.PostsCleared += () =>
				{
					prevPos = 0;
				};
			}

			public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
			{
				int pos = ((LinearLayoutManager)recyclerView.GetLayoutManager()).FindLastVisibleItemPosition();
				if (pos > prevPos && pos != prevPos)
				{
					if (pos == recyclerView.GetAdapter().ItemCount - 1)
					{
						if (pos < (recyclerView.GetAdapter()).ItemCount)
						{
							presenter.GetUserPosts();
							prevPos = pos;
						}
					}
				}
			}
		}
    }
}
