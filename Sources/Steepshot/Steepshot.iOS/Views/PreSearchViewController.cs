using System;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.Cells;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;
using static Steepshot.iOS.Helpers.DeviceHelper;

namespace Steepshot.iOS.Views
{
    public partial class PreSearchViewController : BasePostController<PreSearchPresenter>, IPageCloser
    {
        public string CurrentPostCategory;

        private FeedCollectionViewSource _collectionViewSource;
        private CollectionViewFlowDelegate _gridDelegate;
        private SliderCollectionViewFlowDelegate _sliderGridDelegate;

        private UINavigationController _navController;
        private UIRefreshControl _refreshControl;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _navController = TabBarController != null ? TabBarController.NavigationController : NavigationController;
            _navController.NavigationBar.Translucent = false;

            _gridDelegate = new CollectionViewFlowDelegate(collectionView, _presenter);
            _gridDelegate.IsGrid = true;
            _gridDelegate.ScrolledToBottom += ScrolledToBottom;
            _gridDelegate.CellClicked += CellAction;

            _collectionViewSource = new FeedCollectionViewSource(_presenter, _gridDelegate);
            _collectionViewSource.IsGrid = true;
            collectionView.Source = _collectionViewSource;
            collectionView.RegisterClassForCell(typeof(LoaderCollectionCell), nameof(LoaderCollectionCell));
            collectionView.RegisterClassForCell(typeof(PhotoCollectionViewCell), nameof(PhotoCollectionViewCell));
            collectionView.RegisterClassForCell(typeof(NewFeedCollectionViewCell), nameof(NewFeedCollectionViewCell));

            _sliderGridDelegate = new SliderCollectionViewFlowDelegate(sliderCollection, _presenter);
            _sliderGridDelegate.ScrolledToBottom += ScrolledToBottom;

            var _sliderCollectionViewSource = new SliderCollectionViewSource(_presenter, _sliderGridDelegate);

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

            _refreshControl = new UIRefreshControl();
            _refreshControl.ValueChanged += async (sender, e) =>
            {
                await GetPosts(false, true);
            };
            collectionView.Add(_refreshControl);

            _collectionViewSource.CellAction += CellAction;
            _collectionViewSource.TagAction += TagAction;

            _sliderCollectionViewSource.CellAction += CellAction;
            _sliderCollectionViewSource.TagAction += TagAction;

            collectionView.SetCollectionViewLayout(new UICollectionViewFlowLayout()
            {
                MinimumLineSpacing = 1,
                MinimumInteritemSpacing = 1,
            }, false);

            collectionView.Delegate = _gridDelegate;
            sliderCollection.Delegate = _sliderGridDelegate;

            SliderAction += (isOpening) =>
            {
                if (!sliderCollection.Hidden)
                    sliderCollection.ScrollEnabled = !isOpening;
            };

            if (!AppSettings.User.HasPostingPermission && CurrentPostCategory == null)
            {
                loginButton.Hidden = false;
                loginButton.Layer.CornerRadius = 25;
                loginButton.Layer.BorderWidth = 0;
            }

            loginButton.TouchDown += LoginTapped;

            hotButton.TouchDown += (object sender, EventArgs e) =>
            {
                SwitchSearchType(PostType.Hot);
            };

            topButton.TouchDown += (object sender, EventArgs e) =>
            {
                SwitchSearchType(PostType.Top);
            };

            newButton.TouchDown += (object sender, EventArgs e) =>
            {
                SwitchSearchType(PostType.New);
            };

            switcher.TouchDown += SwitchLayout;

            var searchTap = new UITapGestureRecognizer(SearchTapped);
            searchButton.AddGestureRecognizer(searchTap);

            GetPosts();
        }

        protected async override void LoginTapped(object sender, EventArgs e)
        {
            signInLoader.StartAnimating();
            loginButton.Enabled = false;

            var response = await _presenter.CheckServiceStatusAsync();

            loginButton.Enabled = true;
            signInLoader.StopAnimating();

            var myViewController = new WelcomeViewController(response.IsSuccess);
            NavigationController.PushViewController(myViewController, true);
        }

        public override void ViewWillDisappear(bool animated)
        {
            NavigationController.SetNavigationBarHidden(false, false);

            if (TabBarController != null)
                ((MainTabBarController)TabBarController).SameTabTapped -= SameTabTapped;

            base.ViewWillDisappear(animated);
        }

        public override void ViewWillAppear(bool animated)
        {
            if (!IsMovingToParentViewController)
                collectionView.ReloadData();
            if (CurrentPostCategory != null)
            {
                NavigationItem.Title = CurrentPostCategory;
                var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
                leftBarButton.TintColor = Constants.R15G24B30;
                NavigationItem.LeftBarButtonItem = leftBarButton;
                NavigationController.SetNavigationBarHidden(false, false);

                searchHeight.Constant = 0;
                searchTopMargin.Constant = 0;
                sliderCollectionOffset.Constant = 0;
            }
            else
            {
                if (GetVersion() == HardwareVersion.iPhoneX)
                    sliderCollectionOffset.Constant = 35;
                NavigationController.SetNavigationBarHidden(true, false);
            }

            if (TabBarController != null)
                ((MainTabBarController)TabBarController).SameTabTapped += SameTabTapped;

            base.ViewWillAppear(animated);
        }

        protected override void SameTabTapped()
        {
            if (NavigationController?.ViewControllers.Length == 1)
                collectionView.SetContentOffset(new CGPoint(0, 0), true);
        }

        private void SwitchSearchType(PostType postType)
        {
            if (postType == _presenter.PostType)
                return;
            _presenter.PostType = postType;
            switch (postType)
            {
                case PostType.Hot:
                    hotConstrain.Active = true;
                    topConstraint.Active = newConstraint.Active = false;
                    break;
                case PostType.New:
                    newConstraint.Active = true;
                    topConstraint.Active = hotConstrain.Active = false;
                    break;
                case PostType.Top:
                    topConstraint.Active = true;
                    hotConstrain.Active = newConstraint.Active = false;
                    break;
            }
            UIView.Animate(0.2, 0, UIViewAnimationOptions.CurveEaseOut, () =>
            {
                View.LayoutIfNeeded();
            }, null);
            GetPosts(true, true);
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
                    if (collectionView.Hidden)
                        //NavigationController.PushViewController(new PostViewController(post, _gridDelegate.Variables[_presenter.IndexOf(post)], _presenter), false);
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
            collectionView.Hidden = true;
            sliderCollection.Hidden = false;
            _sliderGridDelegate.GenerateVariables();
            sliderCollection.ReloadData();
            sliderCollection.ScrollToItem(NSIndexPath.FromRowSection(_presenter.IndexOf(post), 0), UICollectionViewScrollPosition.CenteredHorizontally, false);

            foreach (var item in collectionView.IndexPathsForVisibleItems)
            {
                if (collectionView.CellForItem(item) is NewFeedCollectionViewCell cell)
                    cell.Cell.Playback(false);
            }
        }

        public bool ClosePost()
        {
            foreach (var item in sliderCollection.IndexPathsForVisibleItems)
            {
                if (sliderCollection.CellForItem(item) is SliderFeedCollectionViewCell cell)
                    cell.Playback(false);
            }
            if (!sliderCollection.Hidden)
            {
                var visibleRect = new CGRect();
                visibleRect.Location = sliderCollection.ContentOffset;
                visibleRect.Size = sliderCollection.Bounds.Size;
                var visiblePoint = new CGPoint(visibleRect.GetMidX(), visibleRect.GetMidY());
                var index = sliderCollection.IndexPathForItemAtPoint(visiblePoint);

                collectionView.ScrollToItem(index, UICollectionViewScrollPosition.Top, false);
                collectionView.Hidden = false;
                sliderCollection.Hidden = true;
                _gridDelegate.GenerateVariables();
                collectionView.ReloadData();
                return true;
            }
            return false;
        }

        private void SwitchLayout(object sender, EventArgs e)
        {
            _gridDelegate.IsGrid = _collectionViewSource.IsGrid = !_collectionViewSource.IsGrid;
            switcher.Selected = _collectionViewSource.IsGrid;
            if (_collectionViewSource.IsGrid)
            {
                collectionView.SetCollectionViewLayout(new UICollectionViewFlowLayout()
                {
                    MinimumLineSpacing = 1,
                    MinimumInteritemSpacing = 1,
                }, false);

                foreach (var item in collectionView.IndexPathsForVisibleItems)
                {
                    if (collectionView.CellForItem(item) is NewFeedCollectionViewCell cell)
                        cell.Cell.Playback(false);
                }
            }
            else
            {
                collectionView.SetCollectionViewLayout(new UICollectionViewFlowLayout()
                {
                    MinimumLineSpacing = 0,
                    MinimumInteritemSpacing = 0,
                }, false);
            }

            collectionView.ReloadData();
            //collectionView.ScrollToItem(_gridDelegate.TopCurrentPosition, UICollectionViewScrollPosition.Top, false);
            collectionView.SetContentOffset(new CGPoint(0, 0), false);
        }

        protected override async Task GetPosts(bool shouldStartAnimating = true, bool clearOld = false)
        {
            Exception exception;
            do
            {
                if (shouldStartAnimating)
                {
                    activityIndicator.StartAnimating();
                    _refreshControl.EndRefreshing();
                }
                else
                    activityIndicator.StopAnimating();

                noFeedLabel.Hidden = true;

                if (clearOld)
                {
                    _sliderGridDelegate.ClearPosition();
                    _gridDelegate.ClearPosition();
                    _presenter.Clear();
                }

                if (CurrentPostCategory == null)
                    exception = await _presenter.TryLoadNextTopPostsAsync();
                else
                {
                    _presenter.Tag = CurrentPostCategory;
                    exception = await _presenter.TryGetSearchedPostsAsync();
                }

                if (exception is OperationCanceledException)
                    return;

                if (_refreshControl.Refreshing)
                {
                    _refreshControl.EndRefreshing();
                }
                else
                    activityIndicator.StopAnimating();
            } while (exception is RequestException);
            ShowAlert(exception);
        }

        private async Task RefreshTable()
        {
            await GetPosts(false, true);
        }

        void SearchTapped()
        {
            var myViewController = new TagsSearchViewController();
            NavigationController.PushViewController(myViewController, true);
        }

        protected override void SourceChanged(Status status)
        {
            InvokeOnMainThread(HandleAction);
        }

        void HandleAction()
        {
            if (!collectionView.Hidden)
            {
                foreach (var item in _presenter)
                {
                    foreach (var mediaModel in item.Media)
                    {
                        if (_gridDelegate.IsGrid)
                            ImageLoader.Preload(item.Media[0], Constants.CellSize.Width);
                        else
                            ImageLoader.Preload(mediaModel, Constants.ScreenWidth);
                    }
                }

                _gridDelegate.GenerateVariables();
                collectionView.ReloadData();
            }
            else
            {
                foreach (var item in _presenter)
                {
                    foreach (var mediaModel in item.Media)
                    {
                        if (_gridDelegate.IsGrid)
                            ImageLoader.Preload(item.Media[0], Constants.CellSize.Width);
                        else
                            ImageLoader.Preload(mediaModel, Constants.ScreenWidth);
                    }
                }

                foreach (var item in _presenter)
                {
                    ImageLoader.Preload(item.Media[0], Constants.ScreenWidth);
                }

                _sliderGridDelegate.GenerateVariables();
                sliderCollection.ReloadData();
            }
        }
    }
}
