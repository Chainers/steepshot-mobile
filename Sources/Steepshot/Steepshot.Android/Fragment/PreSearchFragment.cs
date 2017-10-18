using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Animation;
using Android.Content;
using Android.Graphics;
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
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class PreSearchFragment : BaseFragmentWithPresenter<PreSearchPresenter>
    {
        private Typeface _font, _semiboldFont;
        private ScrollListener _scrollListner;
        private LinearLayoutManager _linearLayoutManager;
        private GridLayoutManager _gridLayoutManager;
        private GridItemDecoration _gridItemDecoration;

        private List<Button> _buttonsList;
        private const int AnimationDuration = 300;
        private const int MinFontSize = 14;
        private const int MaxFontSize = 20;
        private int _bottomPadding;
        private ValueAnimator _fontGrowingAnimation;
        private ValueAnimator _fontReductionAnimation;
        private ValueAnimator _grayToBlackAnimation;
        private ValueAnimator _blackToGrayAnimation;
        private Button _activeButton;
        private Button _currentButton;

        public string CustomTag
        {
            get => _presenter.Tag;
            set => _presenter.Tag = value;
        }

        FeedAdapter _profileFeedAdapter;
        FeedAdapter ProfileFeedAdapter
        {
            get
            {
                if (_profileFeedAdapter == null)
                {
                    _profileFeedAdapter = new FeedAdapter(Context, _presenter, new[] { _font, _semiboldFont });
                    _profileFeedAdapter.PhotoClick += OnPhotoClick;
                    _profileFeedAdapter.LikeAction += LikeAction;
                    _profileFeedAdapter.UserAction += UserAction;
                    _profileFeedAdapter.CommentAction += CommentAction;
                    _profileFeedAdapter.VotersClick += VotersAction;
                }
                return _profileFeedAdapter;
            }
        }

        GridAdapter _profileGridAdapter;
        GridAdapter ProfileGridAdapter
        {
            get
            {
                if (_profileGridAdapter == null)
                {
                    _profileGridAdapter = new GridAdapter(Context, _presenter);
                    _profileGridAdapter.Click += OnPhotoClick;
                }
                return _profileGridAdapter;
            }
        }

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

        [InjectOnClick(Resource.Id.clear_button)]
        public void OnClearClick(object sender, EventArgs e)
        {
            CustomTag = null;
            _clearButton.Visibility = ViewStates.Visible;
            _searchView.Text = "Tap to search";
            _searchView.SetTextColor(BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Activity, Resource.Color.rgb151_155_158)));
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

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_presearch, null);
                Cheeseknife.Inject(this, InflatedView);
            }
            return InflatedView;
        }

        public override async void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try //TODO:KOA: is try catch realy needed?
            {
                var s = Activity.Intent.GetStringExtra("SEARCH");
                if (s != null && s != CustomTag)
                {
                    Activity.Intent.RemoveExtra("SEARCH");
                    _searchView.Text = _presenter.Tag = CustomTag = s;
                    _searchView.SetTextColor(BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Activity, Resource.Color.rgb15_24_30)));
                    _clearButton.Visibility = ViewStates.Visible;
                    await LoadPosts(true);
                }
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }

            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);

            if (BasePresenter.User.IsAuthenticated)
                _loginButton.Visibility = ViewStates.Gone;

            _font = Typeface.CreateFromAsset(Android.App.Application.Context.Assets, "OpenSans-Regular.ttf");
            _semiboldFont = Typeface.CreateFromAsset(Android.App.Application.Context.Assets, "OpenSans-Semibold.ttf");

            SetAnimation();
            _buttonsList = new List<Button> { _newButton, _hotButton, _trendingButton };
            _bottomPadding = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 2, Resources.DisplayMetrics);
            _currentButton = _trendingButton;
            _trendingButton.Typeface = _semiboldFont;
            _hotButton.Typeface = _font;
            _newButton.Typeface = _font;

            _searchView.Typeface = _font;
            _clearButton.Typeface = _font;
            _loginButton.Typeface = _semiboldFont;
            _scrollListner = new ScrollListener();
            _scrollListner.ScrolledToBottom += async () => await LoadPosts();

            _linearLayoutManager = new LinearLayoutManager(Context);
            _gridLayoutManager = new GridLayoutManager(Context, 3);

            _gridItemDecoration = new GridItemDecoration();
            _searchList.SetLayoutManager(_gridLayoutManager);
            _searchList.AddItemDecoration(_gridItemDecoration);
            _searchList.AddOnScrollListener(_scrollListner);
            _searchList.SetAdapter(ProfileGridAdapter);

            _refresher.Refresh += async delegate
            {
                await LoadPosts(true);
            };
            await LoadPosts();
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

        private async void LikeAction(int position)
        {
            if (BasePresenter.User.IsAuthenticated)
            {
                var errors = await _presenter.TryVote(position);
                if (errors != null && errors.Count != 0)
                    ShowAlert(errors);

                _searchList?.GetAdapter()?.NotifyDataSetChanged();
            }
            else
                OpenLogin();

        }

        private void UserAction(int position)
        {
            var post = _presenter[position];
            if (post == null)
                return;
            if (BasePresenter.User.Login != post.Author)
                ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(post.Author));
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
            Activity.Intent.PutExtra("count", post.NetVotes);
            ((BaseActivity)Activity).OpenNewContentFragment(new VotersFragment());
        }

        private async Task LoadPosts(bool clearOld = false)
        {
            if (_spinner != null && !_refresher.Refreshing)
                _spinner.Visibility = ViewStates.Visible;

            if (clearOld)
            {
                _presenter.LoadCancel();
                _presenter.Clear();
                _scrollListner.ClearPosition();
                _searchList.ScrollToPosition(0);
            }

            List<string> errors;
            if (string.IsNullOrEmpty(CustomTag))
                errors = await _presenter.TryLoadNextTopPosts();
            else
                errors = await _presenter.TryGetSearchedPosts();

            if (errors == null)
                return;

            if (errors.Any())
                ShowAlert(errors);
            else
                _searchList?.GetAdapter()?.NotifyDataSetChanged();

            if (_refresher.Refreshing)
                _refresher.Refreshing = false;
            else if (_spinner != null)
                _spinner.Visibility = ViewStates.Gone;
        }

        protected override void CreatePresenter()
        {
            _presenter = new PreSearchPresenter();
        }

        private void OpenLogin()
        {
            var intent = new Intent(Activity, typeof(PreSignInActivity));
            StartActivity(intent);
        }

        private async Task SwitchSearchType(PostType postType)
        {
            if (postType == _presenter.PostType)
                return;
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
            _presenter.PostType = postType;
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

            _activeButton.Typeface = _semiboldFont;
            _currentButton.Typeface = _font;

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