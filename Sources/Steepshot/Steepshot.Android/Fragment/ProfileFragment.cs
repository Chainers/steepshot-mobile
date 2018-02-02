using System;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Support.V4.View;
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
using Steepshot.Core.Authority;
using Steepshot.Core.Errors;
using Steepshot.Core.Models.Enums;
using Steepshot.Interfaces;
using Newtonsoft.Json;
using Steepshot.Utils.Animations;
using Steepshot.Utils.Animations.Interfaces;

namespace Steepshot.Fragment
{
    public sealed class ProfileFragment : BaseFragmentWithPresenter<UserProfilePresenter>, ICanOpenPost
    {
        private bool _isActivated;
        private string _profileId;
        private TabSettings _tabSettings;

        private ScrollListener _scrollListner;
        private LinearLayoutManager _linearLayoutManager;
        private GridLayoutManager _gridLayoutManager;
        private GridItemDecoration _gridItemDecoration;
        private ProfileSpanSizeLookup _profileSpanSizeLookup;
        private RecyclerView.Adapter _adapter;

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
        [InjectView(Resource.Id.post_prev_pager)] private ViewPager _postPager;
#pragma warning restore 0649

        private PostPagerAdapter<UserProfilePresenter> _profilePagerAdapter;
        private PostPagerAdapter<UserProfilePresenter> ProfilePagerAdapter
        {
            get
            {
                if (_profilePagerAdapter == null)
                {
                    _profilePagerAdapter = new PostPagerAdapter<UserProfilePresenter>(Context, Presenter);
                    _profilePagerAdapter.LikeAction += LikeAction;
                    _profilePagerAdapter.UserAction += UserAction;
                    _profilePagerAdapter.CommentAction += CommentAction;
                    _profilePagerAdapter.VotersClick += VotersAction;
                    _profilePagerAdapter.PhotoClick += OnPhotoClick;
                    _profilePagerAdapter.FlagAction += FlagAction;
                    _profilePagerAdapter.HideAction += HideAction;
                    _profilePagerAdapter.EditAction += EditAction;
                    _profilePagerAdapter.DeleteAction += DeleteAction;
                    _profilePagerAdapter.TagAction += TagAction;
                    _profilePagerAdapter.CloseAction += CloseAction;
                }
                return _profilePagerAdapter;
            }
        }

        private ProfileFeedAdapter _profileFeedAdapter;
        private ProfileFeedAdapter ProfileFeedAdapter
        {
            get
            {
                if (_profileFeedAdapter == null)
                {
                    _profileFeedAdapter = new ProfileFeedAdapter(Context, Presenter);
                    _profileFeedAdapter.PhotoClick += FeedPhotoClick;
                    _profileFeedAdapter.LikeAction += LikeAction;
                    _profileFeedAdapter.UserAction += UserAction;
                    _profileFeedAdapter.CommentAction += CommentAction;
                    _profileFeedAdapter.VotersClick += VotersAction;
                    _profileFeedAdapter.FollowersAction += OnFollowersClick;
                    _profileFeedAdapter.FollowingAction += OnFollowingClick;
                    _profileFeedAdapter.FollowAction += OnFollowClick;
                    _profileFeedAdapter.FlagAction += FlagAction;
                    _profileFeedAdapter.HideAction += HideAction;
                    _profileFeedAdapter.EditAction += EditAction;
                    _profileFeedAdapter.DeleteAction += DeleteAction;
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
                    _profileGridAdapter.Click += FeedPhotoClick;
                    _profileGridAdapter.FollowersAction += OnFollowersClick;
                    _profileGridAdapter.FollowingAction += OnFollowingClick;
                    _profileGridAdapter.FollowAction += OnFollowClick;
                }
                return _profileGridAdapter;
            }
        }

        public override bool UserVisibleHint
        {
            get => base.UserVisibleHint;
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
                            BasePresenter.ProfileUpdateType = ProfileUpdateType.None;
                        }
                        else
                            BasePresenter.ProfileUpdateType = ProfileUpdateType.Full;
                        _isActivated = true;
                    }
                    else
                        _postsList?.GetAdapter()?.NotifyDataSetChanged();
                }
                base.UserVisibleHint = value;
            }
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

        public ProfileFragment()
        {
            //This is fix for crashing when app killed in background
        }

        public override void OnResume()
        {
            base.OnResume();
            if (_postPager.Visibility == ViewStates.Visible)
                if (Activity is RootActivity activity)
                    activity._tabLayout.Visibility = ViewStates.Invisible;
            if (UserVisibleHint)
                UpdateProfile();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_fragment_profile, null);
                Cheeseknife.Inject(this, InflatedView);
            }
            ToggleTabBar();
            return InflatedView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
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

                _tabSettings = BasePresenter.User.Login.Equals(_profileId)
                    ? BasePresenter.User.GetTabSettings($"User_{nameof(ProfileFragment)}")
                    : BasePresenter.User.GetTabSettings(nameof(ProfileFragment));

                SwitchListAdapter(_tabSettings.IsGridView);

                _postsList.AddOnScrollListener(_scrollListner);

                _postPager.SetClipToPadding(false);
                var pagePadding = (int)BitmapUtils.DpToPixel(20, Resources);
                _postPager.SetPadding(pagePadding, 0, pagePadding, 0);
                _postPager.PageMargin = pagePadding / 2;
                _postPager.PageScrollStateChanged += PostPagerOnPageScrollStateChanged;
                _postPager.PageScrolled += PostPagerOnPageScrolled;
                _postPager.Adapter = ProfilePagerAdapter;
                _postPager.SetPageTransformer(false, _profilePagerAdapter, (int)LayerType.None);

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

            var postUrl = Activity?.Intent?.GetStringExtra(CommentsFragment.ResultString);
            if (!string.IsNullOrWhiteSpace(postUrl))
            {
                var count = Activity.Intent.GetIntExtra(CommentsFragment.CountString, 0);
                Activity.Intent.RemoveExtra(CommentsFragment.ResultString);
                Activity.Intent.RemoveExtra(CommentsFragment.CountString);
                var post = Presenter.FirstOrDefault(p => p.Url == postUrl);
                if (post != null)
                {
                    post.Children += count;
                    _adapter.NotifyDataSetChanged();
                }
            }
        }

        private void OnPostPagerGlobalLayout(object sender, EventArgs e)
        {
            if (_postPager.Visibility == ViewStates.Visible)
            {
                _postPager.ViewTreeObserver.GlobalLayout -= OnPostPagerGlobalLayout;
                var storyboard = new Storyboard();
                storyboard.AddRange(new IAnimator[]{
                    _postsList.Opacity(1, 0, 500, Easing.CubicOut),
                    _postPager.Opacity(0, 1, 500, Easing.CubicOut),
                    _profilePagerAdapter.Storyboard
                });
                storyboard.Animate(() => { _postsList.Visibility = ViewStates.Gone; _postsList.Alpha = 1; });
            }
        }

        private void PostPagerOnPageScrolled(object sender, ViewPager.PageScrolledEventArgs pageScrolledEventArgs)
        {
            if (pageScrolledEventArgs.Position == Presenter.Count)
            {
                if (!Presenter.IsLastReaded)
                    GetUserPosts();
                else
                    _profilePagerAdapter.NotifyDataSetChanged();
            }
        }

        private void PostPagerOnPageScrollStateChanged(object sender, ViewPager.PageScrollStateChangedEventArgs pageScrollStateChangedEventArgs)
        {
            if (pageScrollStateChangedEventArgs.State == 0)
            {
                _profilePagerAdapter.CurrentItem = _postPager.CurrentItem;
                _postsList.ScrollToPosition(_postPager.CurrentItem + 1);
                if (_postsList.GetLayoutManager() is GridLayoutManager manager)
                {
                    var positionToScroll = _postPager.CurrentItem + (_postPager.CurrentItem - manager.FindFirstVisibleItemPosition()) / 2;
                    _postsList.ScrollToPosition(positionToScroll < Presenter.Count
                        ? positionToScroll
                        : Presenter.Count);
                }
            }
        }

        private void FeedPhotoClick(Post post)
        {
            if (post == null)
                return;

            OpenPost(post);
        }

        public void OpenPost(Post post)
        {
            if (Activity is RootActivity activity)
                activity._tabLayout.Visibility = ViewStates.Gone;
            _postPager.SetCurrentItem(Presenter.IndexOf(post), false);
            _profilePagerAdapter.CurrentItem = _postPager.CurrentItem;
            _profilePagerAdapter.NotifyDataSetChanged();
            _postPager.Alpha = 0;
            _postPager.Visibility = ViewStates.Visible;
            _postPager.ViewTreeObserver.GlobalLayout += OnPostPagerGlobalLayout;
        }

        public bool ClosePost()
        {
            if (_postPager.Visibility == ViewStates.Visible)
            {
                if (Activity is RootActivity activity)
                    activity._tabLayout.Visibility = ViewStates.Visible;
                _postPager.Visibility = ViewStates.Gone;
                _postsList.Visibility = ViewStates.Visible;
                _postsList.GetAdapter().NotifyDataSetChanged();
                if (_postsList.GetAdapter() == ProfileGridAdapter)
                {
                    var seenItem = _postsList.FindViewHolderForAdapterPosition(_postPager.CurrentItem + 1)?.ItemView
                        .FindViewById(Resource.Id.grid_item_photo) as ImageView;
                    if (seenItem != null)
                        AnimationHelper.PulseGridItem(seenItem);
                }
                return true;
            }
            return false;
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
                if (status.Sender == nameof(UserProfilePresenter.TryFollow) || status.Sender == nameof(UserProfilePresenter.TryGetUserInfo))
                {
                    _firstPostButton.Visibility =
                        _profileId == BasePresenter.User.Login && Presenter.UserProfileResponse.PostCount == 0 && Presenter.UserProfileResponse.HiddenPostCount == 0
                            ? ViewStates.Visible
                            : ViewStates.Gone;
                }

                _profileSpanSizeLookup.LastItemNumber = Presenter.Count;
                _adapter.NotifyDataSetChanged();
                ProfilePagerAdapter.NotifyDataSetChanged();
            });
        }

        private async void RefresherRefresh(object sender, EventArgs e)
        {
            await UpdatePage(ProfileUpdateType.Full);
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

            var error = await Presenter.TryLoadNextPosts();
            if (!IsInitialized)
                return;

            Context.ShowAlert(error);
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

        private async Task UpdatePage(ProfileUpdateType updateType)
        {
            _scrollListner.ClearPosition();
            if (updateType == ProfileUpdateType.Full)
            {
                _listSpinner.Visibility = ViewStates.Visible;
                GetUserPosts(true).ContinueWith(_ => Activity.RunOnUiThread(() =>
                     _listSpinner.Visibility = ViewStates.Gone));
            }
            await LoadProfile();
        }

        private void GoBackClick(object sender, EventArgs e)
        {
            Activity.OnBackPressed();
        }

        private void OnSwitcherClick(object sender, EventArgs e)
        {
            _tabSettings.IsGridView = !(_postsList.GetLayoutManager() is GridLayoutManager);
            BasePresenter.User.Save();
            SwitchListAdapter(_tabSettings.IsGridView);
        }

        private void SwitchListAdapter(bool isGridView)
        {
            lock (_switcher)
            {
                if (isGridView)
                {
                    _switcher.SetImageResource(Resource.Drawable.ic_grid_active);
                    _postsList.SetLayoutManager(_gridLayoutManager);
                    _postsList.AddItemDecoration(_gridItemDecoration);
                    _adapter = ProfileGridAdapter;
                }
                else
                {
                    _switcher.SetImageResource(Resource.Drawable.ic_grid);
                    _postsList.SetLayoutManager(_linearLayoutManager);
                    _postsList.RemoveItemDecoration(_gridItemDecoration);
                    _adapter = ProfileFeedAdapter;
                }
                _adapter.NotifyDataSetChanged();
                _postsList.SetAdapter(_adapter);
                _postsList.ScrollToPosition(_scrollListner.Position);
            }
        }

        private async Task LoadProfile()
        {
            do
            {
                var error = await Presenter.TryGetUserInfo(_profileId);
                if (!IsInitialized)
                    return;

                if (error == null || error is TaskCanceledError)
                {
                    _listLayout.Visibility = ViewStates.Visible;
                    break;
                }

                Context.ShowAlert(error);
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
            if (BasePresenter.User.IsAuthenticated)
            {
                var error = await Presenter.TryFollow();
                if (!IsInitialized)
                    return;

                Context.ShowAlert(error, ToastLength.Long);
            }
            else
            {
                var intent = new Intent(Activity, typeof(WelcomeActivity));
                StartActivity(intent);
            }
        }

        private void OnPhotoClick(Post post)
        {
            if (post == null)
                return;

            var intent = new Intent(Context, typeof(PostPreviewActivity));
            intent.PutExtra(PostPreviewActivity.PhotoExtraPath, post.Media[0].Url);
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

            ((BaseActivity)Activity).OpenNewContentFragment(new CommentsFragment(post, post.Children == 0));
        }

        private void VotersAction(Post post, VotersType type)
        {
            if (post == null)
                return;
            var isLikers = type == VotersType.Likes;
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
                var error = await Presenter.TryVote(post);
                if (!IsInitialized)
                    return;

                Context.ShowAlert(error);
            }
            else
                OpenLogin();
        }

        private async void FlagAction(Post post)
        {
            if (!BasePresenter.User.IsAuthenticated)
                return;

            var error = await Presenter.TryFlag(post);
            if (!IsInitialized)
                return;

            Context.ShowAlert(error);
        }

        private void HideAction(Post post)
        {
            Presenter.HidePost(post);
        }

        private void EditAction(Post post)
        {
            var intent = new Intent(Activity, typeof(PostDescriptionActivity));
            intent.PutExtra(PostDescriptionActivity.EditPost, JsonConvert.SerializeObject(post));
            Activity.StartActivity(intent);
        }

        private async void DeleteAction(Post post)
        {
            var error = await Presenter.TryDeletePost(post);
            if (!IsInitialized)
                return;

            Context.ShowAlert(error);
        }

        private void TagAction(string tag)
        {
            if (tag != null)
            {
                Activity.Intent.PutExtra(SearchFragment.SearchExtra, tag);
                ((BaseActivity)Activity).OpenNewContentFragment(new PreSearchFragment());
            }
            else
                _postsList.GetAdapter()?.NotifyDataSetChanged();
        }
        private void CloseAction()
        {
            ClosePost();
        }

        private void OpenLogin()
        {
            var intent = new Intent(Activity, typeof(WelcomeActivity));
            StartActivity(intent);
        }

        private void UpdateProfile()
        {
            if (BasePresenter.ProfileUpdateType != ProfileUpdateType.None)
            {
                UpdatePage(BasePresenter.ProfileUpdateType);
                BasePresenter.ProfileUpdateType = ProfileUpdateType.None;
            }
        }
    }
}
