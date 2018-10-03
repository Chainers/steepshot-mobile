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
using Steepshot.Core.Interfaces;
using UIKit;
using CoreGraphics;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Utils;

namespace Steepshot.iOS.Views
{
    public partial class FeedViewController : BasePostController<FeedPresenter>, IPageCloser
    {
        private FeedCollectionViewSource _collectionViewSource;
        private CollectionViewFlowDelegate _gridDelegate;
        private UINavigationController _navController;
        private UIRefreshControl _refreshControl;
        private bool _isFeedRefreshing;
        private SliderCollectionViewFlowDelegate _sliderGridDelegate;

        protected override void SourceChanged(Status status)
        {
            if (!feedCollection.Hidden)
            {
                _gridDelegate.GenerateVariables();
                feedCollection.ReloadData();
            }
            else
            {
                _sliderGridDelegate.GenerateVariables();
                sliderCollection.ReloadData();
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            if (!IsMovingToParentViewController)
                feedCollection.ReloadData();

            ((MainTabBarController)TabBarController).SameTabTapped += SameTabTapped;

            base.ViewWillAppear(animated);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _navController = TabBarController.NavigationController;

            _gridDelegate = new CollectionViewFlowDelegate(feedCollection, Presenter);
            _gridDelegate.ScrolledToBottom += ScrolledToBottom;
            _gridDelegate.IsGrid = false;
            _collectionViewSource = new FeedCollectionViewSource(Presenter, _gridDelegate);
            _collectionViewSource.IsGrid = false;
            _collectionViewSource.CellAction += CellAction;
            _collectionViewSource.TagAction += TagAction;

            _refreshControl = new UIRefreshControl();
            _refreshControl.ValueChanged += OnRefresh;

            feedCollection.Source = _collectionViewSource;
            feedCollection.RegisterClassForCell(typeof(NewFeedCollectionViewCell), nameof(NewFeedCollectionViewCell));
            feedCollection.RegisterClassForCell(typeof(LoaderCollectionCell), nameof(LoaderCollectionCell));
            feedCollection.Add(_refreshControl);
            feedCollection.Delegate = _gridDelegate;
            feedCollection.DelaysContentTouches = false;

            _sliderGridDelegate = new SliderCollectionViewFlowDelegate(sliderCollection, Presenter);
            _sliderGridDelegate.ScrolledToBottom += ScrolledToBottom;

            var _sliderCollectionViewSource = new SliderCollectionViewSource(Presenter, _sliderGridDelegate);

            sliderCollection.DecelerationRate = UIScrollView.DecelerationRateFast;
            sliderCollection.ShowsHorizontalScrollIndicator = false;

            sliderCollection.SetCollectionViewLayout(new SliderFlowLayout()
            {
                MinimumLineSpacing = 10,
                MinimumInteritemSpacing = 0,
                ScrollDirection = UICollectionViewScrollDirection.Horizontal,
                SectionInset = new UIEdgeInsets(0, 15, 0, 15),
            }, false);

            sliderCollection.Source = _sliderCollectionViewSource;
            sliderCollection.RegisterClassForCell(typeof(LoaderCollectionCell), nameof(LoaderCollectionCell));
            sliderCollection.RegisterClassForCell(typeof(SliderFeedCollectionViewCell), nameof(SliderFeedCollectionViewCell));

            sliderCollection.DelaysContentTouches = false;

            _sliderCollectionViewSource.CellAction += CellAction;
            _sliderCollectionViewSource.TagAction += TagAction;
            sliderCollection.Delegate = _sliderGridDelegate;

            SliderAction += (isOpening) =>
            {
                if (!sliderCollection.Hidden)
                    sliderCollection.ScrollEnabled = !isOpening;
            };

            if (TabBarController != null)
            {
                TabBarController.NavigationController.NavigationBar.TintColor = Constants.R15G24B30;
                TabBarController.NavigationController.NavigationBar.BarTintColor = UIColor.White;
                TabBarController.NavigationController.SetNavigationBarHidden(true, false);
            }

            ((MainTabBarController)TabBarController).SameTabTapped += SameTabTapped;

            SetNavBar();
            GetPosts();
        }

        public override void ViewWillDisappear(bool animated)
        {
            ((MainTabBarController) TabBarController).SameTabTapped -= SameTabTapped;

            base.ViewWillDisappear(animated);
        }

        protected override void SameTabTapped()
        {
            if (NavigationController?.ViewControllers.Length == 1)
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
                    if (post.Author == AppSettings.User.Login)
                        return;
                    var myViewController = new ProfileViewController();
                    myViewController.Username = post.Author;
                    NavigationController.PushViewController(myViewController, true);
                    break;
                case ActionType.Preview:
                    if (feedCollection.Hidden)
                        NavigationController.PushViewController(new ImagePreviewViewController(post.Media[post.PageIndex].Url) { HidesBottomBarWhenPushed = true }, true);
                    else
                        OpenPost(post);
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
                    Flagged(post);
                    break;
                case ActionType.Close:
                    ClosePost();
                    break;
                default:
                    break;
            }
        }

        public void OpenPost(Post post)
        {
            feedCollection.Hidden = true;
            sliderCollection.Hidden = false;
            _sliderGridDelegate.GenerateVariables();
            sliderCollection.ReloadData();
            sliderCollection.ScrollToItem(NSIndexPath.FromRowSection(Presenter.IndexOf(post), 0), UICollectionViewScrollPosition.CenteredHorizontally, false);
        }

        public bool ClosePost()
        {
            if (!sliderCollection.Hidden)
            {
                var visibleRect = new CGRect();
                visibleRect.Location = sliderCollection.ContentOffset;
                visibleRect.Size = sliderCollection.Bounds.Size;
                var visiblePoint = new CGPoint(visibleRect.GetMidX(), visibleRect.GetMidY());
                var index = sliderCollection.IndexPathForItemAtPoint(visiblePoint);

                feedCollection.ScrollToItem(index, UICollectionViewScrollPosition.Top, false);
                feedCollection.Hidden = false;
                sliderCollection.Hidden = true;
                _gridDelegate.GenerateVariables();
                feedCollection.ReloadData();
                return true;
            }
            return false;
        }

        protected override async Task GetPosts(bool shouldStartAnimating = true, bool clearOld = false)
        {
            Exception exception;
            do
            {
                if (shouldStartAnimating)
                    activityIndicator.StartAnimating();
                noFeedLabel.Hidden = true;

                if (clearOld)
                {
                    Presenter.Clear();
                    _gridDelegate.ClearPosition();
                }
                exception = await Presenter.TryLoadNextTopPostsAsync();

                if (_refreshControl.Refreshing)
                {
                    _refreshControl.EndRefreshing();
                    _isFeedRefreshing = false;
                }
                else
                    activityIndicator.StopAnimating();
            } while (exception is RequestException);
            ShowAlert(exception);
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
