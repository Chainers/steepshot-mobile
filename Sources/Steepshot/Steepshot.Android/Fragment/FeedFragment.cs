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
        private FeedScrollListner _scrollListner;

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

                _scrollListner = new FeedScrollListner();
                _scrollListner.ScrolledToBottom += ScrollListnerScrolledToBottom;

                Refresher.Refresh += OnRefresh;

                PostsList.SetAdapter(_adapter);
                PostsList.SetLayoutManager(new LinearLayoutManager(Android.App.Application.Context));
                PostsList.AddOnScrollListener(_scrollListner);

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
            get => base.UserVisibleHint;
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
