using System;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;
using CoreGraphics;
using Steepshot.Core.Exceptions;
using Steepshot.iOS.Delegates;

namespace Steepshot.iOS.Views
{
    public partial class FeedViewController : BasePostController<FeedPresenter>
    {
        private FeedCollectionViewSource _collectionViewSource;
        private UINavigationController _navController;
        private UIRefreshControl _refreshControl;
        private bool _isFeedRefreshing;

        protected override void SourceChanged(Status status)
        {
            if (status.Sender == nameof(Presenter.HidePost))
                StopPlayingVideo(sliderCollection, feedCollection);
            InvokeOnMainThread(HandleAction);
        }

        private void HandleAction()
        {
            if (!feedCollection.Hidden)
            {
                FeedCollectionViewDelegate.GenerateVariables();
                feedCollection.ReloadData();
            }
            else
            {
                SliderCollectionViewDelegate.GenerateVariables();
                sliderCollection.ReloadData();
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            if (!IsMovingToParentViewController)
                feedCollection.ReloadData();
            else
            {
                Presenter.SourceChanged += SourceChanged;
            }

            ((MainTabBarController)TabBarController).SameTabTapped += SameTabTapped;

            base.ViewWillAppear(animated);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _navController = TabBarController.NavigationController;

            FeedCollection = feedCollection;
            SliderCollection = sliderCollection;

            FeedCollectionViewDelegate = new CollectionViewFlowDelegate(feedCollection, Presenter);
            FeedCollectionViewDelegate.ScrolledToBottom += ScrolledToBottom;
            FeedCollectionViewDelegate.IsGrid = false;
            _collectionViewSource = new FeedCollectionViewSource(Presenter, FeedCollectionViewDelegate);
            _collectionViewSource.IsGrid = false;
            _collectionViewSource.CellAction += CellAction;
            _collectionViewSource.TagAction += TagAction;

            _refreshControl = new UIRefreshControl();
            _refreshControl.ValueChanged += OnRefresh;

            feedCollection.Source = _collectionViewSource;
            feedCollection.RegisterClassForCell(typeof(NewFeedCollectionViewCell), nameof(NewFeedCollectionViewCell));
            feedCollection.RegisterClassForCell(typeof(LoaderCollectionCell), nameof(LoaderCollectionCell));
            feedCollection.Add(_refreshControl);
            feedCollection.Delegate = FeedCollectionViewDelegate;
            feedCollection.DelaysContentTouches = false;

            SliderCollectionViewDelegate = new SliderCollectionViewFlowDelegate(sliderCollection, Presenter);
            SliderCollectionViewDelegate.ScrolledToBottom += ScrolledToBottom;

            SliderViewSource = new SliderCollectionViewSource(Presenter, SliderCollectionViewDelegate);

            sliderCollection.DecelerationRate = UIScrollView.DecelerationRateFast;
            sliderCollection.ShowsHorizontalScrollIndicator = false;

            sliderCollection.SetCollectionViewLayout(new SliderFlowLayout()
            {
                MinimumLineSpacing = 10,
                MinimumInteritemSpacing = 0,
                ScrollDirection = UICollectionViewScrollDirection.Horizontal,
                SectionInset = new UIEdgeInsets(0, 15, 0, 15),
            }, false);

            sliderCollection.Source = SliderViewSource;
            sliderCollection.RegisterClassForCell(typeof(LoaderCollectionCell), nameof(LoaderCollectionCell));
            sliderCollection.RegisterClassForCell(typeof(SliderFeedCollectionViewCell), nameof(SliderFeedCollectionViewCell));

            sliderCollection.DelaysContentTouches = false;

            SliderViewSource.CellAction += CellAction;
            SliderViewSource.TagAction += TagAction;
            sliderCollection.Delegate = SliderCollectionViewDelegate;

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

        public override void ViewDidDisappear(bool animated)
        {
            StopPlayingVideo(sliderCollection, feedCollection);

            if (IsMovingFromParentViewController)
            {
                Presenter.SourceChanged -= SourceChanged;
            }

            ((MainTabBarController)TabBarController).SameTabTapped -= SameTabTapped;

            base.ViewDidDisappear(animated);
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
                    if (post.Author == AppDelegate.User.Login)
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
                    NavigationController.PushViewController(new CommentsViewController(post) { HidesBottomBarWhenPushed = true }, true);
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
                    FeedCollectionViewDelegate.ClearPosition();
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
