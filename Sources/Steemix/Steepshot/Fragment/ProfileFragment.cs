using System;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Refractored.Controls;
using Square.Picasso;

namespace Steepshot
{
	public class ProfileFragment : BaseFragment, UserProfileView
	{
		UserProfilePresenter presenter;

		[InjectView(Resource.Id.btn_back)]ImageButton backButton;
		[InjectView(Resource.Id.profile_name)]TextView ProfileName;
		[InjectView(Resource.Id.joined_text)]TextView JoinedText;
		[InjectView(Resource.Id.profile_image)]CircleImageView ProfileImage;
		[InjectView(Resource.Id.cost_btn)]Button CostButton;
        [InjectView(Resource.Id.follow_cont)]RelativeLayout FollowCont;
        [InjectView(Resource.Id.follow_btn)]Button FollowButton;
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

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var v = inflater.Inflate(Resource.Layout.lyt_fragment_profile, null);
			Cheeseknife.Inject(this, v);
			return v;
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);
			backButton.Visibility = ViewStates.Gone;
			FollowCont.Visibility = ViewStates.Gone;
			PostsList.SetLayoutManager(new GridLayoutManager(Context, 3));
			PostsList.AddItemDecoration(new GridItemdecoration(2, 3));
		}

		[InjectOnClick(Resource.Id.btn_settings)]
		public void OnSettingsClick(object sender, EventArgs e)
		{
			var intent = new Intent(Context, typeof(SettingsActivity));
			StartActivity(intent);
		}

        public override void OnResume()
        {
            base.OnResume();

            if(PostsList.GetAdapter()!=null)
                PostsList.GetAdapter().NotifyDataSetChanged();

            LoadProfile();
        }

        [InjectOnClick(Resource.Id.following_btn)]
		public void OnFollowingClick(object sender, EventArgs e)
		{
			var intent = new Intent(Context, typeof(FollowersActivity));
			intent.PutExtra("isFollowers", false);
			StartActivity(intent);
		}

		[InjectOnClick(Resource.Id.followers_btn)]
		public void OnFollowersClick(object sender, EventArgs e)
		{
			var intent = new Intent(Context, typeof(FollowersActivity));
			intent.PutExtra("isFollowers", true);
			StartActivity(intent);
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

        public void OnClick(int position)
        {
            Intent intent = new Intent(this.Context,typeof(PostPreviewActivity));
            intent.PutExtra("PhotoURL", presenter.UserPosts[position].Body);
            StartActivity(intent);
        }

        [InjectOnClick(Resource.Id.btn_switcher)]
		public void OnSwitcherClick(object sender, EventArgs e)
		{
			if (PostsList.GetLayoutManager() is GridLayoutManager)
			{
				Switcher.SetImageResource(Resource.Drawable.ic_gray_grid);
				PostsList.SetLayoutManager(new LinearLayoutManager(Context));
				PostsList.SetAdapter(FeedAdapter);
			}
			else
			{ 
				Switcher.SetImageResource(Resource.Drawable.ic_gray_list);
				PostsList.SetLayoutManager(new GridLayoutManager(Context, 3));
				PostsList.AddItemDecoration(new GridItemdecoration(2, 3));
				PostsList.SetAdapter(GridAdapter);
			}
		}



		private async void LoadProfile()
		{
			var Profile = await presenter.GetUserInfo(UserPrincipal.Instance.CurrentUser.Login);
            if (Profile != null)
            {
                ProfileName.Text = Profile.Username;
                JoinedText.Text = Profile.LastAccountUpdate.ToString();
                if(!string.IsNullOrEmpty(Profile.ProfileImage))
                    Picasso.With(this.Context).Load(Profile.ProfileImage).Resize(ProfileImage.Width, ProfileImage.Width).Into(ProfileImage);
                else
                    Picasso.With(this.Context).Load(Resource.Drawable.ic_user_placeholder).Resize(ProfileImage.Width, ProfileImage.Width).Into(ProfileImage);
                CostButton.Text = (string.Format(GetString(Resource.String.cost_param_on_balance), Profile.EstimatedBalance));
                PhotosCount.Text = Profile.PostCount.ToString();
                Description.Text = Profile.About;
                Site.Text = Profile.Website;
                if(!string.IsNullOrEmpty(Profile.Location))
                    Place.Text = Profile.Location.Trim();
                FollowingCount.Text = Profile.FollowingCount.ToString();
                FollowersCount.Text = Profile.FollowersCount.ToString();
                spinner.Visibility = ViewStates.Gone;
                Content.Visibility = ViewStates.Visible;
				var Posts = await presenter.GetUserPosts();
                if (PostsList.GetAdapter() == null)
                    PostsList.SetAdapter(GridAdapter);
                else
                    PostsList.GetAdapter().NotifyDataSetChanged();
            }
            else
            {
                Toast.MakeText(this.Context, "Profile loading error. Try relaunch app", ToastLength.Short).Show();
            }
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();
			Cheeseknife.Reset(this);
		}

		protected override void CreatePresenter()
		{
			presenter = new UserProfilePresenter(this);
		}

        void FeedAdapter_CommentAction(int position)
        {
            Intent intent = new Intent(this.Context, typeof(CommentsActivity));
            intent.PutExtra("uid", presenter.UserPosts[position].Url);
            this.Context.StartActivity(intent);
        }

        void FeedAdapter_UserAction(int position)
        {
            Intent intent = new Intent(this.Context, typeof(ProfileActivity));
            intent.PutExtra("ID", presenter.UserPosts[position].Author);
            this.Context.StartActivity(intent);
        }

        async void FeedAdapter_LikeAction(int position)
        {
            if (UserPrincipal.Instance.IsAuthenticated)
            {
                var response = await presenter.Vote(presenter.UserPosts[position]);

                if (response.Success)
                {
                    presenter.UserPosts[position].Vote = !presenter.UserPosts[position].Vote;
                    FeedAdapter.NotifyDataSetChanged();
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
    }
}
