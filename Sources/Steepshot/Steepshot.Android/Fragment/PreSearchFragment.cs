using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.Transitions;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Activity;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Utils;
using Steepshot.Core.Models;
using Steepshot.Core.Authority;
using Steepshot.Interfaces;
using Steepshot.Core.Errors;
using Steepshot.Core.Models.Enums;

namespace Steepshot.Fragment
{
    public sealed class PreSearchFragment : BaseFragmentWithPresenter<PreSearchPresenter>, ICanOpenPost
    {
        public const string IsGuestKey = "isGuest";

        private bool _isGuest;
        private TabSettings _tabSettings;

        private ScrollListener _scrollListner;
        private LinearLayoutManager _linearLayoutManager;
        private GridLayoutManager _gridLayoutManager;
        private GridItemDecoration _gridItemDecoration;
        private FeedSpanSizeLookup _feedSpanSizeLookup;

        private List<Button> _buttonsList;
        private int _bottomPadding;
        private bool _isActivated;
        private bool _isNeedToLoadPosts;
        private RecyclerView.Adapter _adapter;

        private Button _activeButton;
        private Button _currentButton;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.search_list)] private RecyclerView _postsList;
        [InjectView(Resource.Id.search_view)] private TextView _searchView;
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar _spinner;
        [InjectView(Resource.Id.trending_button)] private Button _trendingButton;
        [InjectView(Resource.Id.hot_button)] private Button _hotButton;
        [InjectView(Resource.Id.new_button)] private Button _newButton;
        [InjectView(Resource.Id.clear_button)] private Button _clearButton;
        [InjectView(Resource.Id.btn_layout_switcher)] private ImageButton _switcher;
        [InjectView(Resource.Id.refresher)] private SwipeRefreshLayout _refresher;
        [InjectView(Resource.Id.login)] private Button _loginButton;
        [InjectView(Resource.Id.search_type)] private RelativeLayout _searchTypeLayout;
        [InjectView(Resource.Id.toolbar)] private RelativeLayout _toolbarLayout;
        [InjectView(Resource.Id.app_bar)] private AppBarLayout _toolbar;
        [InjectView(Resource.Id.empty_query_label)] private TextView _emptyQueryLabel;
        [InjectView(Resource.Id.btn_back)] private ImageButton _backButton;
        [InjectView(Resource.Id.btn_switcher)] private ImageButton _panelSwitcher;
        [InjectView(Resource.Id.btn_settings)] private ImageButton _settings;
        [InjectView(Resource.Id.profile_login)] private TextView _viewTitle;
        [InjectView(Resource.Id.search_toolbar)] private RelativeLayout _searchToolbarLayout;
        [InjectView(Resource.Id.tag_toolbar)] private RelativeLayout _tagToolbarLayout;
        [InjectView(Resource.Id.post_prev_pager)] private ViewPager _postPager;
#pragma warning restore 0649

        private string CustomTag
        {
            get => Presenter.Tag;
            set => Presenter.Tag = value;
        }

        private PostPagerAdapter<PreSearchPresenter> _profilePagerAdapter;
        private PostPagerAdapter<PreSearchPresenter> ProfilePagerAdapter
        {
            get
            {
                if (_profilePagerAdapter == null)
                {
                    _profilePagerAdapter = new PostPagerAdapter<PreSearchPresenter>(Context, Presenter);
                    _profilePagerAdapter.LikeAction += LikeAction;
                    _profilePagerAdapter.UserAction += UserAction;
                    _profilePagerAdapter.CommentAction += CommentAction;
                    _profilePagerAdapter.VotersClick += VotersAction;
                    _profilePagerAdapter.PhotoClick += OnPhotoClick;
                    _profilePagerAdapter.FlagAction += FlagAction;
                    _profilePagerAdapter.HideAction += HideAction;
                    _profilePagerAdapter.DeleteAction += DeleteAction;
                    _profilePagerAdapter.TagAction += TagAction;
                    _profilePagerAdapter.CloseAction += CloseAction;
                }
                return _profilePagerAdapter;
            }
        }

        private FeedAdapter<PreSearchPresenter> _profileFeedAdapter;
        private FeedAdapter<PreSearchPresenter> ProfileFeedAdapter
        {
            get
            {
                if (_profileFeedAdapter == null)
                {
                    _profileFeedAdapter = new FeedAdapter<PreSearchPresenter>(Context, Presenter);
                    _profileFeedAdapter.PhotoClick += FeedPhotoClick;
                    _profileFeedAdapter.LikeAction += LikeAction;
                    _profileFeedAdapter.UserAction += UserAction;
                    _profileFeedAdapter.CommentAction += CommentAction;
                    _profileFeedAdapter.VotersClick += VotersAction;
                    _profileFeedAdapter.FlagAction += FlagAction;
                    _profileFeedAdapter.HideAction += HideAction;
                    _profileFeedAdapter.DeleteAction += DeleteAction;
                    _profileFeedAdapter.TagAction += TagAction;
                }
                return _profileFeedAdapter;
            }
        }

        private GridAdapter<PreSearchPresenter> _profileGridAdapter;
        private GridAdapter<PreSearchPresenter> ProfileGridAdapter
        {
            get
            {
                if (_profileGridAdapter == null)
                {
                    _profileGridAdapter = new GridAdapter<PreSearchPresenter>(Context, Presenter);
                    _profileGridAdapter.Click += FeedPhotoClick;
                }
                return _profileGridAdapter;
            }
        }

        public override bool UserVisibleHint
        {
            get
            {
                return base.UserVisibleHint;
            }
            set
            {
                if (value)
                {
                    var isLoaded = LoadPostsByTag();
                    if (!isLoaded && !_isActivated)
                    {
                        if (Presenter != null)
                            LoadPosts(null, false);
                        else
                            _isNeedToLoadPosts = true;
                        _isActivated = true;
                    }
                }
                base.UserVisibleHint = value;
            }
        }

        public PreSearchFragment()
        {
            //This is fix for crashing when app killed in background
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            if (savedInstanceState != null)
                _isGuest = savedInstanceState.GetBoolean(IsGuestKey);
            base.OnCreate(savedInstanceState);
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutBoolean(IsGuestKey, _isGuest);
            base.OnSaveInstanceState(outState);
        }

        public PreSearchFragment(bool isGuest = false)
        {
            _isGuest = isGuest;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_presearch, null);
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

                if (BasePresenter.User.IsAuthenticated)
                    _loginButton.Visibility = ViewStates.Gone;

                Presenter.SourceChanged += PresenterSourceChanged;

                _buttonsList = new List<Button> { _newButton, _hotButton, _trendingButton };
                _bottomPadding = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 3, Resources.DisplayMetrics);
                _currentButton = _hotButton;
                _trendingButton.Typeface = Style.Regular;
                _trendingButton.Click += OnTrendClick;
                _hotButton.Typeface = Style.Semibold;
                _hotButton.Click += OnTopClick;
                _newButton.Typeface = Style.Regular;
                _newButton.Click += OnNewClick;
                _toolbar.OffsetChanged += OnToolbarOffsetChanged;

                _searchView.Typeface = Style.Regular;
                _clearButton.Typeface = Style.Regular;
                _clearButton.Visibility = ViewStates.Gone;
                _clearButton.Click += OnClearClick;
                _loginButton.Typeface = Style.Semibold;
                _loginButton.Text = Localization.Texts.SignIn;
                _loginButton.Click += OnLogin;
                _scrollListner = new ScrollListener();
                _scrollListner.ScrolledToBottom += ScrollListnerScrolledToBottom;

                _linearLayoutManager = new LinearLayoutManager(Context);
                _gridLayoutManager = new GridLayoutManager(Context, 3);
                _feedSpanSizeLookup = new FeedSpanSizeLookup();
                _gridLayoutManager.SetSpanSizeLookup(_feedSpanSizeLookup);

                _gridItemDecoration = new GridItemDecoration();

                _tabSettings = BasePresenter.User.GetTabSettings(nameof(PreSearchFragment));
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

                _switcher.Click += OnSwitcherClick;
                _refresher.Refresh += RefresherRefresh;

                _emptyQueryLabel.Typeface = Style.Light;
                _emptyQueryLabel.Text = Localization.Texts.EmptyQuery;

                _searchToolbarLayout.Click += OnSearch;

                _viewTitle.Typeface = Style.Semibold;
                _backButton.Visibility = ViewStates.Visible;
                _backButton.Click += GoBackClick;
                _panelSwitcher.Visibility = ViewStates.Gone;
                _settings.Visibility = ViewStates.Gone;
            }

            var isLoaded = LoadPostsByTag();
            if (!isLoaded && savedInstanceState == null && _isNeedToLoadPosts)
            {
                _isNeedToLoadPosts = false;
                LoadPosts(null, true);
            }

            var postUrl = Activity?.Intent?.GetStringExtra(CommentsFragment.ResultString);
            if (!string.IsNullOrWhiteSpace(postUrl))
            {
                var count = Activity.Intent.GetIntExtra(CommentsFragment.CountString, 0);
                Activity.Intent.RemoveExtra(CommentsFragment.ResultString);
                Activity.Intent.RemoveExtra(CommentsFragment.CountString);
                var post = Presenter.FirstOrDefault(p => p.Url == postUrl);
                post.Children += count;
                _adapter.NotifyDataSetChanged();
            }
        }

        public override void OnResume()
        {
            base.OnResume();
            if (_postPager.Visibility == ViewStates.Visible)
                if (Activity is RootActivity activity)
                    activity._tabLayout.Visibility = ViewStates.Invisible;
        }

        private void PostPagerOnPageScrolled(object sender, ViewPager.PageScrolledEventArgs pageScrolledEventArgs)
        {
            if (pageScrolledEventArgs.Position == Presenter.Count)
            {
                if (!Presenter.IsLastReaded)
                    LoadPosts(CustomTag, false);
                else
                    _profilePagerAdapter.NotifyDataSetChanged();
            }
        }

        private void PostPagerOnPageScrollStateChanged(object sender, ViewPager.PageScrollStateChangedEventArgs pageScrollStateChangedEventArgs)
        {
            if (pageScrollStateChangedEventArgs.State == 0)
            {
                _profilePagerAdapter.CurrentItem = _postPager.CurrentItem;
                _postsList.ScrollToPosition(_postPager.CurrentItem);
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
            _postPager.Visibility = ViewStates.Visible;
            _postsList.Visibility = ViewStates.Gone;
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
                    var seenItem = _postsList.FindViewHolderForAdapterPosition(_postPager.CurrentItem)?.ItemView
                        .FindViewById(Resource.Id.grid_item_photo) as ImageView;
                    if (seenItem != null)
                        AnimationHelper.PulseGridItem(seenItem);
                }
                return true;
            }
            return false;
        }

        private void OnToolbarOffsetChanged(object sender, AppBarLayout.OffsetChangedEventArgs e)
        {
            ViewCompat.SetElevation(_toolbar, BitmapUtils.DpToPixel(2, Resources));
        }

        public override void OnDetach()
        {
            base.OnDetach();
            Cheeseknife.Reset(this);
        }

        private void OnClearClick(object sender, EventArgs e)
        {
            CustomTag = null;
            _clearButton.Visibility = ViewStates.Gone;
            _searchView.Text = Localization.Texts.TapToSearch;
            _searchView.SetTextColor(Style.R151G155B158);
            _spinner.Visibility = ViewStates.Visible;
            _emptyQueryLabel.Visibility = ViewStates.Invisible;
            _refresher.Refreshing = false;
            LoadPosts(null, true);
        }

        private async void OnTrendClick(object sender, EventArgs e)
        {
            await SwitchSearchType(PostType.Top);
        }

        private async void OnTopClick(object sender, EventArgs e)
        {
            await SwitchSearchType(PostType.Hot);
        }

        private async void OnNewClick(object sender, EventArgs e)
        {
            await SwitchSearchType(PostType.New);
        }

        private void OnSearch(object sender, EventArgs e)
        {
            ((BaseActivity)Activity).OpenNewContentFragment(new SearchFragment());
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

        private void OnLogin(object sender, EventArgs e)
        {
            OpenLogin();
        }

        private void GoBackClick(object sender, EventArgs e)
        {
            Activity.OnBackPressed();
        }

        private void PresenterSourceChanged(Status status)
        {
            if (!IsInitialized)
                return;

            Activity.RunOnUiThread(() =>
            {
                lock (_switcher)
                {
                    if (Presenter.Count == 0)
                    {
                        _scrollListner.ClearPosition();
                        _feedSpanSizeLookup.LastItemNumber = -1;
                    }
                    else
                    {
                        _feedSpanSizeLookup.LastItemNumber = Presenter.Count;
                    }
                    _adapter.NotifyDataSetChanged();
                    ProfilePagerAdapter.NotifyDataSetChanged();
                }
            });
        }

        private async void ScrollListnerScrolledToBottom()
        {
            await LoadPosts(CustomTag, false);
        }

        private async void RefresherRefresh(object sender, EventArgs e)
        {
            _spinner.Visibility = ViewStates.Gone;
            await LoadPosts(CustomTag, true);
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
            {
                OpenLogin();
            }
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
            Presenter.RemovePost(post);
        }

        private void TagAction(string tag)
        {
            if (tag != null)
            {
                Activity.Intent.PutExtra(SearchFragment.SearchExtra, tag);
                ((BaseActivity)Activity).OpenNewContentFragment(new PreSearchFragment());
            }
            else
                _adapter.NotifyDataSetChanged();
        }

        private void UserAction(Post post)
        {
            if (post == null)
                return;

            if (BasePresenter.User.Login != post.Author)
                ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(post.Author));
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

        private void CloseAction()
        {
            ClosePost();
        }

        private async void DeleteAction(Post post)
        {
            var error = await Presenter.TryDeletePost(post);
            if (!IsInitialized)
                return;

            Context.ShowAlert(error);
        }

        private async Task LoadPosts(string tag, bool clearOld)
        {
            if (clearOld)
            {
                Presenter.LoadCancel();
                Presenter.Clear();
            }

            ErrorBase error;
            if (string.IsNullOrEmpty(tag))
                error = await Presenter.TryLoadNextTopPosts();
            else
                error = await Presenter.TryGetSearchedPosts();

            if (!IsInitialized)
                return;

            if (error is TaskCanceledError)
                return;

            Context.ShowAlert(error);

            if (error == null)
            {
                _refresher.Refreshing = false;
                _spinner.Visibility = ViewStates.Gone;
                _emptyQueryLabel.Visibility = Presenter.Count > 0 ? ViewStates.Invisible : ViewStates.Visible;
            }
        }

        private void OpenLogin()
        {
            var intent = new Intent(Activity, typeof(WelcomeActivity));
            StartActivity(intent);
        }

        private async Task SwitchSearchType(PostType postType)
        {
            if (postType == Presenter.PostType)
                return;
            _emptyQueryLabel.Visibility = ViewStates.Invisible;
            _spinner.Visibility = ViewStates.Visible;
            _refresher.Refreshing = false;

            switch (postType)
            {
                case PostType.Top:
                    _activeButton = _trendingButton;
                    AnimatedButtonSwitch();
                    break;
                case PostType.Hot:
                    _activeButton = _hotButton;
                    AnimatedButtonSwitch();
                    break;
                case PostType.New:
                    _activeButton = _newButton;
                    AnimatedButtonSwitch();
                    break;
            }
            Presenter.PostType = postType;
            await LoadPosts(CustomTag, true);
            if (postType == Presenter.PostType)
                _emptyQueryLabel.Visibility = Presenter.Count > 0 ? ViewStates.Invisible : ViewStates.Visible;
        }

        private bool LoadPostsByTag(string tag = null)
        {
            if (IsInitialized)
            {
                var selectedTag = tag ?? Activity?.Intent?.GetStringExtra(SearchFragment.SearchExtra);
                _emptyQueryLabel.Visibility = ViewStates.Invisible;
                if (!string.IsNullOrWhiteSpace(selectedTag) && selectedTag != CustomTag)
                {
                    Activity.Intent.RemoveExtra(SearchFragment.SearchExtra);
                    _viewTitle.Text = _searchView.Text = Presenter.Tag = CustomTag = selectedTag;
                    _searchView.SetTextColor(Style.R15G24B30);
                    _clearButton.Visibility = ViewStates.Visible;
                    _spinner.Visibility = ViewStates.Visible;

                    _searchToolbarLayout.Visibility = ViewStates.Gone;
                    _tagToolbarLayout.Visibility = ViewStates.Visible;

                    LoadPosts(selectedTag, true);
                    return true;
                }
            }
            return false;
        }

        private async Task ButtonSwitchAnimation()
        {
            const int loop = 3;
            const int minFontSize = 14;
            const int maxFontSize = 20;
            const int minR = 15, minG = 24, minB = 30;
            const int maxR = 151, maxG = 155, maxB = 158;

            var fontSizeDelta = (maxFontSize - minFontSize) / (loop - 1);
            var deltaR = (maxR - minR) / (loop - 1);
            var deltaG = (maxG - minG) / (loop - 1);
            var deltaB = (maxB - minB) / (loop - 1);

            for (int i = 0; i < loop; i++)
            {
                //fontGrowingAnimation
                _activeButton.SetTextSize(ComplexUnitType.Sp, minFontSize + fontSizeDelta * i);

                //fontReductionAnimation
                _currentButton.SetTextSize(ComplexUnitType.Sp, maxFontSize - fontSizeDelta * i);

                //grayToBlackAnimation
                _activeButton.SetTextColor(Color.Rgb(maxR - deltaR * i, maxG - deltaG * i, maxB - deltaB * i));

                //blackToGrayAnimation
                _currentButton.SetTextColor(Color.Rgb(minR + deltaR * i, minG + deltaG * i, minB + deltaB * i));

                await Task.Delay(100);
            }
        }

        private async Task AnimatedButtonSwitch()
        {
            _hotButton.Enabled = _newButton.Enabled = _trendingButton.Enabled = false;
            TransitionManager.BeginDelayedTransition(_searchTypeLayout);

            _activeButton.Typeface = Style.Semibold;
            _currentButton.Typeface = Style.Regular;

            _activeButton.SetPadding(0, 0, 0, _bottomPadding * 2);
            _currentButton.SetPadding(0, 0, 0, _bottomPadding);

            var lastButton = _buttonsList.OrderByDescending(b => b.GetX()).First();

            var activeButtonLayoutParameters = (RelativeLayout.LayoutParams)_activeButton.LayoutParameters;
            activeButtonLayoutParameters.RemoveRule(LayoutRules.RightOf);
            _activeButton.LayoutParameters = activeButtonLayoutParameters;

            var currentButtonLayoutParameters = (RelativeLayout.LayoutParams)_currentButton.LayoutParameters;
            currentButtonLayoutParameters.AddRule(LayoutRules.RightOf, lastButton.Id);
            _currentButton.LayoutParameters = currentButtonLayoutParameters;

            await ButtonSwitchAnimation();

            _currentButton = _activeButton;
            _hotButton.Enabled = _newButton.Enabled = _trendingButton.Enabled = true;
        }
    }
}
