using System;
using System.Globalization;
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
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class ProfileFragment : BaseFragmentWithPresenter<UserProfilePresenter>
    {
        private readonly string _profileId;
        private Typeface font;
        private Typeface semibold_font;
        private ScrollListener _scrollListner;
        private LinearLayoutManager _linearLayoutManager;
        private GridLayoutManager _gridLayoutManager;

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

        [InjectOnClick(Resource.Id.btn_settings)]
        public void OnSettingsClick(object sender, EventArgs e)
        {
            var intent = new Intent(Context, typeof(SettingsActivity));
            StartActivity(intent);
        }

        ProfileFeedAdapter _profileFeedAdapter;
        ProfileFeedAdapter ProfileFeedAdapter
        {
            get
            {
                if (_profileFeedAdapter == null)
                {
                    _profileFeedAdapter = new ProfileFeedAdapter(Context, _presenter.Posts, new Typeface[] { font, semibold_font });
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
                    _profileGridAdapter = new ProfileGridAdapter(Context, _presenter.Posts, new Typeface[] { font, semibold_font });
                    _profileGridAdapter.Click += OnPhotoClick;
                    _profileGridAdapter.FollowersAction += OnFollowersClick;
                    _profileGridAdapter.FollowingAction += OnFollowingClick;
                    _profileGridAdapter.FollowAction += async () => await OnFollowClick();
                }
                return _profileGridAdapter;
            }
        }

        protected override void CreatePresenter()
        {
            _presenter = new UserProfilePresenter(_profileId);
        }

        public ProfileFragment(string profileId)
        {
            _profileId = profileId;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                V = inflater.Inflate(Resource.Layout.lyt_fragment_profile, null);
                Cheeseknife.Inject(this, V);
            }
            return V;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;
            base.OnViewCreated(view, savedInstanceState);
            font = Typeface.CreateFromAsset(Android.App.Application.Context.Assets, "OpenSans-Regular.ttf");
            semibold_font = Typeface.CreateFromAsset(Android.App.Application.Context.Assets, "OpenSans-Semibold.ttf");
            _login.Typeface = semibold_font;

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

            _postsList.SetLayoutManager(_gridLayoutManager);
            _postsList.AddItemDecoration(new ProfileGridItemdecoration(1));
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

        private async Task GetUserPosts(bool isRefresh = false)
        {
            var errors = await _presenter.GetUserPosts(isRefresh);
            if (errors != null && errors.Count != 0)
                ShowAlert(errors);

            if (_listSpinner != null)
                _listSpinner.Visibility = ViewStates.Gone;
            _postsList?.GetAdapter()?.NotifyDataSetChanged();
        }

        private async Task UpdatePage()
        {
            _scrollListner.ClearPosition();
            LoadProfile();
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
                _postsList.SetAdapter(ProfileFeedAdapter);
            }
            else
            {
                _switcher.SetImageResource(Resource.Drawable.grid_active);
                _postsList.SetLayoutManager(_gridLayoutManager);
                _postsList.AddItemDecoration(new GridItemdecoration(2, 3));
                _postsList.SetAdapter(ProfileGridAdapter);
            }
        }

        private async Task LoadProfile()
        {
            var response = await _presenter.GetUserInfo(_profileId);
            if (response.Success)
            {
                if (_listLayout != null)
                    _listLayout.Visibility = ViewStates.Visible;

                ProfileGridAdapter.ProfileData = response.Result;
                ProfileFeedAdapter.ProfileData = response.Result;
                _postsList?.GetAdapter()?.NotifyDataSetChanged();
            }
            else
            {
                if (response.Errors != null && response.Errors.Count != 0)
                    ShowAlert(response.Errors);
            }
            if (_listLayout != null)
                _loadingSpinner.Visibility = ViewStates.Gone;
        }

        private async Task OnFollowClick()
        {
            var resp = await _presenter.Follow(ProfileGridAdapter.ProfileData.HasFollowed);

            if (resp.Result.IsSuccess)
            {
                var hasFollowed = ProfileGridAdapter.ProfileData.HasFollowed == 1 ? 0 : 1;
                ProfileGridAdapter.ProfileData.HasFollowed = ProfileFeedAdapter.ProfileData.HasFollowed = hasFollowed;
                _postsList?.GetAdapter()?.NotifyDataSetChanged();
            }
            else if (resp.Errors != null && resp.Errors.Count != 0)
                ShowAlert(resp.Errors);
        }

        public void OnPhotoClick(int position)
        {
            var photo = _presenter.Posts[position].Photos?.FirstOrDefault();
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
            var intent = new Intent(Context, typeof(CommentsActivity));
            intent.PutExtra("uid", _presenter.Posts[position].Url);
            Context.StartActivity(intent);
        }

        private void VotersAction(int position)
        {
            Activity.Intent.PutExtra("url", _presenter.Posts[position].Url);
            ((BaseActivity)Activity).OpenNewContentFragment(new VotersFragment());
        }

        private void UserAction(int position)
        {
            if (_profileId != _presenter.Posts[position].Author)
                ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(_presenter.Posts[position].Author));
        }

        private async void LikeAction(int position)
        {
            if (BasePresenter.User.IsAuthenticated)
            {
                var errors = await _presenter.Vote(position);
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
