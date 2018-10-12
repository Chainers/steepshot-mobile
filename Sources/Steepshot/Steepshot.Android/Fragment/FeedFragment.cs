using System;
using System.Threading.Tasks;
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
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public sealed class FeedFragment : BasePostsFragment<FeedPresenter>
    {
        public const string PostUrlExtraPath = "url";
        public const string PostNetVotesExtraPath = "count";

        private FeedAdapter<FeedPresenter> _adapter;
        private PostPagerAdapter<FeedPresenter> _postPagerAdapter;
        private ScrollListener _scrollListner;

#pragma warning disable 0649, 4014
        [BindView(Resource.Id.loading_spinner)] private ProgressBar _bar;
        [BindView(Resource.Id.logo)] private ImageView _logo;
        [BindView(Resource.Id.app_bar)] private AppBarLayout _toolbar;
        [BindView(Resource.Id.empty_query_label)] private TextView _emptyQueryLabel;
        [BindView(Resource.Id.feed_container)] private RelativeLayout _feedContainer;
        [BindView(Resource.Id.browse_button)] private Button _browseButton;
        [BindView(Resource.Id.main_message)] private TextView _mainMessage;
        [BindView(Resource.Id.hint_message)] private TextView _hintMessage;
#pragma warning restore 0649


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!IsInitialized)
            {
                InflatedView = inflater.Inflate(Resource.Layout.lyt_feed, null);
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

                Presenter.SourceChanged += PresenterSourceChanged;
                _adapter = new FeedAdapter<FeedPresenter>(Context, Presenter);
                _adapter.PostAction += PostAction;
                _adapter.AutoLinkAction += AutoLinkAction;

                _logo.Click += OnLogoClick;
                _browseButton.Click += GoToBrowseButtonClick;
                _toolbar.OffsetChanged += OnToolbarOffsetChanged;

                _scrollListner = new ScrollListener();
                _scrollListner.ScrolledToBottom += ScrollListnerScrolledToBottom;

                Refresher.Refresh += OnRefresh;

                PostsList.SetAdapter(_adapter);
                PostsList.SetLayoutManager(new LinearLayoutManager(Android.App.Application.Context));
                PostsList.AddOnScrollListener(_scrollListner);

                PostPager.SetClipToPadding(false);
                PostPager.SetPadding(Style.PostPagerMargin * 2, 0, Style.PostPagerMargin * 2, 0);
                PostPager.PageMargin = Style.PostPagerMargin;
                PostPager.PageScrollStateChanged += PostPagerOnPageScrollStateChanged;
                PostPager.PageScrolled += PostPagerOnPageScrolled;

                _postPagerAdapter = new PostPagerAdapter<FeedPresenter>(PostPager, Context, Presenter);
                _postPagerAdapter.PostAction += PostAction;
                _postPagerAdapter.AutoLinkAction += AutoLinkAction;
                _postPagerAdapter.CloseAction += CloseAction;

                PostPager.Adapter = _postPagerAdapter;
                PostPager.SetPageTransformer(false, _postPagerAdapter, (int)LayerType.None);

                _emptyQueryLabel.Typeface = Style.Light;
                _emptyQueryLabel.Text = App.Localization.GetText(LocalizationKeys.EmptyCategory);

                _mainMessage.Text = App.Localization.GetText(LocalizationKeys.Greeting);
                _hintMessage.Text = App.Localization.GetText(LocalizationKeys.EmptyFeedHint);
                _browseButton.Text = App.Localization.GetText(LocalizationKeys.GoToBrowse);

                if (UserVisibleHint)
                    UserVisibleHint = true;
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
                    GetPosts(false);
                }
                base.UserVisibleHint = value;
            }
        }

        public override void OnResume()
        {
            base.OnResume();
            _adapter.NotifyDataSetChanged();
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
                PostsList.ScrollToPosition(PostPager.CurrentItem);
                if (PostsList.GetLayoutManager() is GridLayoutManager manager)
                {
                    var positionToScroll = PostPager.CurrentItem + (PostPager.CurrentItem - manager.FindFirstVisibleItemPosition()) / 2;
                    PostsList.ScrollToPosition(positionToScroll < Presenter.Count
                        ? positionToScroll
                        : Presenter.Count);
                }
            }
        }

        private void OnToolbarOffsetChanged(object sender, AppBarLayout.OffsetChangedEventArgs e)
        {
            ViewCompat.SetElevation(_toolbar, BitmapUtils.DpToPixel(2, Resources));
        }

        private void OnLogoClick(object sender, EventArgs e)
        {
            PostsList.ScrollToPosition(0);
        }

        private void PresenterSourceChanged(Status status)
        {
            if (!IsInitialized)
                return;

            switch (status.Sender)
            {
                default:
                    {
                        _adapter.NotifyDataSetChanged();
                        _postPagerAdapter.NotifyDataSetChanged();

                        break;
                    }
            }
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            _scrollListner.ClearPosition();
            Presenter.Clear();
            GetPosts(false);
        }

        protected override async Task GetPosts(bool isRefresh)
        {
            var exception = await Presenter.TryLoadNextTopPostsAsync();
            if (!IsInitialized)
                return;

            Context.ShowAlert(exception, ToastLength.Short);

            _bar.Visibility = ViewStates.Gone;
            Refresher.Refreshing = false;

            _feedContainer.Visibility = ViewStates.Invisible;

            if (exception == null && Presenter.Count == 0)
            {
                _feedContainer.Visibility = ViewStates.Visible;
            }
        }

        private void GoToBrowseButtonClick(object sender, EventArgs e)
        {
            ((RootActivity)Activity).SelectTab(1);
        }
    }
}
