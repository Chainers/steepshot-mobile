using System;
using System.Linq;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Autofac;
using CheeseBind;
using Steepshot.Activity;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Utils;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Enums;
using Steepshot.Interfaces;
using Steepshot.Core.Utils;
using Steepshot.CustomViews;

namespace Steepshot.Fragment
{
    public sealed class FeedFragment : BaseFragmentWithPresenter<FeedPresenter>, ICanOpenPost
    {
        public const string PostUrlExtraPath = "url";
        public const string PostNetVotesExtraPath = "count";

        private FeedAdapter<FeedPresenter> _adapter;
        private PostPagerAdapter<FeedPresenter> _postPagerAdapter;
        private FeedScrollListner _scrollListner;

#pragma warning disable 0649, 4014
        [BindView(Resource.Id.feed_list)] private RecyclerView _feedList;
        [BindView(Resource.Id.loading_spinner)] private ProgressBar _bar;
        [BindView(Resource.Id.feed_refresher)] private SwipeRefreshLayout _refresher;
        [BindView(Resource.Id.logo)] private ImageView _logo;
        [BindView(Resource.Id.app_bar)] private AppBarLayout _toolbar;
        [BindView(Resource.Id.empty_query_label)] private TextView _emptyQueryLabel;
        [BindView(Resource.Id.post_prev_pager)] private ViewPager _postPager;
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
                _scrollListner.ScrolledToBottom += LoadPosts;

                _refresher.Refresh += OnRefresh;

                _feedList.SetAdapter(_adapter);
                _feedList.SetLayoutManager(new LinearLayoutManager(Android.App.Application.Context));
                _feedList.AddOnScrollListener(_scrollListner);

                _postPager.SetClipToPadding(false);
                _postPager.SetPadding(Style.PostPagerMargin * 2, 0, Style.PostPagerMargin * 2, 0);
                _postPager.PageMargin = Style.PostPagerMargin;
                _postPager.PageScrollStateChanged += PostPagerOnPageScrollStateChanged;
                _postPager.PageScrolled += PostPagerOnPageScrolled;

                _postPagerAdapter = new PostPagerAdapter<FeedPresenter>(_postPager, Context, Presenter);
                _postPagerAdapter.PostAction += PostAction;
                _postPagerAdapter.AutoLinkAction += AutoLinkAction;
                _postPagerAdapter.CloseAction += CloseAction;

                _postPager.Adapter = _postPagerAdapter;
                _postPager.SetPageTransformer(false, _postPagerAdapter, (int)LayerType.None);

                _emptyQueryLabel.Typeface = Style.Light;
                _emptyQueryLabel.Text = App.Localization.GetText(LocalizationKeys.EmptyCategory);

                _mainMessage.Text = App.Localization.GetText(LocalizationKeys.Greeting);
                _hintMessage.Text = App.Localization.GetText(LocalizationKeys.EmptyFeedHint);
                _browseButton.Text = App.Localization.GetText(LocalizationKeys.GoToBrowse);

                LoadPosts();
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
            _adapter.NotifyDataSetChanged();
        }

        private void PostPagerOnPageScrolled(object sender, ViewPager.PageScrolledEventArgs pageScrolledEventArgs)
        {
            if (pageScrolledEventArgs.Position == Presenter.Count)
            {
                if (!Presenter.IsLastReaded)
                    LoadPosts();
                else
                    _postPagerAdapter.NotifyDataSetChanged();
            }
        }

        private void PostPagerOnPageScrollStateChanged(object sender, ViewPager.PageScrollStateChangedEventArgs pageScrollStateChangedEventArgs)
        {
            if (pageScrollStateChangedEventArgs.State == 0)
            {
                _feedList.ScrollToPosition(_postPager.CurrentItem);
                if (_feedList.GetLayoutManager() is GridLayoutManager manager)
                {
                    var positionToScroll = _postPager.CurrentItem + (_postPager.CurrentItem - manager.FindFirstVisibleItemPosition()) / 2;
                    _feedList.ScrollToPosition(positionToScroll < Presenter.Count
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
            _feedList.ScrollToPosition(0);
        }

        private void PresenterSourceChanged(Status status)
        {
            if (!IsInitialized)
                return;

            Activity.RunOnUiThread(() =>
            {
                _adapter.NotifyDataSetChanged();
                _postPagerAdapter.NotifyDataSetChanged();
            });
        }

        private void OnRefresh(object sender, EventArgs e)
        {
            _scrollListner.ClearPosition();
            Presenter.Clear();
            LoadPosts();
        }

        private async void LoadPosts()
        {
            var exception = await Presenter.TryLoadNextTopPostsAsync();
            if (!IsInitialized)
                return;

            Context.ShowAlert(exception, ToastLength.Short);

            _bar.Visibility = ViewStates.Gone;
            _refresher.Refreshing = false;

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

        public void OpenPost(Post post)
        {
            _postPager.SetCurrentItem(Presenter.IndexOf(post), false);
            _postPagerAdapter.NotifyDataSetChanged();
            _postPager.Visibility = ViewStates.Visible;
            _feedList.Visibility = ViewStates.Gone;
        }

        public bool ClosePost()
        {
            if (_postPager.Visibility == ViewStates.Visible)
            {
                _feedList.ScrollToPosition(_postPager.CurrentItem);
                _postPager.Visibility = ViewStates.Gone;
                _feedList.Visibility = ViewStates.Visible;
                _feedList.GetAdapter().NotifyDataSetChanged();
                return true;
            }
            return false;
        }

        private async void PostAction(ActionType type, Post post)
        {
            if (post == null)
                return;
            switch (type)
            {
                case ActionType.Like:
                    {
                        if (!App.User.HasPostingPermission)
                            return;
                        
                        var result = await Presenter.TryVoteAsync(post);
                        if (!IsInitialized)
                            return;

                        if (result.IsSuccess && Activity is RootActivity root)
                            root.TryUpdateProfile();

                        Context.ShowAlert(result);
                        break;
                    }
                case ActionType.VotersLikes:
                case ActionType.VotersFlags:
                    {
                        var isLikers = type == ActionType.VotersLikes;
                        Activity.Intent.PutExtra(PostUrlExtraPath, post.Url);
                        Activity.Intent.PutExtra(PostNetVotesExtraPath, isLikers ? post.NetLikes : post.NetFlags);
                        Activity.Intent.PutExtra(VotersFragment.VotersType, isLikers);
                        ((BaseActivity)Activity).OpenNewContentFragment(new VotersFragment());
                        break;
                    }
                case ActionType.Comments:
                    {
                        ((BaseActivity)Activity).OpenNewContentFragment(new CommentsFragment(post, post.Children == 0));
                        break;
                    }
                case ActionType.Profile:
                    {
                        ((BaseActivity)Activity).OpenNewContentFragment(new ProfileFragment(post.Author));
                        break;
                    }
                case ActionType.Flag:
                    {
                        if (!App.User.HasPostingPermission)
                            return;
                        
                        var result = await Presenter.TryFlagAsync(post);
                        if (!IsInitialized)
                            return;

                        if (result.IsSuccess && Activity is RootActivity root)
                            root.TryUpdateProfile();

                        Context.ShowAlert(result);
                        break;
                    }
                case ActionType.Hide:
                    {
                        Presenter.HidePost(post);
                        break;
                    }
                case ActionType.Delete:
                    {
                        var actionAlert = new ActionAlertDialog(Context,
                            App.Localization.GetText(LocalizationKeys.DeleteAlertTitle),
                            App.Localization.GetText(LocalizationKeys.DeleteAlertMessage),
                            App.Localization.GetText(LocalizationKeys.Delete),
                            App.Localization.GetText(LocalizationKeys.Cancel), AutoLinkAction);

                        actionAlert.AlertAction += async () =>
                        {
                            var result = await Presenter.TryDeletePostAsync(post);
                            if (!IsInitialized)
                                return;

                            Context.ShowAlert(result);
                        };

                        actionAlert.Show();
                        break;
                    }
                case ActionType.Share:
                    {
                        var shareIntent = new Intent(Intent.ActionSend);
                        shareIntent.SetType("text/plain");
                        shareIntent.PutExtra(Intent.ExtraSubject, post.Title);
                        shareIntent.PutExtra(Intent.ExtraText, string.Format(App.User.Chain == KnownChains.Steem ? Constants.SteemPostUrl : Constants.GolosPostUrl, post.Url));
                        StartActivity(Intent.CreateChooser(shareIntent, App.Localization.GetText(LocalizationKeys.Sharepost)));
                        break;
                    }
                case ActionType.Photo:
                    {
                        OpenPost(post);
                        break;
                    }
                case ActionType.Promote:
                    {
                        var actionAlert = new PromoteAlertDialog(Context, post, AutoLinkAction);
                        actionAlert.Window.RequestFeature(WindowFeatures.NoTitle);
                        actionAlert.Show();
                        break;
                    }
            }
        }

        public override void OnDetach()
        {
            _feedList.SetAdapter(null);
            base.OnDetach();
        }
        
        private void CloseAction()
        {
            ClosePost();
        }
    }
}
