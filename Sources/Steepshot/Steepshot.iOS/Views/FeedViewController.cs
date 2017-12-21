using System;
using System.Threading.Tasks;
using Foundation;
using Steepshot.Core;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class FeedViewController : BaseViewControllerWithPresenter<FeedPresenter>
    {
        private ProfileCollectionViewSource _collectionViewSource;
        private CollectionViewFlowDelegate _gridDelegate;
        private UINavigationController _navController;
        private UINavigationItem _navItem;
        private UIRefreshControl _refreshControl;
        private bool _isFeedRefreshing;

        protected override void CreatePresenter()
        {
            _presenter = new FeedPresenter();
            _presenter.SourceChanged += SourceChanged;
        }

        private void SourceChanged(Status status)
        {
            feedCollection.ReloadData();
            flowLayout.InvalidateLayout();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _navController = TabBarController != null ? TabBarController.NavigationController : NavigationController;
            _navItem = NavigationItem;//TabBarController != null ? TabBarController.NavigationItem : NavigationItem;
            _collectionViewSource = new ProfileCollectionViewSource(_presenter);
            _collectionViewSource.IsGrid = false;
            _collectionViewSource.CellAction += CellAction;

            _gridDelegate = new CollectionViewFlowDelegate(feedCollection, _presenter);
            _gridDelegate.ScrolledToBottom += ScrolledToBottom;
            _gridDelegate.IsGrid = false;

            _refreshControl = new UIRefreshControl();
            _refreshControl.ValueChanged += OnRefresh;

            feedCollection.Source = _collectionViewSource;
            feedCollection.RegisterClassForCell(typeof(FeedCollectionViewCell), nameof(FeedCollectionViewCell));
            feedCollection.RegisterNibForCell(UINib.FromName(nameof(FeedCollectionViewCell), NSBundle.MainBundle), nameof(FeedCollectionViewCell));
            feedCollection.Add(_refreshControl);
            feedCollection.Delegate = _gridDelegate;
            //flowLayout.EstimatedItemSize = new CGSize(UIScreen.MainScreen.Bounds.Width, 485);

            if (TabBarController != null)
            {
                TabBarController.NavigationController.NavigationBar.TintColor = Helpers.Constants.R15G24B30;
                TabBarController.NavigationController.NavigationBar.BarTintColor = UIColor.White;
                TabBarController.NavigationController.SetNavigationBarHidden(true, false);
                //TabBarController.TabBar.TintColor = Helpers.Constants.NavBlue;

                foreach (var controler in TabBarController.ViewControllers)
                    controler.TabBarItem.ImageInsets = new UIEdgeInsets(5, 0, -5, 0);
            }

            SetNavBar();
            GetPosts();
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
                    var myViewController2 = new ImagePreviewViewController();
                    //TODO: pass image
                    myViewController2.ImageForPreview = null;
                    myViewController2.ImageUrl = post.Body;
                    _navController.PushViewController(myViewController2, true);
                    break;
                case ActionType.Voters:
                    var myViewController3 = new VotersViewController();
                    myViewController3.PostUrl = post.Url;
                    NavigationController.PushViewController(myViewController3, true);
                    break;
                case ActionType.Comments:
                    var myViewController4 = new CommentsViewController();
                    myViewController4.Post = post;
                    _navController.PushViewController(myViewController4, true);
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
            await GetPosts();
        }

        private async Task Vote(Post post)
        {
            var error = await _presenter.TryVote(post);
            ShowAlert(error);
        }

        private void Flag(Post post)
        {
            UIAlertController actionSheetAlert = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            actionSheetAlert.AddAction(UIAlertAction.Create(Localization.Messages.FlagPhoto, UIAlertActionStyle.Default, obj => FlagPhoto(post)));
            actionSheetAlert.AddAction(UIAlertAction.Create(Localization.Messages.HidePhoto, UIAlertActionStyle.Default, obj => HidePhoto(post)));
            actionSheetAlert.AddAction(UIAlertAction.Create(Localization.Messages.Cancel, UIAlertActionStyle.Cancel, null));
            PresentViewController(actionSheetAlert, true, null);
        }

        private void HidePhoto(Post post)
        {
            BasePresenter.User.PostBlackList.Add(post.Url);
            BasePresenter.User.Save();

            _presenter.RemovePost(post);
        }

        private async Task FlagPhoto(Post post)
        {
            if (post == null)
                return;

            var error = await _presenter.TryFlag(post);
            ShowAlert(error);
        }

        private void SetNavBar()
        {
            _navItem.LeftBarButtonItem = new UIBarButtonItem(new UIImageView(UIImage.FromBundle("ic_feed_logo")));
        }
    }
}
