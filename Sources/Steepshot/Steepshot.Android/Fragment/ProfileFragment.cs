using System;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Activity;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Presenters;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class ProfileFragment : BaseFragmentWithPresenter<UserProfilePresenter>
    {
        private readonly string _profileId;
        private Typeface _font;
        private Typeface _semiboldFont;
        private ScrollListener _scrollListner;
        private LinearLayoutManager _linearLayoutManager;
        private GridLayoutManager _gridLayoutManager;
        private GridItemDecoration _gridItemDecoration;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.btn_back)] ImageButton _backButton;
        [InjectView(Resource.Id.btn_switcher)] ImageButton _switcher;
        [InjectView(Resource.Id.posts_list)] RecyclerView _postsList;
        [InjectView(Resource.Id.loading_spinner)] ProgressBar _loadingSpinner;
        [InjectView(Resource.Id.list_spinner)] ProgressBar _listSpinner;
        [InjectView(Resource.Id.refresher)] SwipeRefreshLayout _refresher;
        [InjectView(Resource.Id.btn_settings)] ImageButton _settings;
        [InjectView(Resource.Id.profile_login)] TextView _login;
        [InjectView(Resource.Id.list_layout)] RelativeLayout _listLayout;
#pragma warning restore 0649

        ProfileFeedAdapter _profileFeedAdapter;
        ProfileFeedAdapter ProfileFeedAdapter
        {
            get
            {
                if (_profileFeedAdapter == null)
                {
                    _profileFeedAdapter = new ProfileFeedAdapter(Context, _presenter, new[] { _font, _semiboldFont });
                    _profileFeedAdapter.PhotoClick += OnPhotoClick;
                    _profileFeedAdapter.LikeAction += LikeAction;
                    _profileFeedAdapter.UserAction += UserAction;
                    _profileFeedAdapter.CommentAction += CommentAction;
                    _profileFeedAdapter.VotersClick += VotersAction;
                    _profileFeedAdapter.FollowersAction += OnFollowersClick;
                    _profileFeedAdapter.FollowingAction += OnFollowingClick;
                    _profileFeedAdapter.FollowAction += async () => await OnFollowClick();
                }
                return _profileFeedAdapter;
            }
        }

        ProfileGridAdapter _profileGridAdapter;
        ProfileGridAdapter ProfileGridAdapter
        {
            get
            {
                if (_profileGridAdapter == null)
                {
                    _profileGridAdapter = new ProfileGridAdapter(Context, _presenter, new[] { _font, _semiboldFont });
                    _profileGridAdapter.Click += OnPhotoClick;
                    _profileGridAdapter.FollowersAction += OnFollowersClick;
                    _profileGridAdapter.FollowingAction += OnFollowingClick;
                    _profileGridAdapter.FollowAction += async () => await OnFollowClick();
                }
                return _profileGridAdapter;
            }
        }

        public override bool CustomUserVisibleHint
        {
            get => base.CustomUserVisibleHint;
            set
            {
                if (value && BasePresenter.ShouldUpdateProfile)
                {
                    UpdatePage();
                    BasePresenter.ShouldUpdateProfile = false;
                }
                UserVisibleHint = value;
            }
        }

        public ProfileFragment(string profileId)
        {
            _profileId = profileId;
        }


        protected override void CreatePresenter()
        {
            _presenter = new UserProfilePresenter(_profileId);
        }


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_fragment_profile, null);
                Cheeseknife.Inject(this, InflatedView);
            }
            return InflatedView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);

            _font = Typeface.CreateFromAsset(Android.App.Application.Context.Assets, "OpenSans-Regular.ttf");
            _semiboldFont = Typeface.CreateFromAsset(Android.App.Application.Context.Assets, "OpenSans-Semibold.ttf");
            _login.Typeface = _semiboldFont;

            if (_profileId != BasePresenter.User.Login)
            {
                _settings.Visibility = ViewStates.Invisible;
                _backButton.Visibility = ViewStates.Visible;
                _login.Text = _profileId;
            }

            _scrollListner = new ScrollListener();
            _scrollListner.ScrolledToBottom += () => GetUserPosts();

            _linearLayoutManager = new LinearLayoutManager(Context);
            _gridLayoutManager = new GridLayoutManager(Context, 3);
            _gridLayoutManager.SetSpanSizeLookup(new ProfileSpanSizeLookup());

            _gridItemDecoration = new GridItemDecoration(true);
            _postsList.SetLayoutManager(_gridLayoutManager);
            _postsList.AddItemDecoration(_gridItemDecoration);
            _postsList.AddOnScrollListener(_scrollListner);
            _postsList.SetAdapter(ProfileGridAdapter);

            _refresher.Refresh += async delegate
            {
                await UpdatePage();
                _refresher.Refreshing = false;
            };
            LoadProfile();
            GetUserPosts();
        }

        private async Task GetUserPosts(bool isRefresh = false)
        {
            var errors = await _presenter.TryLoadNextPosts(isRefresh);
            if (errors != null && errors.Count != 0)
                ShowAlert(errors);

            if (_listSpinner != null)
                _listSpinner.Visibility = ViewStates.Gone;
            _postsList?.GetAdapter()?.NotifyDataSetChanged();
        }

        [InjectOnClick(Resource.Id.btn_settings)]
        public void OnSettingsClick(object sender, EventArgs e)
        {
            var intent = new Intent(Context, typeof(SettingsActivity));
            StartActivity(intent);
        }

        private async Task UpdatePage()
        {
            _scrollListner.ClearPosition();
            await LoadProfile();
            await GetUserPosts(true);
        }

        [InjectOnClick(Resource.Id.btn_back)]
        public void GoBackClick(object sender, EventArgs e)
        {
            Activity.OnBackPressed();
        }

        [InjectOnClick(Resource.Id.btn_switcher)]
        public void OnSwitcherClick(object sender, EventArgs e)
        {
            if (_postsList.GetLayoutManager() is GridLayoutManager)
            {
                _switcher.SetImageResource(Resource.Drawable.grid);
                _postsList.SetLayoutManager(_linearLayoutManager);
                _postsList.RemoveItemDecoration(_gridItemDecoration);
                _postsList.SetAdapter(ProfileFeedAdapter);
            }
            else
            {
                _switcher.SetImageResource(Resource.Drawable.grid_active);
                _postsList.SetLayoutManager(_gridLayoutManager);
                _postsList.AddItemDecoration(_gridItemDecoration);
                _postsList.SetAdapter(ProfileGridAdapter);
            }
        }

        private async Task LoadProfile()
        {
            var response = await _presenter.TryGetUserInfo(_profileId);
            if (response != null && response.Success)
            {
                if (_listLayout != null)
                    _listLayout.Visibility = ViewStates.Visible;

                ProfileGridAdapter.ProfileData = response.Result;
                ProfileFeedAdapter.ProfileData = response.Result;
                _postsList?.GetAdapter()?.NotifyDataSetChanged();
            }
            else
            {
                ShowAlert(response);
            }
            if (_loadingSpinner != null)
                _loadingSpinner.Visibility = ViewStates.Gone;
        }

        private async Task OnFollowClick()
        {
            var response = await _presenter.TryFollow(ProfileGridAdapter.ProfileData.HasFollowed);
            if (response == null) // cancelled
                return;

            if (response.Success)
            {
                ProfileGridAdapter.ProfileData.HasFollowed = ProfileFeedAdapter.ProfileData.HasFollowed = !ProfileGridAdapter.ProfileData.HasFollowed;
                _postsList?.GetAdapter()?.NotifyDataSetChanged();
            }
            else
            {
                ShowAlert(response);
            }
        }

        public void OnPhotoClick(int position)
        {
            var post = _presenter[position];
            if (post == null)
                return;
            var photo = post.Photos?.FirstOrDefault();
            if (photo != null)
            {
                var intent = new Intent(Context, typeof(PostPreviewActivity));
                intent.PutExtra("PhotoURL", photo);
                StartActivity(intent);
            }
        }

        private void OnFollowingClick()
        {
            Activity.Intent.PutExtra("isFollowers", false);
            Activity.Intent.PutExtra("username", _profileId);
            Activity.Intent.PutExtra("count", ProfileFeedAdapter.ProfileData.FollowingCount);
            ((BaseActivity)Activity).OpenNewContentFragment(new FollowersFragment());
        }

        private void OnFollowersClick()
        {
            Activity.Intent.PutExtra("isFollowers", true);
            Activity.Intent.PutExtra("username", _profileId);
            Activity.Intent.PutExtra("count", ProfileFeedAdapter.ProfileData.FollowersCount);
            ((BaseActivity)Activity).OpenNewContentFragment(new FollowersFragment());
        }

        private void CommentAction(int position)
        {
            var post = _presenter[position];
            if (post == null)
                return;
            var intent = new Intent(Context, typeof(CommentsActivity));
            intent.PutExtra("uid", post.Url);
            Context.StartActivity(intent);
        }

        private void VotersAction(int position)
        {
            var post = _presenter[position];
            if (post == null)
                return;
            Activity.Intent.PutExtra("url", post.Url);
            ((BaseActivity)Activity).OpenNewContentFragment(new VotersFragment());
        }

        private void UserAction(int position)
        {
            var post = _presenter[position];
            if (post == null)
                return;
            if (_profileId != post.Author)
                ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(post.Author));
        }

        private async void LikeAction(int position)
        {
            if (BasePresenter.User.IsAuthenticated)
            {
                var errors = await _presenter.TryVote(position);
                if (errors != null && errors.Count != 0)
                    ShowAlert(errors);

                _postsList?.GetAdapter()?.NotifyDataSetChanged();
            }
            else
            {
                var intent = new Intent(Context, typeof(PreSignInActivity));
                StartActivity(intent);
            }
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
        }

        public class ProfileSpanSizeLookup : GridLayoutManager.SpanSizeLookup
        {
            public override int GetSpanSize(int position)
            {
                if (position == 0)
                    return 3;
                else
                    return 1;
            }
        }
    }
}
