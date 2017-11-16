using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Animation;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.Transitions;
using Android.Support.V4.Content;
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

namespace Steepshot.Fragment
{
    public sealed class PreSearchFragment : BaseFragmentWithPresenter<PreSearchPresenter>
    {
        private readonly bool _isGuest;
        //ValueAnimator disposing issue probably fixed with static modificator
        private static ValueAnimator _fontGrowingAnimation;
        private static ValueAnimator _fontReductionAnimation;
        private static ValueAnimator _grayToBlackAnimation;
        private static ValueAnimator _blackToGrayAnimation;

        private ScrollListener _scrollListner;
        private LinearLayoutManager _linearLayoutManager;
        private GridLayoutManager _gridLayoutManager;
        private GridItemDecoration _gridItemDecoration;
        private FeedSpanSizeLookup _feedSpanSizeLookup;

        private List<Button> _buttonsList;
        private const int AnimationDuration = 300;
        private const int MinFontSize = 14;
        private const int MaxFontSize = 20;
        private int _bottomPadding;
        private bool _isActivated;
        private RecyclerView.Adapter _adapter;

        private Button _activeButton;
        private Button _currentButton;

#pragma warning disable 0649, 4014
        [InjectView(Resource.Id.search_list)] private RecyclerView _searchList;
        [InjectView(Resource.Id.search_view)] private TextView _searchView;
        [InjectView(Resource.Id.loading_spinner)] private ProgressBar _spinner;
        [InjectView(Resource.Id.trending_button)] private Button _trendingButton;
        [InjectView(Resource.Id.hot_button)] private Button _hotButton;
        [InjectView(Resource.Id.new_button)] private Button _newButton;
        [InjectView(Resource.Id.clear_button)] private Button _clearButton;
        [InjectView(Resource.Id.btn_switcher)] private ImageButton _switcher;
        [InjectView(Resource.Id.refresher)] private SwipeRefreshLayout _refresher;
        [InjectView(Resource.Id.login)] private Button _loginButton;
        [InjectView(Resource.Id.search_type)] private RelativeLayout _searchTypeLayout;
        [InjectView(Resource.Id.toolbar)] private RelativeLayout _toolbarLayout;
        [InjectView(Resource.Id.app_bar)] private AppBarLayout _toolbar;
#pragma warning restore 0649

        private string CustomTag
        {
            get => Presenter.Tag;
            set => Presenter.Tag = value;
        }

        private FeedAdapter<PreSearchPresenter> _profileFeedAdapter;
        private FeedAdapter<PreSearchPresenter> ProfileFeedAdapter
        {
            get
            {
                if (_profileFeedAdapter == null)
                {
                    _profileFeedAdapter = new FeedAdapter<PreSearchPresenter>(Context, Presenter);
                    _profileFeedAdapter.PhotoClick += OnPhotoClick;
                    _profileFeedAdapter.LikeAction += LikeAction;
                    _profileFeedAdapter.UserAction += UserAction;
                    _profileFeedAdapter.CommentAction += CommentAction;
                    _profileFeedAdapter.VotersClick += VotersAction;
                    _profileFeedAdapter.FlagAction += FlagAction;
                    _profileFeedAdapter.HideAction += HideAction;
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
                    _profileGridAdapter.Click += OnPhotoClick;
                }
                return _profileGridAdapter;
            }
        }

        public override bool CustomUserVisibleHint
        {
            get => base.CustomUserVisibleHint;
            set
            {
                if (value && !_isActivated)
                {
                    LoadPosts();
                    _isActivated = true;
                }
                UserVisibleHint = value;
            }
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

                SetAnimation();
                _buttonsList = new List<Button> { _newButton, _hotButton, _trendingButton };
                _bottomPadding = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 2, Resources.DisplayMetrics);
                _currentButton = _trendingButton;
                _trendingButton.Typeface = Style.Semibold;
                _trendingButton.Click += OnTrendClick;
                _hotButton.Typeface = Style.Regular;
                _hotButton.Click += OnTopClick;
                _newButton.Typeface = Style.Regular;
                _newButton.Click += OnNewClick;
                _toolbar.OffsetChanged += OnToolbarOffsetChanged;

                _searchView.Typeface = Style.Regular;
                _clearButton.Typeface = Style.Regular;
                _clearButton.Visibility = ViewStates.Gone;
                _clearButton.Click += OnClearClick;
                _loginButton.Typeface = Style.Semibold;
                _loginButton.Click += OnLogin;
                _scrollListner = new ScrollListener();
                _scrollListner.ScrolledToBottom += ScrollListnerScrolledToBottom;

                _linearLayoutManager = new LinearLayoutManager(Context);
                _gridLayoutManager = new GridLayoutManager(Context, 3);
                _feedSpanSizeLookup = new FeedSpanSizeLookup();
                _gridLayoutManager.SetSpanSizeLookup(_feedSpanSizeLookup);

                _gridItemDecoration = new GridItemDecoration();
                _searchList.SetLayoutManager(_gridLayoutManager);
                _searchList.AddItemDecoration(_gridItemDecoration);
                _searchList.AddOnScrollListener(_scrollListner);
                _adapter = ProfileGridAdapter;
                _searchList.SetAdapter(_adapter);
                _switcher.Click += OnSwitcherClick;
                _refresher.Refresh += RefresherRefresh;

                _toolbarLayout.Click += OnSearch;
            }

            var s = Activity.Intent.GetStringExtra(SearchFragment.SearchExtra);
            if (!string.IsNullOrWhiteSpace(s) && s != CustomTag)
            {
                Activity.Intent.RemoveExtra(SearchFragment.SearchExtra);
                _searchView.Text = Presenter.Tag = CustomTag = s;
                _searchView.SetTextColor(Style.R15G24B30);
                _clearButton.Visibility = ViewStates.Visible;
                _spinner.Visibility = ViewStates.Visible;

                LoadPosts(true);
            }
            else if (savedInstanceState == null && _isGuest)
            {
                LoadPosts(true);
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
                _adapter.NotifyDataSetChanged();
            }
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
            lock (_switcher)
            {
                _scrollListner.ClearPosition();
                _searchList.ScrollToPosition(0);
                if (_searchList.GetLayoutManager() is GridLayoutManager)
                {
                    _switcher.SetImageResource(Resource.Drawable.grid);
                    _searchList.SetLayoutManager(_linearLayoutManager);
                    _searchList.RemoveItemDecoration(_gridItemDecoration);
                    _adapter = ProfileFeedAdapter;
                }
                else
                {
                    _switcher.SetImageResource(Resource.Drawable.grid_active);
                    _searchList.SetLayoutManager(_gridLayoutManager);
                    _searchList.AddItemDecoration(_gridItemDecoration);
                    _adapter = ProfileGridAdapter;
                }
                _searchList.SetAdapter(_adapter);
            }
        }

        private void OnLogin(object sender, EventArgs e)
        {
            OpenLogin();
        }

        private void PresenterSourceChanged(Status status)
        {
            if (!IsInitialized || IsDetached || IsRemoving)
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
                }
            });
        }

        private async void ScrollListnerScrolledToBottom()
        {
            await LoadPosts();
        }

        private async void RefresherRefresh(object sender, EventArgs e)
        {
            _spinner.Visibility = ViewStates.Gone;
            await LoadPosts(true);
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
                var errors = await Presenter.TryVote(post);
                if (!IsInitialized || IsDetached || IsRemoving)
                    return;

                Context.ShowAlert(errors);
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

            var errors = await Presenter.TryFlag(post);
            if (!IsInitialized || IsDetached || IsRemoving)
                return;

            Context.ShowAlert(errors);
        }

        private void HideAction(Post post)
        {
            Presenter.RemovePost(post);
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
            if (post.Children > 0)
            {
                var intent = new Intent(Context, typeof(CommentsActivity));
                intent.PutExtra(CommentsActivity.PostExtraPath, post.Url);
                StartActivityForResult(intent, CommentsActivity.RequestCode);
            }
            else
                OpenLogin();
        }

        private void VotersAction(Post post)
        {
            if (post == null)
                return;

            Activity.Intent.PutExtra(FeedFragment.PostUrlExtraPath, post.Url);
            Activity.Intent.PutExtra(FeedFragment.PostNetVotesExtraPath, post.NetVotes);
            ((BaseActivity)Activity).OpenNewContentFragment(new VotersFragment());
        }

        private async Task LoadPosts(bool clearOld = false)
        {
            if (clearOld)
            {
                Presenter.LoadCancel();
                Presenter.Clear();
            }

            List<string> errors;
            if (string.IsNullOrEmpty(CustomTag))
                errors = await Presenter.TryLoadNextTopPosts();
            else
                errors = await Presenter.TryGetSearchedPosts();

            if (!IsInitialized || IsDetached || IsRemoving)
                return;

            Context.ShowAlert(errors);

            _refresher.Refreshing = false;
            _spinner.Visibility = ViewStates.Gone;
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
            await LoadPosts(true);
        }

        private void SetAnimation()
        {
            _fontGrowingAnimation = ValueAnimator.OfFloat(MinFontSize, MaxFontSize);
            _fontGrowingAnimation.SetDuration(AnimationDuration);

            _fontReductionAnimation = ValueAnimator.OfFloat(MaxFontSize, MinFontSize);
            _fontReductionAnimation.SetDuration(AnimationDuration);

            _grayToBlackAnimation = ValueAnimator.OfArgb(Resource.Color.rgb151_155_158, Resource.Color.rgb15_24_30);
            _grayToBlackAnimation.SetDuration(AnimationDuration);

            _blackToGrayAnimation = ValueAnimator.OfArgb(Resource.Color.rgb15_24_30, Resource.Color.rgb151_155_158);
            _blackToGrayAnimation.SetDuration(AnimationDuration);

            _fontGrowingAnimation.Update += OnFontGrowingAnimationOnUpdate;
            _fontReductionAnimation.Update += OnFontReductionAnimationOnUpdate;
            _grayToBlackAnimation.Update += OnGrayToBlackAnimationOnUpdate;
            _blackToGrayAnimation.Update += OnBlackToGrayAnimationOnUpdate;
            _blackToGrayAnimation.AnimationEnd += OnBlackToGrayAnimationOnAnimationEnd;
        }

        private void OnBlackToGrayAnimationOnAnimationEnd(object sender, EventArgs e)
        {
            _currentButton = _activeButton;
            _hotButton.Enabled = _newButton.Enabled = _trendingButton.Enabled = true;
        }

        private void OnBlackToGrayAnimationOnUpdate(object sender, ValueAnimator.AnimatorUpdateEventArgs e)
        {
            _currentButton.SetTextColor(BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Activity, (int)e.Animation.AnimatedValue)));
        }

        private void OnGrayToBlackAnimationOnUpdate(object sender, ValueAnimator.AnimatorUpdateEventArgs e)
        {
            _activeButton.SetTextColor(BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Activity, (int)e.Animation.AnimatedValue)));
        }

        private void OnFontReductionAnimationOnUpdate(object sender, ValueAnimator.AnimatorUpdateEventArgs e)
        {
            _currentButton.SetTextSize(ComplexUnitType.Sp, (float)e.Animation.AnimatedValue);
        }

        private void OnFontGrowingAnimationOnUpdate(object sender, ValueAnimator.AnimatorUpdateEventArgs e)
        {
            _activeButton.SetTextSize(ComplexUnitType.Sp, (float)e.Animation.AnimatedValue);
        }

        private void AnimatedButtonSwitch()
        {
            _hotButton.Enabled = _newButton.Enabled = _trendingButton.Enabled = false;
            TransitionManager.BeginDelayedTransition(_searchTypeLayout);

            _activeButton.Typeface = Style.Semibold;
            _currentButton.Typeface = Style.Regular;

            _activeButton.SetPadding(0, 0, 0, 0);
            _currentButton.SetPadding(0, 0, 0, _bottomPadding);

            var lastButton = _buttonsList.OrderByDescending(b => b.GetX()).First();

            RelativeLayout.LayoutParams activeButtonLayoutParameters = (RelativeLayout.LayoutParams)_activeButton.LayoutParameters;
            activeButtonLayoutParameters.RemoveRule(LayoutRules.RightOf);
            _activeButton.LayoutParameters = activeButtonLayoutParameters;

            RelativeLayout.LayoutParams currentButtonLayoutParameters = (RelativeLayout.LayoutParams)_currentButton.LayoutParameters;
            currentButtonLayoutParameters.AddRule(LayoutRules.RightOf, lastButton.Id);
            _currentButton.LayoutParameters = currentButtonLayoutParameters;

            _fontGrowingAnimation.Start();
            _fontReductionAnimation.Start();
            _grayToBlackAnimation.Start();
            _blackToGrayAnimation.Start();
        }
    }
}
