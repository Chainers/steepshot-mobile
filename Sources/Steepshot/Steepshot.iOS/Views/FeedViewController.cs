using System;
using System.Threading.Tasks;
using Foundation;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;
using Steepshot.Core.Utils;
using Steepshot.Core.Localization;
using CoreGraphics;

namespace Steepshot.iOS.Views
{
    public partial class FeedViewController : BaseViewControllerWithPresenter<FeedPresenter>
    {
        private ProfileCollectionViewSource _collectionViewSource;
        private CollectionViewFlowDelegate _gridDelegate;
        private UINavigationController _navController;
        private UIRefreshControl _refreshControl;
        private bool _isFeedRefreshing;

        protected override void CreatePresenter()
        {
            _presenter = new FeedPresenter();
            _presenter.SourceChanged += SourceChanged;
        }

        private void SourceChanged(Status status)
        {
            _gridDelegate.GenerateVariables();
            feedCollection.ReloadData();
        }

        public override void ViewWillAppear(bool animated)
        {
            if (!IsMovingToParentViewController)
                feedCollection.ReloadData();
            base.ViewWillAppear(animated);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _navController = TabBarController.NavigationController;

            _gridDelegate = new CollectionViewFlowDelegate(feedCollection, _presenter);
            _gridDelegate.ScrolledToBottom += ScrolledToBottom;
            _gridDelegate.IsGrid = false;
            _collectionViewSource = new ProfileCollectionViewSource(_presenter, _gridDelegate);
            _collectionViewSource.IsGrid = false;
            _collectionViewSource.CellAction += CellAction;
            _collectionViewSource.TagAction += TagAction;

            _refreshControl = new UIRefreshControl();
            _refreshControl.ValueChanged += OnRefresh;

            feedCollection.Source = _collectionViewSource;
            feedCollection.RegisterClassForCell(typeof(NewFeedCollectionViewCell), nameof(NewFeedCollectionViewCell));
            feedCollection.RegisterClassForCell(typeof(LoaderCollectionCell), nameof(LoaderCollectionCell));
            feedCollection.RegisterClassForCell(typeof(FeedCollectionViewCell), nameof(FeedCollectionViewCell));
            feedCollection.RegisterNibForCell(UINib.FromName(nameof(FeedCollectionViewCell), NSBundle.MainBundle), nameof(FeedCollectionViewCell));
            feedCollection.Add(_refreshControl);
            feedCollection.Delegate = _gridDelegate;

            if (TabBarController != null)
            {
                TabBarController.NavigationController.NavigationBar.TintColor = Helpers.Constants.R15G24B30;
                TabBarController.NavigationController.NavigationBar.BarTintColor = UIColor.White;
                TabBarController.NavigationController.SetNavigationBarHidden(true, false);
            }

            ((MainTabBarController)TabBarController).SameTabTapped += SameTabTapped;

            SetNavBar();
            GetPosts();
        }

        private void SameTabTapped()
        {
            feedCollection.SetContentOffset(new CGPoint(0, 0), true);
        }

        private async void OnRefresh(object sender, EventArgs e)
        {
            if (_isFeedRefreshing)
                return;
            _isFeedRefreshing = true;
            await GetPosts(false, true);
            _refreshControl.EndRefreshing();
        }

        private void CellAction(ActionType type, Post post)
        {
            switch (type)
            {
                case ActionType.Profile:
                    if (post.Author == BasePresenter.User.Login)
                        return;
                    var myViewController = new ProfileViewController();
                    myViewController.Username = post.Author;
                    NavigationController.PushViewController(myViewController, true);
                    break;
                case ActionType.Preview:
                    NavigationController.PushViewController(new ImagePreviewViewController(post.Body) { HidesBottomBarWhenPushed = true }, true);
                    break;
                case ActionType.Voters:
                    NavigationController.PushViewController(new VotersViewController(post, VotersType.Likes), true);
                    break;
                case ActionType.Flagers:
                    NavigationController.PushViewController(new VotersViewController(post, VotersType.Flags), true);
                    break;
                case ActionType.Comments:
                    var myViewController4 = new CommentsViewController();
                    myViewController4.Post = post;
                    myViewController4.HidesBottomBarWhenPushed = true;
                    NavigationController.PushViewController(myViewController4, true);
                    break;
                case ActionType.Like:
                    Vote(post);
                    break;
                case ActionType.More:
                    Flag(post);
                    break;
                default:
                    break;
            }
        }

        private void TagAction(string tag)
        {
            var myViewController = new PreSearchViewController();
            myViewController.CurrentPostCategory = tag;
            NavigationController.PushViewController(myViewController, true);
        }

        private async Task GetPosts(bool shouldStartAnimating = true, bool clearOld = false)
        {
            if (shouldStartAnimating)
                activityIndicator.StartAnimating();
            noFeedLabel.Hidden = true;

            if (clearOld)
            {
                _presenter.Clear();
                _gridDelegate.ClearPosition();
            }
            var error = await _presenter.TryLoadNextTopPosts();
            ShowAlert(error);

            if (_refreshControl.Refreshing)
            {
                _refreshControl.EndRefreshing();
                _isFeedRefreshing = false;
            }
            else
                activityIndicator.StopAnimating();
        }

        private async void ScrolledToBottom()
        {
            await GetPosts(false, false);
        }

        private async Task Vote(Post post)
        {
            var error = await _presenter.TryVote(post);
            ShowAlert(error);
        }

        private void Flag(Post post)
        {
            UIAlertController actionSheetAlert = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            actionSheetAlert.AddAction(UIAlertAction.Create(AppSettings.LocalizationManager.GetText(LocalizationKeys.FlagPhoto), UIAlertActionStyle.Default, obj => FlagPhoto(post)));
            actionSheetAlert.AddAction(UIAlertAction.Create(AppSettings.LocalizationManager.GetText(LocalizationKeys.HidePhoto), UIAlertActionStyle.Default, obj => HidePhoto(post)));
            actionSheetAlert.AddAction(UIAlertAction.Create(AppSettings.LocalizationManager.GetText(LocalizationKeys.CopyLink), UIAlertActionStyle.Default, obj => CopyLink(post)));
            actionSheetAlert.AddAction(UIAlertAction.Create(AppSettings.LocalizationManager.GetText(LocalizationKeys.Sharepost), UIAlertActionStyle.Default, obj => SharePhoto(post)));
            actionSheetAlert.AddAction(UIAlertAction.Create(AppSettings.LocalizationManager.GetText(LocalizationKeys.Cancel), UIAlertActionStyle.Cancel, null));
            PresentViewController(actionSheetAlert, true, null);
        }

        private void HidePhoto(Post post)
        {
            BasePresenter.User.PostBlackList.Add(post.Url);
            BasePresenter.User.Save();

            _presenter.HidePost(post);
        }

        private async Task FlagPhoto(Post post)
        {
            if (post == null)
                return;

            var error = await _presenter.TryFlag(post);
            ShowAlert(error);
        }

        private void CopyLink(Post post)
        {
            UIPasteboard clipboard = UIPasteboard.General;
            clipboard.String = AppSettings.LocalizationManager.GetText(LocalizationKeys.PostLink, post.Url);

            ShowAlert(LocalizationKeys.Copied);
        }

        private void SharePhoto(Post post)
        {
            var item = NSObject.FromObject("HI");
            var activityItems = new NSObject[] { item };
            UIActivity[] applicationActivities = null;

            var activityController = new UIActivityViewController(activityItems, applicationActivities);
            PresentViewController(activityController, true, null);
        }

        private void SetNavBar()
        {
            var logo = new UIImageView(UIImage.FromBundle("ic_feed_logo"));
            logo.UserInteractionEnabled = true;
            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(logo);

            UITapGestureRecognizer tap = new UITapGestureRecognizer(() =>
            {
            });
            logo.AddGestureRecognizer(tap);
        }
    }
}
