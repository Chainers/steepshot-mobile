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
using Steemix.Droid.Activities;

namespace Steemix.Droid.Fragments
{
	public class ProfileFragment : BaseFragment<ViewModels.ProfileViewModel>
	{
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
			FollowButton.Visibility = ViewStates.Gone;
			PostsList.SetLayoutManager(new GridLayoutManager(Context, 3));
			PostsList.AddItemDecoration(new GridItemdecoration(2, 3));
			LoadProfile();
		}

		[InjectOnClick(Resource.Id.btn_settings)]
		public void OnSettingsClick(object sender, EventArgs e)
		{
			var intent = new Intent(Context, typeof(SettingsActivity));
			StartActivity(intent);
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

		[InjectOnClick(Resource.Id.btn_switcher)]
		public void OnSwitcherClick(object sender, EventArgs e)
		{
			if (PostsList.GetLayoutManager() is GridLayoutManager)
			{
				Switcher.SetImageResource(Resource.Drawable.ic_gray_grid);
				PostsList.SetLayoutManager(new LinearLayoutManager(Context));
				PostsList.SetAdapter(new Adapter.FeedAdapter(Context, ViewModel.UserPosts));
			}
			else
			{ 
				Switcher.SetImageResource(Resource.Drawable.ic_gray_list);
				PostsList.SetLayoutManager(new GridLayoutManager(Context, 3));
				PostsList.AddItemDecoration(new GridItemdecoration(2, 3));
				PostsList.SetAdapter(new Adapter.PostsGridAdapter(Context, ViewModel.UserPosts));
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
			var Profile = await ViewModel.GetUserInfo(UserPrincipal.CurrentUser.Login);
			ProfileName.Text = Profile.Username;
			JoinedText.Text = Profile.LastAccountUpdate.ToString();
			Picasso.With(this.Context).Load(Profile.ProfileImage).Resize(ProfileImage.Width, ProfileImage.Width).Into(ProfileImage);
			CostButton.Text = (string.Format(GetString(Resource.String.cost_param_on_balance),Profile.EstimatedBalance));
			PhotosCount.Text = Profile.PostCount.ToString();
			FollowingCount.Text = Profile.FollowingCount.ToString();
			FollowersCount.Text = Profile.FollowersCount.ToString();
			spinner.Visibility = ViewStates.Gone;
			Content.Visibility = ViewStates.Visible;
			var Posts = await ViewModel.GetUserPosts();
			PostsList.SetAdapter(new Adapter.PostsGridAdapter(Context, ViewModel.UserPosts));
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();
			Cheeseknife.Reset(this);
		}
	}
}
