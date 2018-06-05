using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Steepshot.Activity;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Utils;
using Steepshot.Core.Models;
using Steepshot.Core.Authority;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Enums;
using Steepshot.Interfaces;
using Steepshot.Core.Errors;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;

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
        private Dialog _moreActionsDialog;
        private bool isSubscribed;
        private bool isSubscription;

#pragma warning disable 0649, 4014
        [BindView(Resource.Id.btn_back)] private ImageButton _backButton;
        [BindView(Resource.Id.btn_switcher)] private ImageButton _switcher;
        [BindView(Resource.Id.more)] private ImageButton _more;
        [BindView(Resource.Id.posts_list)] private RecyclerView _postsList;
        [BindView(Resource.Id.loading_spinner)] private ProgressBar _loadingSpinner;
        [BindView(Resource.Id.list_spinner)] private ProgressBar _listSpinner;
        [BindView(Resource.Id.refresher)] private SwipeRefreshLayout _refresher;
        [BindView(Resource.Id.btn_settings)] private ImageButton _settings;
        [BindView(Resource.Id.profile_login)] private TextView _login;
        [BindView(Resource.Id.list_layout)] private RelativeLayout _listLayout;
        [BindView(Resource.Id.first_post)] private Button _firstPostButton;
        [BindView(Resource.Id.post_prev_pager)] private ViewPager _postPager;
        [BindView(Resource.Id.like_power)] private TextView _likePowerLabel;
#pragma warning restore 0649

        private PostPagerAdapter<UserProfilePresenter> _profilePagerAdapter;
        private PostPagerAdapter<UserProfilePresenter> ProfilePagerAdapter
        {
            get
            {
                if (_profilePagerAdapter == null)
                {
                    _profilePagerAdapter = new PostPagerAdapter<UserProfilePresenter>(_postPager, Context, Presenter);
                    _profilePagerAdapter.PostAction += PostAction;
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
                    _profileFeedAdapter.PostAction += PostAction;
                    _profileFeedAdapter.ProfileAction += ProfileAction;
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
                    _profileGridAdapter.ProfileAction += ProfileAction;
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
                    if (Presenter != null)
                    {
                        LoadProfile();
                        if (!_isActivated)
                        {
                            GetUserPosts();
                            BasePresenter.ProfileUpdateType = ProfileUpdateType.None;
                        }
                        _isActivated = true;
                    }
                    else
                        BasePresenter.ProfileUpdateType = ProfileUpdateType.Full;
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
            _adapter.NotifyDataSetChanged();

            if (UserVisibleHint)
                UpdateProfile();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_fragment_profile, null);
                Cheeseknife.Bind(this, InflatedView);
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
                _likePowerLabel.Typeface = Style.Semibold;

                _scrollListner = new ScrollListener();
                _scrollListner.ScrolledToBottom += GetUserPosts;

                _linearLayoutManager = new LinearLayoutManager(Context);
                _gridLayoutManager = new GridLayoutManager(Context, 3);
                _profileSpanSizeLookup = new ProfileSpanSizeLookup();
                _gridLayoutManager.SetSpanSizeLookup(_profileSpanSizeLookup);

                _gridItemDecoration = new GridItemDecoration(true);

                _tabSettings = AppSettings.User.Login.Equals(_profileId)
                    ? AppSettings.User.GetTabSettings($"User_{nameof(ProfileFragment)}")
                    : AppSettings.User.GetTabSettings(nameof(ProfileFragment));

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
                _more.Click += MoreOnClick;

                _moreActionsDialog = new BottomSheetDialog(Context);
                _moreActionsDialog.Window.RequestFeature(WindowFeatures.NoTitle);

                _firstPostButton.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.CreateFirstPostText);

                if (_profileId != AppSettings.User.Login)
                {
                    _settings.Visibility = ViewStates.Gone;
                    _backButton.Visibility = ViewStates.Visible;
                    _more.Visibility = ViewStates.Visible;
                    _more.Enabled = false;
                    _login.Text = _profileId;
                    LoadProfile();
                    GetUserPosts();
                }
                else
                {
                    _more.Visibility = ViewStates.Gone;
                    _login.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.MyProfile);
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
            _postPager.SetCurrentItem(Presenter.IndexOf(post), false);
            _profilePagerAdapter.NotifyDataSetChanged();
            _postPager.Visibility = ViewStates.Visible;
            _postsList.Visibility = ViewStates.Gone;
        }

        public bool ClosePost()
        {
            if (_postPager.Visibility == ViewStates.Visible)
            {
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
                        _profileId == AppSettings.User.Login && Presenter.UserProfileResponse.PostCount == 0 && Presenter.UserProfileResponse.HiddenPostCount == 0
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

        private void MoreOnClick(object sender, EventArgs eventArgs)
        {
            if (!isSubscription)
            {
                var inflater = (LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService);
                using (var dialogView = inflater.Inflate(Resource.Layout.lyt_profile_popup, null))
                {
                    dialogView.SetMinimumWidth((int)(Resources.DisplayMetrics.WidthPixels * 0.8));
                    var pushes = dialogView.FindViewById<Button>(Resource.Id.pushes);
                    if (isSubscribed)
                    {
                        pushes.SetTextColor(Style.R255G34B5);
                        pushes.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.UnwatchUser);
                    }
                    else
                    {
                        pushes.SetTextColor(Color.Black);
                        pushes.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.WatchUser);
                    }

                    pushes.Typeface = Style.Semibold;

                    var cancel = dialogView.FindViewById<Button>(Resource.Id.cancel);
                    cancel.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Cancel);
                    cancel.Typeface = Style.Semibold;

                    pushes.Click -= PushesOnClick;
                    pushes.Click += PushesOnClick;

                    cancel.Click -= CancelDialog;
                    cancel.Click += CancelDialog;

                    _moreActionsDialog.SetContentView(dialogView);
                    dialogView.SetBackgroundColor(Color.Transparent);
                    _moreActionsDialog.Window.FindViewById(Resource.Id.design_bottom_sheet).SetBackgroundColor(Color.Transparent);
                    _moreActionsDialog.Show();
                }
            }
        }

        private async void PushesOnClick(object sender, EventArgs eventArgs)
        {
            _moreActionsDialog.Dismiss();
            var model = new PushNotificationsModel(AppSettings.User.UserInfo, !isSubscribed);
            model.WatchedUser = _profileId;

            isSubscription = true;

            var result = await BasePresenter.TrySubscribeForPushes(model);
            if (result.IsSuccess)
            {
                isSubscribed = !isSubscribed;
            }

            isSubscription = false;
        }

        private void CancelDialog(object sender, EventArgs e)
        {
            _moreActionsDialog.Dismiss();
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
            AppSettings.User.Save();
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

                if (error == null || error is CanceledError)
                {
                    _listLayout.Visibility = ViewStates.Visible;
                    _more.Enabled = true;
                    isSubscribed = Presenter.UserProfileResponse.IsSubscribed;
                    break;
                }

                Context.ShowAlert(error);
                await Task.Delay(5000);
                if (!IsInitialized)
                    return;

            } while (true);

            _firstPostButton.Visibility =
                    _profileId == AppSettings.User.Login && Presenter.UserProfileResponse.PostCount == 0 && Presenter.UserProfileResponse.HiddenPostCount == 0
                    ? ViewStates.Visible
                    : ViewStates.Gone;
            _loadingSpinner.Visibility = ViewStates.Gone;
        }

        private async void ProfileAction(ActionType type)
        {
            switch (type)
            {
                case ActionType.Balance:
                    break;
                case ActionType.Followers:
                    Activity.Intent.PutExtra(FollowersFragment.IsFollowersExtra, true);
                    Activity.Intent.PutExtra(FollowersFragment.UsernameExtra, _profileId);
                    Activity.Intent.PutExtra(FollowersFragment.CountExtra, Presenter.UserProfileResponse.FollowersCount);
                    ((BaseActivity)Activity).OpenNewContentFragment(new FollowersFragment());
                    break;
                case ActionType.Following:
                    Activity.Intent.PutExtra(FollowersFragment.IsFollowersExtra, false);
                    Activity.Intent.PutExtra(FollowersFragment.UsernameExtra, _profileId);
                    Activity.Intent.PutExtra(FollowersFragment.CountExtra, Presenter.UserProfileResponse.FollowingCount);
                    ((BaseActivity)Activity).OpenNewContentFragment(new FollowersFragment());
                    break;
                case ActionType.Follow:
                    if (AppSettings.User.IsAuthenticated)
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
                    break;
                case ActionType.LikePower:
                    var avatar = _postsList.FindViewById(Resource.Id.profile_image);
                    avatar.Enabled = false;
                    _likePowerLabel.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PowerOfLike, Presenter.UserProfileResponse.VotingPower);
                    _likePowerLabel.Visibility = ViewStates.Visible;
                    await Task.Delay(1000);
                    _likePowerLabel.Visibility = ViewStates.Gone;
                    avatar.Enabled = true;
                    break;
            }
        }

        private async void PostAction(ActionType type, Post post)
        {
            switch (type)
            {
                case ActionType.Like:
                    {
                        if (AppSettings.User.IsAuthenticated)
                        {
                            var error = await Presenter.TryVote(post);
                            if (!IsInitialized)
                                return;

                            Context.ShowAlert(error);
                        }
                        else
                            OpenLogin();
                        break;
                    }
                case ActionType.VotersLikes:
                case ActionType.VotersFlags:
                    {
                        if (post == null)
                            return;

                        var isLikers = type == ActionType.VotersLikes;
                        Activity.Intent.PutExtra(FeedFragment.PostUrlExtraPath, post.Url);
                        Activity.Intent.PutExtra(FeedFragment.PostNetVotesExtraPath, isLikers ? post.NetLikes : post.NetFlags);
                        Activity.Intent.PutExtra(VotersFragment.VotersType, isLikers);
                        ((BaseActivity)Activity).OpenNewContentFragment(new VotersFragment());
                        break;
                    }
                case ActionType.Comments:
                    {
                        if (post == null)
                            return;
                        if (post.Children == 0 && !AppSettings.User.IsAuthenticated)
                        {
                            OpenLogin();
                            return;
                        }

                        ((BaseActivity)Activity).OpenNewContentFragment(new CommentsFragment(post, post.Children == 0));
                        break;
                    }
                case ActionType.Profile:
                    {
                        if (post == null)
                            return;

                        if (_profileId != post.Author)
                            ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(post.Author));
                        break;
                    }
                case ActionType.Flag:
                    {
                        if (!AppSettings.User.IsAuthenticated)
                            return;

                        var error = await Presenter.TryFlag(post);
                        if (!IsInitialized)
                            return;

                        Context.ShowAlert(error);
                        break;
                    }
                case ActionType.Hide:
                    {
                        Presenter.HidePost(post);
                        break;
                    }
                case ActionType.Edit:
                    {
                        ((BaseActivity)Activity).OpenNewContentFragment(new PostEditFragment(post));
                        ((RootActivity)Activity)._tabLayout.Visibility = ViewStates.Gone;
                        break;
                    }
                case ActionType.Delete:
                    {
                        var error = await Presenter.TryDeletePost(post);
                        if (!IsInitialized)
                            return;
                        Context.ShowAlert(error);
                        break;
                    }
                case ActionType.Share:
                    {
                        var shareIntent = new Intent(Intent.ActionSend);
                        shareIntent.SetType("text/plain");
                        shareIntent.PutExtra(Intent.ExtraSubject, post.Title);
                        shareIntent.PutExtra(Intent.ExtraText, AppSettings.LocalizationManager.GetText(LocalizationKeys.PostLink, post.Url));
                        StartActivity(Intent.CreateChooser(shareIntent, AppSettings.LocalizationManager.GetText(LocalizationKeys.Sharepost)));
                        break;
                    }
                case ActionType.Photo:
                    {
                        OpenPost(post);
                        break;
                    }
            }
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
