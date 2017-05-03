using System;
using Android.App;
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
	[Activity(Label = "ProfileActivity",ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class ProfileActivity : BaseActivity,UserProfileView
	{
		UserProfilePresenter presenter;

		string ProfileId;

		[InjectView(Resource.Id.btn_back)]
		ImageButton backButton;

		[InjectView(Resource.Id.profile_name)]
		TextView ProfileName;

		[InjectView(Resource.Id.joined_text)]
		TextView JoinedText;

		[InjectView(Resource.Id.profile_image)]
		CircleImageView ProfileImage;

		[InjectView(Resource.Id.cost_btn)]
		Button CostButton;

		[InjectView(Resource.Id.follow_btn)]
		Button FollowButton;

		[InjectView(Resource.Id.description)]
		TextView Description;

		[InjectView(Resource.Id.place)]
		TextView Place;

		[InjectView(Resource.Id.site)]
		TextView Site;

		[InjectView(Resource.Id.btn_switcher)]
		ImageButton Switcher;

		[InjectView(Resource.Id.photos_count)]
		TextView PhotosCount;

		[InjectView(Resource.Id.following_count)]
		TextView FollowingCount;

		[InjectView(Resource.Id.followers_count)]
		TextView FollowersCount;

		[InjectView(Resource.Id.posts_list)]
		RecyclerView PostsList;

		[InjectView(Resource.Id.loading_spinner)]
		ProgressBar spinner;

		[InjectView(Resource.Id.cl_profile)]
		CoordinatorLayout Content;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.lyt_fragment_profile);
			Cheeseknife.Inject(this);
			ProfileId = Intent.GetStringExtra("ID");
			Settings.Visibility = ViewStates.Gone;
			PostsList.SetLayoutManager(new GridLayoutManager(this, 3));
			PostsList.AddItemDecoration(new GridItemdecoration(2, 3));
		}

		protected override void OnResume()
		{
			base.OnResume();
			LoadProfile();
		}


		[InjectView(Resource.Id.btn_settings)]
		ImageButton Settings;

        [InjectView(Resource.Id.follow_btn)]
        Button FollowBtn;

        [InjectView(Resource.Id.follow_spinner)]
        ProgressBar FollowSpinner;

        [InjectOnClick(Resource.Id.follow_btn)]
		public async void OnFollowClick(object sender, EventArgs e)
		{
            FollowSpinner.Visibility = ViewStates.Visible;
            FollowBtn.Visibility = ViewStates.Invisible;
            var resp = await presenter.Follow();
            if (resp.Errors.Count == 0)
            {
                FollowBtn.Text = (resp.Result.IsFollowed) ? GetString(
                    Resource.String.text_unfollow) : GetString(Resource.String.text_follow);
            }
            else
            {
                Toast.MakeText(this, resp.Errors[0], ToastLength.Long).Show();
            }
            FollowSpinner.Visibility = ViewStates.Invisible;
            FollowBtn.Visibility = ViewStates.Visible;
        }

		[InjectOnClick(Resource.Id.btn_back)]
		public void OnBackClick(object sender, EventArgs e)
		{
			OnBackPressed();
		}


		[InjectOnClick(Resource.Id.following_btn)]
		public void OnFollowingClick(object sender, EventArgs e)
		{
			var intent = new Intent(this, typeof(FollowersActivity));
			intent.PutExtra("isFollowers", false);
			intent.PutExtra("username", ProfileId);
			StartActivity(intent);
		}

		[InjectOnClick(Resource.Id.followers_btn)]
		public void OnFollowersClick(object sender, EventArgs e)
		{
			var intent = new Intent(this, typeof(FollowersActivity));
			intent.PutExtra("isFollowers", true);
			intent.PutExtra("username", ProfileId);
			StartActivity(intent);
		}

        FeedAdapter _feedAdapter;
        FeedAdapter FeedAdapter
        {
            get
            {
                if (_feedAdapter == null)
                {
                    _feedAdapter = new FeedAdapter(this, presenter.UserPosts);
                    _feedAdapter.PhotoClick += OnClick;
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
                    _gridAdapter = new PostsGridAdapter(this, presenter.UserPosts);
                    _gridAdapter.Click += OnClick;
                }
                return _gridAdapter;
            }
        }

        [InjectOnClick(Resource.Id.btn_switcher)]
		public void OnSwitcherClick(object sender, EventArgs e)
		{
            if (PostsList.GetLayoutManager() is GridLayoutManager)
            {
                Switcher.SetImageResource(Resource.Drawable.ic_gray_grid);
                PostsList.SetLayoutManager(new LinearLayoutManager(this));
                PostsList.SetAdapter(FeedAdapter);
            }
            else
            {
                Switcher.SetImageResource(Resource.Drawable.ic_gray_list);
                PostsList.SetLayoutManager(new GridLayoutManager(this, 3));
                PostsList.AddItemDecoration(new GridItemdecoration(2, 3));
                PostsList.SetAdapter(GridAdapter);
            }
        }

        public void OnClick(int position)
        {
            Intent intent = new Intent(this, typeof(PostPreviewActivity));
            intent.PutExtra("PhotoURL", presenter.UserPosts[position].Body);
            StartActivity(intent);
        }

        private async void LoadProfile()
        {
            var Profile = await presenter.GetUserInfo(ProfileId,true);
            if (Profile != null)
            {
                ProfileName.Text = Profile.Username;
                JoinedText.Text = Profile.LastAccountUpdate.ToString();
                if (!string.IsNullOrEmpty(Profile.ProfileImage))
                    Picasso.With(this).Load(Profile.ProfileImage).Resize(ProfileImage.Width, ProfileImage.Width).Into(ProfileImage);
                else
                    Picasso.With(this).Load(Resource.Drawable.ic_user_placeholder).Resize(ProfileImage.Width, ProfileImage.Width).Into(ProfileImage);
                CostButton.Text = (string.Format(GetString(Resource.String.cost_param_on_balance), Profile.EstimatedBalance));
                PhotosCount.Text = Profile.PostCount.ToString();
                Description.Text = Profile.About;
                //Site.Text = Profile.WebSite;
                if (!string.IsNullOrEmpty(Profile.Location))
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

                FollowBtn.Text = (Profile.HasFollowed==0) ? GetString(
                Resource.String.text_follow) : GetString(Resource.String.text_unfollow);
               // Toast.MakeText(this, (Profile.HasFollowed==0)? "You can follow this user" : "You followed this user", ToastLength.Long).Show();
            }
            else
            {
                Toast.MakeText(this, "Profile loading error. Try relaunch app", ToastLength.Short).Show();
            }
        }

        protected override void CreatePresenter()
		{
			presenter = new UserProfilePresenter(this);
		}
	}
}
