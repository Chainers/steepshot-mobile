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

namespace Steemix.Droid.Activities
{
	[Activity(Label = "ProfileActivity",ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class ProfileActivity : BaseActivity<ViewModels.UserProfileViewModel>
	{
		string ProfileId;
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

		[InjectOnClick(Resource.Id.follow_btn)]
		public void OnFollowClick(object sender, EventArgs e)
		{
			Toast.MakeText(this, "TODO", ToastLength.Short).Show();
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
			StartActivity(intent);
		}

		[InjectOnClick(Resource.Id.followers_btn)]
		public void OnFollowersClick(object sender, EventArgs e)
		{
			var intent = new Intent(this, typeof(FollowersActivity));
			intent.PutExtra("isFollowers", true);
			StartActivity(intent);
		}

		[InjectOnClick(Resource.Id.btn_switcher)]
		public void OnSwitcherClick(object sender, EventArgs e)
		{
			if (PostsList.GetLayoutManager() is GridLayoutManager)
			{
				Switcher.SetImageResource(Resource.Drawable.ic_gray_grid);
				PostsList.SetLayoutManager(new LinearLayoutManager(this));
				PostsList.SetAdapter(new Adapter.FeedAdapter(this, ViewModel.UserPosts));
			}
			else
			{
				Switcher.SetImageResource(Resource.Drawable.ic_gray_list);
				PostsList.SetLayoutManager(new GridLayoutManager(this, 3));
				PostsList.AddItemDecoration(new GridItemdecoration(2, 3));
				PostsList.SetAdapter(new Adapter.PostsGridAdapter(this, ViewModel.UserPosts));
			}
		}

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

		private async void LoadProfile()
		{
			var Profile = await ViewModel.GetUserInfo(ProfileId);

			if (UserPrincipal.CurrentUser.Login.Equals(Profile.Username))
			{ 
				FollowButton.Visibility = ViewStates.Gone;
			}

			ProfileName.Text = Profile.Username;
			JoinedText.Text = Profile.LastAccountUpdate.ToString();
			Picasso.With(this).Load(Profile.ProfileImage).Into(ProfileImage);
			CostButton.Text = (string.Format(GetString(Resource.String.cost_param_on_balance), Profile.EstimatedBalance));
			PhotosCount.Text = Profile.PostCount.ToString();
			FollowingCount.Text = Profile.FollowingCount.ToString();
			FollowersCount.Text = Profile.FollowersCount.ToString();
			spinner.Visibility = ViewStates.Gone;
			Content.Visibility = ViewStates.Visible;
			await ViewModel.GetUserPosts();
			PostsList.SetAdapter(new Adapter.PostsGridAdapter(this, ViewModel.UserPosts));
		}
	}
}
