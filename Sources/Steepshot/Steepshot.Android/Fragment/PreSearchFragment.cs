using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Animation;
using Android.Content;
using Android.OS;
using Android.Support.Transitions;
using Android.Support.V4.Content;
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

namespace Steepshot.Fragment
{
    public class PreSearchFragment : BaseFragmentWithPresenter<PreSearchPresenter>
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
#pragma warning restore 0649

        private string CustomTag
        {
            get => Presenter.Tag;
            set => Presenter.Tag = value;
        }

        private FeedAdapter _profileFeedAdapter;
        private FeedAdapter ProfileFeedAdapter
        {
            get
            {
                if (_profileFeedAdapter == null)
                {
                    _profileFeedAdapter = new FeedAdapter(Context, Presenter);
                    _profileFeedAdapter.PhotoClick += OnPhotoClick;
                    _profileFeedAdapter.LikeAction += LikeAction;
                    _profileFeedAdapter.UserAction += UserAction;
                    _profileFeedAdapter.CommentAction += CommentAction;
                    _profileFeedAdapter.VotersClick += VotersAction;
                }
                return _profileFeedAdapter;
            }
        }

        private GridAdapter _profileGridAdapter;
        private GridAdapter ProfileGridAdapter
        {
            get
            {
                if (_profileGridAdapter == null)
                {
                    _profileGridAdapter = new GridAdapter(Context, Presenter);
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

                SetAnimation();
                _buttonsList = new List<Button> { _newButton, _hotButton, _trendingButton };
                _bottomPadding = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 2, Resources.DisplayMetrics);
                _currentButton = _trendingButton;
                _trendingButton.Typeface = Style.Semibold;
                _hotButton.Typeface = Style.Regular;
                _newButton.Typeface = Style.Regular;

                _searchView.Typeface = Style.Regular;
                _clearButton.Typeface = Style.Regular;
                _clearButton.Visibility = ViewStates.Gone;
                _loginButton.Typeface = Style.Semibold;
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
                _searchList.SetAdapter(ProfileGridAdapter);

                _refresher.Refresh += RefresherRefresh;
            }

            var s = Activity.Intent.GetStringExtra("SEARCH");
            if (!string.IsNullOrWhiteSpace(s) && s != CustomTag)
            {
                Activity.Intent.RemoveExtra("SEARCH");
                _searchView.Text = Presenter.Tag = CustomTag = s;
                _searchView.SetTextColor(Style.R15G24B30);
                _clearButton.Visibility = ViewStates.Visible;
                _spinner.Visibility = ViewStates.Visible;

                LoadPosts(true);
            }
            else if (!IsInitialized && _isGuest)
            {
                LoadPosts(true);
            }
        }


        [InjectOnClick(Resource.Id.clear_button)]
        public void OnClearClick(object sender, EventArgs e)
        {
            CustomTag = null;
            _clearButton.Visibility = ViewStates.Gone;
            _searchView.Text = Localization.Texts.TapToSearch;
            _searchView.SetTextColor(Style.R151G155B158);
        }

        [InjectOnClick(Resource.Id.trending_button)]
        public async void OnTrendClick(object sender, EventArgs e)
        {
            await SwitchSearchType(PostType.Top);
        }

        [InjectOnClick(Resource.Id.hot_button)]
        public async void OnTopClick(object sender, EventArgs e)
        {
            await SwitchSearchType(PostType.Hot);
        }

        [InjectOnClick(Resource.Id.new_button)]
        public async void OnNewClick(object sender, EventArgs e)
        {
            await SwitchSearchType(PostType.New);
        }

        [InjectOnClick(Resource.Id.toolbar)]
        public void OnSearch(object sender, EventArgs e)
        {
            ((BaseActivity)Activity).OpenNewContentFragment(new SearchFragment());
        }

        [InjectOnClick(Resource.Id.btn_switcher)]
        public void OnSwitcherClick(object sender, EventArgs e)
        {
            _scrollListner.ClearPosition();
            _searchList.ScrollToPosition(0);
            if (_searchList.GetLayoutManager() is GridLayoutManager)
            {
                _switcher.SetImageResource(Resource.Drawable.grid);
                _searchList.SetLayoutManager(_linearLayoutManager);
                _searchList.RemoveItemDecoration(_gridItemDecoration);
                _searchList.SetAdapter(ProfileFeedAdapter);
            }
            else
            {
                _switcher.SetImageResource(Resource.Drawable.grid_active);
                _searchList.SetLayoutManager(_gridLayoutManager);
                _searchList.AddItemDecoration(_gridItemDecoration);
                _searchList.SetAdapter(ProfileGridAdapter);
            }
        }

        [InjectOnClick(Resource.Id.login)]
        public void OnLogin(object sender, EventArgs e)
        {
            OpenLogin();
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
                var feedAdapter = (FeedAdapter)_searchList.GetAdapter();
                feedAdapter.IsEnableVote = false;
                var errors = await Presenter.TryVote(post);
                if (IsDetached || IsRemoving)
                    return;

                if (errors != null && errors.Count != 0)
                    Context.ShowAlert(errors);
                else
                    await Task.Delay(3000);

                if (IsDetached || IsRemoving)
                    return;

                feedAdapter.IsEnableVote = true;
            }
            else
            {
                OpenLogin();
            }
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

            var intent = new Intent(Context, typeof(CommentsActivity));
            intent.PutExtra(CommentsActivity.PostExtraPath, post.Url);
            Context.StartActivity(intent);
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
                _scrollListner.ClearPosition();
                _feedSpanSizeLookup.LastItemNumber = -1;
                _searchList.GetAdapter()?.NotifyDataSetChanged();
            }

            List<string> errors;
            if (string.IsNullOrEmpty(CustomTag))
                errors = await Presenter.TryLoadNextTopPosts();
            else
                errors = await Presenter.TryGetSearchedPosts();

            if (IsDetached || IsRemoving)
                return;

            if (errors != null && !errors.Any())
            {
                _feedSpanSizeLookup.LastItemNumber = Presenter.Count;
                _searchList.GetAdapter()?.NotifyDataSetChanged();
            }
            else
            {
                Context.ShowAlert(errors);
            }

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

            _fontGrowingAnimation.Update += (sender, e) =>
            {
                _activeButton.SetTextSize(ComplexUnitType.Sp, (float)e.Animation.AnimatedValue);
            };

            _fontReductionAnimation.Update += (sender, e) =>
            {
                _currentButton.SetTextSize(ComplexUnitType.Sp, (float)e.Animation.AnimatedValue);
            };

            _grayToBlackAnimation.Update += (sender, e) =>
            {
                _activeButton.SetTextColor(BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Activity, (int)e.Animation.AnimatedValue)));
            };

            _blackToGrayAnimation.Update += (sender, e) =>
            {
                _currentButton.SetTextColor(BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Activity, (int)e.Animation.AnimatedValue)));
            };

            _blackToGrayAnimation.AnimationEnd += (sender, e) =>
            {
                _currentButton = _activeButton;
            };
        }

        private void AnimatedButtonSwitch()
        {
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
