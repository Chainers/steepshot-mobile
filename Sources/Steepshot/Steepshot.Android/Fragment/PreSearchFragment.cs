using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.Transitions;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Utils;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Enums;

namespace Steepshot.Fragment
{
    public sealed class PreSearchFragment : BasePostsFragment<PreSearchPresenter>
    {
        public const string IsGuestKey = "isGuest";

        private bool _isGuest;
        private readonly bool _loadOnViewCreated;
        private TabOptions _tabOptions;

        private FeedScrollListner _scrollListner;
        private LinearLayoutManager _linearLayoutManager;
        private GridLayoutManager _gridLayoutManager;
        private GridItemDecoration _gridItemDecoration;
        private FeedSpanSizeLookup _feedSpanSizeLookup;

        private List<Button> _buttonsList;
        private int _bottomPadding;
        private RecyclerView.Adapter _rvAdapter;

        private Button _activeButton;
        private Button _currentButton;

#pragma warning disable 0649, 4014
        [BindView(Resource.Id.search_view)] private TextView _searchView;
        [BindView(Resource.Id.loading_spinner)] private ProgressBar _spinner;
        [BindView(Resource.Id.trending_button)] private Button _trendingButton;
        [BindView(Resource.Id.hot_button)] private Button _hotButton;
        [BindView(Resource.Id.new_button)] private Button _newButton;
        [BindView(Resource.Id.clear_button)] private Button _clearButton;
        [BindView(Resource.Id.btn_layout_switcher)] private ImageButton _switcher;
        [BindView(Resource.Id.login)] private Button _loginButton;
        [BindView(Resource.Id.search_type)] private RelativeLayout _searchTypeLayout;
        [BindView(Resource.Id.toolbar)] private RelativeLayout _toolbarLayout;
        [BindView(Resource.Id.app_bar)] private AppBarLayout _toolbar;
        [BindView(Resource.Id.empty_query_label)] private TextView _emptyQueryLabel;
        [BindView(Resource.Id.btn_back)] private ImageButton _backButton;
        [BindView(Resource.Id.btn_switcher)] private ImageButton _panelSwitcher;
        [BindView(Resource.Id.btn_settings)] private ImageButton _settings;
        [BindView(Resource.Id.profile_login)] private TextView _viewTitle;
        [BindView(Resource.Id.search_toolbar)] private RelativeLayout _searchToolbarLayout;
        [BindView(Resource.Id.tag_toolbar)] private RelativeLayout _tagToolbarLayout;
#pragma warning restore 0649

        private string _customTag;

        private FeedAdapter<PreSearchPresenter> _profileFeedAdapter;
        private FeedAdapter<PreSearchPresenter> ProfileFeedAdapter
        {
            get
            {
                if (_profileFeedAdapter == null)
                {
                    _profileFeedAdapter = new FeedAdapter<PreSearchPresenter>(Context, Presenter);
                    _profileFeedAdapter.PostAction += PostAction;
                    _profileFeedAdapter.AutoLinkAction += AutoLinkAction;
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
                if (value && IsInitialized)
                {
                    if (!string.IsNullOrEmpty(_customTag))
                    {
                        _emptyQueryLabel.Visibility = ViewStates.Invisible;

                        _viewTitle.Text = _searchView.Text = Presenter.Tag = _customTag;
                        _searchView.SetTextColor(Style.R15G24B30);
                        _clearButton.Visibility = ViewStates.Visible;
                        _spinner.Visibility = ViewStates.Visible;

                        _searchToolbarLayout.Visibility = ViewStates.Gone;
                        _tagToolbarLayout.Visibility = ViewStates.Visible;
                    }

                    GetPosts(false);
                }
                base.UserVisibleHint = value;
            }
        }

        public PreSearchFragment()
        {
            //This is fix for crashing when app killed in background
        }

        public PreSearchFragment(string tag)
        {
            _customTag = tag;
            _loadOnViewCreated = true;
        }

        public PreSearchFragment(bool isGuest = false)
        {
            _isGuest = isGuest;
            _loadOnViewCreated = true;
        }


        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (savedInstanceState != null)
                _isGuest = savedInstanceState.GetBoolean(IsGuestKey);
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutBoolean(IsGuestKey, _isGuest);
            base.OnSaveInstanceState(outState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_presearch, null);
                Cheeseknife.Bind(this, InflatedView);
            }
            ToggleTabBar();
            return InflatedView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);

            if (App.User.HasPostingPermission)
                _loginButton.Visibility = ViewStates.Gone;

            Presenter.SourceChanged += PresenterSourceChanged;

            _hotButton.Text = App.Localization.GetText(LocalizationKeys.Hot);
            _newButton.Text = App.Localization.GetText(LocalizationKeys.New);
            _trendingButton.Text = App.Localization.GetText(LocalizationKeys.Top);
            _clearButton.Text = App.Localization.GetText(LocalizationKeys.Clear);

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

            _searchView.Text = App.Localization.GetText(LocalizationKeys.TapToSearch);
            _searchView.Typeface = Style.Regular;
            _clearButton.Typeface = Style.Regular;
            _clearButton.Visibility = ViewStates.Gone;
            _clearButton.Click += OnClearClick;
            _loginButton.Typeface = Style.Semibold;
            _loginButton.Text = App.Localization.GetText(LocalizationKeys.SignIn);
            _loginButton.Click += OnLogin;
            _scrollListner = new FeedScrollListner();
            _scrollListner.ScrolledToBottom += ScrollListnerScrolledToBottom;

            _linearLayoutManager = new LinearLayoutManager(Context);
            _gridLayoutManager = new GridLayoutManager(Context, 3);
            _feedSpanSizeLookup = new FeedSpanSizeLookup();
            _gridLayoutManager.SetSpanSizeLookup(_feedSpanSizeLookup);

            _gridItemDecoration = new GridItemDecoration();

            _tabOptions = App.NavigationManager.GetTabSettings(nameof(PreSearchFragment));
            SwitchListAdapter(_tabOptions.IsGridView);
            PostsList.AddOnScrollListener(_scrollListner);

            _switcher.Click += OnSwitcherClick;
            Refresher.Refresh += OnRefresh;

            _emptyQueryLabel.Typeface = Style.Light;
            _emptyQueryLabel.Text = App.Localization.GetText(LocalizationKeys.EmptyQuery);

            _searchToolbarLayout.Click += OnSearch;

            _viewTitle.Typeface = Style.Semibold;
            _backButton.Visibility = ViewStates.Visible;
            _backButton.Click += GoBackClick;
            _panelSwitcher.Visibility = ViewStates.Gone;
            _settings.Visibility = ViewStates.Gone;

            if (_loadOnViewCreated || UserVisibleHint)
                UserVisibleHint = true;
        }

        private void PostPagerOnPageScrolled(object sender, ViewPager.PageScrolledEventArgs pageScrolledEventArgs)
        {
            if (pageScrolledEventArgs.Position == Presenter.Count)
            {
                if (!Presenter.IsLastReaded)
                    GetPosts(false);
            }
        }

        private void FeedPhotoClick(Post post)
        {
            if (post == null)
                return;

            OpenPost(post);
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
                default:
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

                        _rvAdapter.NotifyDataSetChanged();                        

                        break;
                    }
            }
        }

        private async void OnRefresh(object sender, EventArgs e)
        {
            _spinner.Visibility = ViewStates.Gone;
            await GetPosts(true);
        }

        private void OnLogin(object sender, EventArgs e)
        {
            OpenLogin();
        }

        private void OnToolbarOffsetChanged(object sender, AppBarLayout.OffsetChangedEventArgs e)
        {
            ViewCompat.SetElevation(_toolbar, MediaUtils.DpToPixel(2, Resources));
        }

        private void OnClearClick(object sender, EventArgs e)
        {
            Presenter.Tag = _customTag = null;
            _clearButton.Visibility = ViewStates.Gone;
            _searchView.Text = App.Localization.GetText(LocalizationKeys.TapToSearch);
            _searchView.SetTextColor(Style.R151G155B158);
            _spinner.Visibility = ViewStates.Visible;
            _emptyQueryLabel.Visibility = ViewStates.Invisible;
            Refresher.Refreshing = false;
            GetPosts(true);
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

        protected override async Task GetPosts(bool clearOld)
        {
            if (clearOld)
            {
                Presenter.LoadCancel();
                Presenter.Clear();
            }

            Exception exception;
            if (string.IsNullOrEmpty(Presenter.Tag))
                exception = await Presenter.TryLoadNextTopPostsAsync();
            else
                exception = await Presenter.TryGetSearchedPostsAsync();

            if (!IsInitialized)
                return;

            Context.ShowAlert(exception, ToastLength.Short);

            if (exception == null)
            {
                Refresher.Refreshing = false;
                _spinner.Visibility = ViewStates.Gone;
                _emptyQueryLabel.Visibility = Presenter.Count > 0 ? ViewStates.Invisible : ViewStates.Visible;
            }
        }

        private async Task SwitchSearchType(PostType postType)
        {
            if (postType == Presenter.PostType)
                return;
            _emptyQueryLabel.Visibility = ViewStates.Invisible;
            _spinner.Visibility = ViewStates.Visible;
            Refresher.Refreshing = false;

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
            await GetPosts(true);
            if (postType == Presenter.PostType)
                _emptyQueryLabel.Visibility = Presenter.Count > 0 ? ViewStates.Invisible : ViewStates.Visible;
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
