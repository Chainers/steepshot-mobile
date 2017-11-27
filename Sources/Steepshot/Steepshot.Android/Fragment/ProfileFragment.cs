using System;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Activity;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Utils;
using Steepshot.Core.Models;
using Steepshot.Core.Utils;

namespace Steepshot.Fragment
{
    public sealed class ProfileFragment : BaseFragmentWithPresenter<UserProfilePresenter>
    {
        private bool _isActivated;
        private string _profileId;
        private ScrollListener _scrollListner;
        private LinearLayoutManager _linearLayoutManager;
        private GridLayoutManager _gridLayoutManager;
        private GridItemDecoration _gridItemDecoration;
        private ProfileSpanSizeLookup _profileSpanSizeLookup;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.btn_back)] private ImageButton _backButton;
        [InjectView(Resource.Id.btn_switcher)] private ImageButton _switcher;
        [InjectView(Resource.Id.posts_list)] private RecyclerView _postsList;
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar _loadingSpinner;
        [InjectView(Resource.Id.list_spinner)] private ProgressBar _listSpinner;
        [InjectView(Resource.Id.refresher)] private SwipeRefreshLayout _refresher;
        [InjectView(Resource.Id.btn_settings)] private ImageButton _settings;
        [InjectView(Resource.Id.profile_login)] private TextView _login;
        [InjectView(Resource.Id.list_layout)] private RelativeLayout _listLayout;
        [InjectView(Resource.Id.first_post)] private Button _firstPostButton;
#pragma warning restore 0649

        private ProfileFeedAdapter _profileFeedAdapter;
        private ProfileFeedAdapter ProfileFeedAdapter
        {
            get
            {
                if (_profileFeedAdapter == null)
                {
                    _profileFeedAdapter = new ProfileFeedAdapter(Context, Presenter);
                    _profileFeedAdapter.PhotoClick += OnPhotoClick;
                    _profileFeedAdapter.LikeAction += LikeAction;
                    _profileFeedAdapter.UserAction += UserAction;
                    _profileFeedAdapter.CommentAction += CommentAction;
                    _profileFeedAdapter.VotersClick += VotersAction;
                    _profileFeedAdapter.FollowersAction += OnFollowersClick;
                    _profileFeedAdapter.FollowingAction += OnFollowingClick;
                    _profileFeedAdapter.FollowAction += OnFollowClick;
                    _profileFeedAdapter.FlagAction += FlagAction;
                    _profileFeedAdapter.HideAction += HideAction;
                    _profileFeedAdapter.TagAction += TagAction;
                }
                return _profileFeedAdapter;
            }
        }

        private ProfileGridAdapter _profileGridAdapter;
        private ProfileGridAdapter ProfileGridAdapter
        {
            get
            {
                if (_profileGridAdapter == null)
                {
                    _profileGridAdapter = new ProfileGridAdapter(Context, Presenter);
                    _profileGridAdapter.Click += OnPhotoClick;
                    _profileGridAdapter.FollowersAction += OnFollowersClick;
                    _profileGridAdapter.FollowingAction += OnFollowingClick;
                    _profileGridAdapter.FollowAction += OnFollowClick;
                }
                return _profileGridAdapter;
            }
        }

        public override bool CustomUserVisibleHint
        {
            get => base.CustomUserVisibleHint;
            set
            {
                if (value)
                {
                    if (!_isActivated)
                    {
                        if (Presenter != null)
                        {
                            LoadProfile();
                            GetUserPosts();
                            BasePresenter.ShouldUpdateProfile = false;
                        }
                        else
                            BasePresenter.ShouldUpdateProfile = true;
                        _isActivated = true;
                    }
                }
                UserVisibleHint = value;
            }
        }

        public ProfileFragment()
        {
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (savedInstanceState != null)
                _profileId = savedInstanceState.GetString("profileId");
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString("profileId", _profileId);
            base.OnSaveInstanceState(outState);
        }

        public ProfileFragment(string profileId)
        {
            _profileId = profileId;
        }

        public override void OnResume()
        {
            base.OnResume();
            UpdateProfile();
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

            Presenter.UserName = _profileId;
            Presenter.SourceChanged += PresenterSourceChanged;

            _login.Typeface = Style.Semibold;
            _firstPostButton.Typeface = Style.Semibold;

            _scrollListner = new ScrollListener();
            _scrollListner.ScrolledToBottom += GetUserPosts;

            _linearLayoutManager = new LinearLayoutManager(Context);
            _gridLayoutManager = new GridLayoutManager(Context, 3);
            _profileSpanSizeLookup = new ProfileSpanSizeLookup();
            _gridLayoutManager.SetSpanSizeLookup(_profileSpanSizeLookup);

            _gridItemDecoration = new GridItemDecoration(true);
            _postsList.SetLayoutManager(_gridLayoutManager);
            _postsList.AddItemDecoration(_gridItemDecoration);
            _postsList.AddOnScrollListener(_scrollListner);
            _postsList.SetAdapter(ProfileGridAdapter);

            _refresher.Refresh += RefresherRefresh;
            _settings.Click += OnSettingsClick;
            _login.Click += OnLoginClick;
            _backButton.Click += GoBackClick;
            _switcher.Click += OnSwitcherClick;
            _firstPostButton.Click += OnFirstPostButtonClick;

            _firstPostButton.Text = Localization.Texts.CreateFirstPostText;

            if (_profileId != BasePresenter.User.Login)
            {
                _settings.Visibility = ViewStates.Gone;
                _backButton.Visibility = ViewStates.Visible;
                _login.Text = _profileId;
                LoadProfile();
                GetUserPosts();
            }
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (resultCode == -1 && requestCode == CommentsActivity.RequestCode)
            {
                var postUrl = data.GetStringExtra(CommentsActivity.ResultString);
                var count = data.GetIntExtra(CommentsActivity.CountString, 0);
                var post = Presenter.FirstOrDefault(p => p.Url == postUrl);
                post.Children += count;
                _postsList.GetAdapter()?.NotifyDataSetChanged();
            }
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
        }

        private void OnFirstPostButtonClick(object sender, EventArgs e)
        {
            var intent = new Intent(Activity, typeof(CameraActivity));
            Activity.StartActivity(intent);
        }

        private void PresenterSourceChanged(Status status)
        {
            if (!IsInitialized)
                return;

            Activity.RunOnUiThread(() =>
            {
                _profileSpanSizeLookup.LastItemNumber = Presenter.Count;
                _postsList.GetAdapter()?.NotifyDataSetChanged();
            });
        }

        private async void RefresherRefresh(object sender, EventArgs e)
        {
            await UpdatePage();
            if (!IsInitialized)
                return;
            _refresher.Refreshing = false;
        }

        private async void GetUserPosts()
        {
            await GetUserPosts(false);
        }

        private async Task GetUserPosts(bool isRefresh)
        {
            if (isRefresh)
                Presenter.Clear();

            var errors = await Presenter.TryLoadNextPosts();
            if (!IsInitialized)
                return;

            Context.ShowAlert(errors);
            _listSpinner.Visibility = ViewStates.Gone;
        }

        private void OnSettingsClick(object sender, EventArgs e)
        {
            var intent = new Intent(Context, typeof(SettingsActivity));
            StartActivity(intent);
        }

        private void OnLoginClick(object sender, EventArgs e)
        {
            _postsList.ScrollToPosition(0);
        }

        private async Task UpdatePage()
        {
            _scrollListner.ClearPosition();
            await LoadProfile();
            await GetUserPosts(true);
        }

        private void GoBackClick(object sender, EventArgs e)
        {
            Activity.OnBackPressed();
        }

        private void OnSwitcherClick(object sender, EventArgs e)
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
            do
            {
                var errors = await Presenter.TryGetUserInfo(_profileId);
                if (!IsInitialized)
                    return;

                if (errors != null && !errors.Any())
                {
                    _listLayout.Visibility = ViewStates.Visible;
                    break;
                }

                Context.ShowAlert(errors);
                await Task.Delay(5000);
                if (!IsInitialized)
                    return;

            } while (true);

            _firstPostButton.Visibility =
                    _profileId == BasePresenter.User.Login && Presenter.UserProfileResponse.PostCount == 0 && Presenter.UserProfileResponse.HiddenPostCount == 0
                    ? ViewStates.Visible
                    : ViewStates.Gone;
            _loadingSpinner.Visibility = ViewStates.Gone;
        }

        private async void OnFollowClick()
        {
            var errors = await Presenter.TryFollow();
            if (!IsInitialized)
                return;

            Context.ShowAlert(errors, ToastLength.Long);
        }

        private void OnPhotoClick(Post post)
        {
            if (post == null)
                return;

            var photo = post.Photos?.FirstOrDefault();
            if (photo == null)
                return;

            var intent = new Intent(Context, typeof(PostPreviewActivity));
            intent.PutExtra(PostPreviewActivity.PhotoExtraPath, photo);
            StartActivity(intent);
        }

        private void OnFollowingClick()
        {
            Activity.Intent.PutExtra(FollowersFragment.IsFollowersExtra, false);
            Activity.Intent.PutExtra(FollowersFragment.UsernameExtra, _profileId);
            Activity.Intent.PutExtra(FollowersFragment.CountExtra, Presenter.UserProfileResponse.FollowingCount);
            ((BaseActivity)Activity).OpenNewContentFragment(new FollowersFragment());
        }

        private void OnFollowersClick()
        {
            Activity.Intent.PutExtra(FollowersFragment.IsFollowersExtra, true);
            Activity.Intent.PutExtra(FollowersFragment.UsernameExtra, _profileId);
            Activity.Intent.PutExtra(FollowersFragment.CountExtra, Presenter.UserProfileResponse.FollowersCount);
            ((BaseActivity)Activity).OpenNewContentFragment(new FollowersFragment());
        }

        private void CommentAction(Post post)
        {
            if (post == null)
                return;
            if (post.Children == 0 && !BasePresenter.User.IsAuthenticated)
            {
                OpenLogin();
                return;
            }

            var intent = new Intent(Context, typeof(CommentsActivity));
            intent.PutExtra(CommentsActivity.PostExtraPath, post.Url);
            StartActivityForResult(intent, CommentsActivity.RequestCode);
        }

        private void VotersAction(Post post, bool isLikers)
        {
            if (post == null)
                return;

            Activity.Intent.PutExtra(FeedFragment.PostUrlExtraPath, post.Url);
            Activity.Intent.PutExtra(FeedFragment.PostNetVotesExtraPath, isLikers ? post.NetLikes : post.NetFlags);
            Activity.Intent.PutExtra(VotersFragment.VotersType, isLikers);
            ((BaseActivity)Activity).OpenNewContentFragment(new VotersFragment());
        }

        private void UserAction(Post post)
        {
            if (post == null)
                return;

            if (_profileId != post.Author)
                ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(post.Author));
        }

        private async void LikeAction(Post post)
        {
            if (BasePresenter.User.IsAuthenticated)
            {
                var errors = await Presenter.TryVote(post);
                if (!IsInitialized)
                    return;

                Context.ShowAlert(errors);
            }
            else
                OpenLogin();
        }

        private async void FlagAction(Post post)
        {
            if (!BasePresenter.User.IsAuthenticated)
                return;

            var errors = await Presenter.TryFlag(post);
            if (!IsInitialized)
                return;

            Context.ShowAlert(errors);
        }

        private void HideAction(Post post)
        {
            Presenter.RemovePost(post);
        }

        private void TagAction(string tag)
        {
            if (tag != null)
            {
                Activity.Intent.PutExtra(SearchFragment.SearchExtra, tag);
                ((RootActivity)Activity).SelectTab(1);
            }
            else
                _postsList.GetAdapter()?.NotifyDataSetChanged();
        }

        private void OpenLogin()
        {
            var intent = new Intent(Activity, typeof(WelcomeActivity));
            StartActivity(intent);
        }

        private void UpdateProfile()
        {
            if (BasePresenter.ShouldUpdateProfile)
            {
                _listSpinner.Visibility = ViewStates.Visible;
                UpdatePage();
                BasePresenter.ShouldUpdateProfile = false;
            }
        }
    }
}
