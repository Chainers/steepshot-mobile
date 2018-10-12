using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
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
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Fragment
{
    public sealed class ProfileFragment : BasePostsFragment<UserProfilePresenter>
    {
        private TabOptions _tabOptions;

        private ScrollListener _scrollListner;
        private LinearLayoutManager _linearLayoutManager;
        private GridLayoutManager _gridLayoutManager;
        private GridItemDecoration _gridItemDecoration;
        private ProfileSpanSizeLookup _profileSpanSizeLookup;
        private RecyclerView.Adapter _rvAdapter;
        private Dialog _moreActionsDialog;
        private readonly bool _loadOnViewCreated;
        private bool isSubscribed;
        private bool isSubscription;

#pragma warning disable 0649, 4014
        [BindView(Resource.Id.btn_back)] private ImageButton _backButton;
        [BindView(Resource.Id.btn_switcher)] private ImageButton _switcher;
        [BindView(Resource.Id.more)] private ImageButton _more;
        [BindView(Resource.Id.loading_spinner)] private ProgressBar _loadingSpinner;
        [BindView(Resource.Id.list_spinner)] private ProgressBar _listSpinner;
        [BindView(Resource.Id.btn_settings)] private ImageButton _settings;
        [BindView(Resource.Id.profile_login)] private TextView _login;
        [BindView(Resource.Id.list_layout)] private RelativeLayout _listLayout;
        [BindView(Resource.Id.first_post)] private Button _firstPostButton;
        [BindView(Resource.Id.like_power)] private TextView _likePowerLabel;
#pragma warning restore 0649

        private PostPagerAdapter<UserProfilePresenter> _profilePagerAdapter;
        private PostPagerAdapter<UserProfilePresenter> ProfilePagerAdapter
        {
            get
            {
                if (_profilePagerAdapter == null)
                {
                    _profilePagerAdapter = new PostPagerAdapter<UserProfilePresenter>(PostPager, Context, Presenter);
                    _profilePagerAdapter.PostAction += PostAction;
                    _profilePagerAdapter.AutoLinkAction += AutoLinkAction;
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
                    _profileFeedAdapter.AutoLinkAction += AutoLinkAction;
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
                    if (IsInitialized)
                    {
                        LoadProfile();
                        GetPosts(false);
                        App.ProfileUpdateType = ProfileUpdateType.None;
                    }
                    else
                    {
                        App.ProfileUpdateType = ProfileUpdateType.Full;
                    }
                }
                base.UserVisibleHint = value;
            }
        }


        public ProfileFragment()
        {
            //This is fix for crashing when app killed in background
        }

        public ProfileFragment(string profileId)
        {
            ProfileId = profileId;
            _loadOnViewCreated = true;
        }


        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (savedInstanceState != null)
                ProfileId = savedInstanceState.GetString("profileId");
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString("profileId", ProfileId);
            base.OnSaveInstanceState(outState);
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

                Presenter.UserName = ProfileId;
                Presenter.SourceChanged += PresenterSourceChanged;

                _login.Typeface = Style.Semibold;
                _firstPostButton.Typeface = Style.Semibold;
                _likePowerLabel.Typeface = Style.Semibold;

                _scrollListner = new ScrollListener();
                _scrollListner.ScrolledToBottom += ScrollListnerScrolledToBottom;

                _linearLayoutManager = new LinearLayoutManager(Context);
                _gridLayoutManager = new GridLayoutManager(Context, 3);
                _profileSpanSizeLookup = new ProfileSpanSizeLookup();
                _gridLayoutManager.SetSpanSizeLookup(_profileSpanSizeLookup);

                _gridItemDecoration = new GridItemDecoration(true);

                _tabOptions = App.User.Login.Equals(ProfileId)
                    ? App.NavigationManager.GetTabSettings($"User_{nameof(ProfileFragment)}")
                    : App.NavigationManager.GetTabSettings(nameof(ProfileFragment));

                SwitchListAdapter(_tabOptions.IsGridView);

                PostsList.AddOnScrollListener(_scrollListner);

                PostPager.SetClipToPadding(false);
                PostPager.SetPadding(Style.PostPagerMargin * 2, 0, Style.PostPagerMargin * 2, 0);
                PostPager.PageMargin = Style.PostPagerMargin;
                PostPager.PageScrollStateChanged += PostPagerOnPageScrollStateChanged;
                PostPager.PageScrolled += PostPagerOnPageScrolled;
                PostPager.Adapter = ProfilePagerAdapter;
                PostPager.SetPageTransformer(false, _profilePagerAdapter, (int)LayerType.None);

                Refresher.Refresh += OnRefresh;
                _settings.Click += OnSettingsClick;
                _login.Click += OnLoginClick;
                _backButton.Click += GoBackClick;
                _switcher.Click += OnSwitcherClick;
                _firstPostButton.Click += OnFirstPostButtonClick;
                _more.Click += MoreOnClick;

                _moreActionsDialog = new BottomSheetDialog(Context);
                _moreActionsDialog.Window.RequestFeature(WindowFeatures.NoTitle);

                _firstPostButton.Text = App.Localization.GetText(LocalizationKeys.CreateFirstPostText);

                if (!string.Equals(ProfileId, App.User.Login, StringComparison.OrdinalIgnoreCase))
                {
                    _settings.Visibility = ViewStates.Gone;
                    _backButton.Visibility = ViewStates.Visible;
                    _more.Visibility = ViewStates.Visible;
                    _more.Enabled = false;
                    _login.Text = ProfileId;
                }
                else
                {
                    _more.Visibility = ViewStates.Gone;
                    _login.Text = App.Localization.GetText(LocalizationKeys.MyProfile);
                }

                if (_loadOnViewCreated || UserVisibleHint)
                    UserVisibleHint = true;
            }
        }

        private void PostPagerOnPageScrolled(object sender, ViewPager.PageScrolledEventArgs pageScrolledEventArgs)
        {
            if (pageScrolledEventArgs.Position == Presenter.Count)
            {
                if (!Presenter.IsLastReaded)
                    GetPosts(false);
            }
        }

        private void PostPagerOnPageScrollStateChanged(object sender, ViewPager.PageScrollStateChangedEventArgs pageScrollStateChangedEventArgs)
        {
            if (pageScrollStateChangedEventArgs.State == 0)
            {
                PostsList.ScrollToPosition(PostPager.CurrentItem + 1);
                if (PostsList.GetLayoutManager() is GridLayoutManager manager)
                {
                    var positionToScroll = PostPager.CurrentItem + (PostPager.CurrentItem - manager.FindFirstVisibleItemPosition()) / 2;
                    PostsList.ScrollToPosition(positionToScroll < Presenter.Count
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

        public override bool ClosePost()
        {
            if (base.ClosePost())
            {
                if (_rvAdapter == ProfileGridAdapter)
                    ProfileGridAdapter.PulseAsync(ProfilePagerAdapter.CurrentPost, Presenter.OnDisposeCts.Token);
                return true;
            }

            return false;
        }

        private void OnSwitcherClick(object sender, EventArgs e)
        {
            _switcher.Enabled = false;
            _tabOptions.IsGridView = !(PostsList.GetLayoutManager() is GridLayoutManager);
            App.NavigationManager.Save();
            SwitchListAdapter(_tabOptions.IsGridView);
            _switcher.Enabled = true;
        }

        private void SwitchListAdapter(bool isGridView)
        {
            if (isGridView)
            {
                _switcher.SetImageResource(Resource.Drawable.ic_grid_active);
                PostsList.SetLayoutManager(_gridLayoutManager);
                PostsList.AddItemDecoration(_gridItemDecoration);
                _rvAdapter = ProfileGridAdapter;
            }
            else
            {
                _switcher.SetImageResource(Resource.Drawable.ic_grid);
                PostsList.SetLayoutManager(_linearLayoutManager);
                PostsList.RemoveItemDecoration(_gridItemDecoration);
                _rvAdapter = ProfileFeedAdapter;
            }
            PostsList.SetAdapter(_rvAdapter);
            PostsList.ScrollToPosition(_scrollListner.Position);
        }

        private void PresenterSourceChanged(Status status)
        {
            if (!IsInitialized)
                return;

            switch (status.Sender)
            {
                case nameof(UserProfilePresenter.TryFollowAsync) when status.IsChanged:
                case nameof(UserProfilePresenter.TryGetUserInfoAsync) when status.IsChanged:
                    {
                        _firstPostButton.Visibility = ProfileId == App.User.Login
                                                      && Presenter.UserProfileResponse.PostCount == 0
                                                      && Presenter.UserProfileResponse.HiddenPostCount == 0
                                ? ViewStates.Visible
                                : ViewStates.Gone;
                        break;
                    }
                default:
                    {
                        _profileSpanSizeLookup.LastItemNumber = Presenter.Count;
                        _rvAdapter.NotifyDataSetChanged();
                        ProfilePagerAdapter.NotifyDataSetChanged();

                        break;
                    }
            }
        }

        private async void OnRefresh(object sender, EventArgs e)
        {
            await Presenter.TryUpdateUserPostsAsync(App.User.Login);
            await UpdatePage(ProfileUpdateType.Full);
            if (!IsInitialized)
                return;
            Refresher.Refreshing = false;
        }

        private void OnFirstPostButtonClick(object sender, EventArgs e)
        {
            var intent = new Intent(Activity, typeof(CameraActivity));
            Activity.StartActivity(intent);
        }

        protected override async Task GetPosts(bool isRefresh)
        {
            if (isRefresh)
                Presenter.Clear();

            var exception = await Presenter.TryLoadNextPostsAsync();
            if (!IsInitialized)
                return;

            Context.ShowAlert(exception, ToastLength.Short);
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
                    dialogView.SetMinimumWidth((int)(Style.ScreenWidth * 0.8));
                    var pushes = dialogView.FindViewById<Button>(Resource.Id.pushes);
                    if (isSubscribed)
                    {
                        pushes.SetTextColor(Style.R255G34B5);
                        pushes.Text = App.Localization.GetText(LocalizationKeys.UnwatchUser);
                    }
                    else
                    {
                        pushes.SetTextColor(Color.Black);
                        pushes.Text = App.Localization.GetText(LocalizationKeys.WatchUser);
                    }

                    pushes.Typeface = Style.Semibold;

                    var cancel = dialogView.FindViewById<Button>(Resource.Id.cancel);
                    cancel.Text = App.Localization.GetText(LocalizationKeys.Cancel);
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
            var model = new PushNotificationsModel(App.User.UserInfo, !isSubscribed)
            {
                WatchedUser = ProfileId
            };

            isSubscription = true;

            var result = await Presenter.TrySubscribeForPushesAsync(model);
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
            PostsList.ScrollToPosition(0);
        }

        private async Task UpdatePage(ProfileUpdateType updateType)
        {
            _scrollListner.ClearPosition();
            if (updateType == ProfileUpdateType.Full)
            {
                _listSpinner.Visibility = ViewStates.Visible;
                await GetPosts(true);
                if (!IsInitialized)
                    return;
                _listSpinner.Visibility = ViewStates.Gone;
            }
            await LoadProfile();
        }

        private async Task LoadProfile()
        {
            do
            {
                var result = await Presenter.TryGetUserInfoAsync(ProfileId);
                if (!IsInitialized)
                    return;

                if (result.IsSuccess || result.Exception is System.OperationCanceledException)
                {
                    _listLayout.Visibility = ViewStates.Visible;
                    _more.Enabled = true;
                    isSubscribed = Presenter.UserProfileResponse.IsSubscribed;
                    break;
                }

                Context.ShowAlert(result, ToastLength.Short);
                await Task.Delay(5000);
                if (!IsInitialized)
                    return;

            } while (true);

            _firstPostButton.Visibility =
                ProfileId == App.User.Login && Presenter.UserProfileResponse.PostCount == 0 && Presenter.UserProfileResponse.HiddenPostCount == 0
                    ? ViewStates.Visible
                    : ViewStates.Gone;
            _loadingSpinner.Visibility = ViewStates.Gone;
        }

        private async void ProfileAction(ActionType type)
        {
            switch (type)
            {
                case ActionType.Transfer:
                    ((BaseActivity)Activity).OpenNewContentFragment(Presenter.UserProfileResponse.Username.Equals(App.User.Login, StringComparison.OrdinalIgnoreCase) ? new TransferFragment() : new TransferFragment(Presenter.UserProfileResponse));
                    break;
                case ActionType.Balance:
                    ((BaseActivity)Activity).OpenNewContentFragment(new WalletFragment());
                    break;
                case ActionType.Followers:
                    Activity.Intent.PutExtra(FollowersFragment.IsFollowersExtra, true);
                    Activity.Intent.PutExtra(FollowersFragment.UsernameExtra, ProfileId);
                    Activity.Intent.PutExtra(FollowersFragment.CountExtra, Presenter.UserProfileResponse.FollowersCount);
                    ((BaseActivity)Activity).OpenNewContentFragment(new FollowersFragment());
                    break;
                case ActionType.Following:
                    Activity.Intent.PutExtra(FollowersFragment.IsFollowersExtra, false);
                    Activity.Intent.PutExtra(FollowersFragment.UsernameExtra, ProfileId);
                    Activity.Intent.PutExtra(FollowersFragment.CountExtra, Presenter.UserProfileResponse.FollowingCount);
                    ((BaseActivity)Activity).OpenNewContentFragment(new FollowersFragment());
                    break;
                case ActionType.Follow:
                    if (App.User.HasPostingPermission)
                    {
                        var result = await Presenter.TryFollowAsync();
                        if (!IsInitialized)
                            return;

                        Context.ShowAlert(result, ToastLength.Long);
                    }
                    else
                    {
                        var intent = new Intent(Activity, typeof(WelcomeActivity));
                        StartActivity(intent);
                    }
                    break;
                case ActionType.LikePower:
                    var avatar = PostsList.FindViewById(Resource.Id.profile_image);
                    avatar.Enabled = false;
                    _likePowerLabel.Text = App.Localization.GetText(LocalizationKeys.PowerOfLike, Presenter.UserProfileResponse.VotingPower);
                    _likePowerLabel.Visibility = ViewStates.Visible;
                    await Task.Delay(1000);
                    _likePowerLabel.Visibility = ViewStates.Gone;
                    avatar.Enabled = true;
                    break;
            }
        }


        public override void OnResume()
        {
            base.OnResume();
            //_adapter.NotifyDataSetChanged();

            if (UserVisibleHint)
                UpdateProfile();
        }

        private void UpdateProfile()
        {
            if (App.ProfileUpdateType != ProfileUpdateType.None)
            {
                UpdatePage(App.ProfileUpdateType);
                App.ProfileUpdateType = ProfileUpdateType.None;
            }
        }
    }
}
