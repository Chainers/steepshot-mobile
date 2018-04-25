using System;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using iOS.Hardware;
using Steepshot.Core.Errors;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class PreSearchViewController : BasePostController<PreSearchPresenter>
    {
        public string CurrentPostCategory;

        private ProfileCollectionViewSource _collectionViewSource;
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

            _collectionViewSource = new ProfileCollectionViewSource(_presenter, _gridDelegate);
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

            if (!BasePresenter.User.IsAuthenticated && CurrentPostCategory == null)
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
            if(TabBarController != null)
                ((MainTabBarController)TabBarController).SameTabTapped += SameTabTapped;

            GetPosts();
        }

        public override void ViewWillDisappear(bool animated)
        {
            NavigationController.SetNavigationBarHidden(false, false);
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
                if(DeviceModel.Model(DeviceHardware.HardwareModel) == "iPhone10,6")
                    sliderCollectionOffset.Constant = 35;
                NavigationController.SetNavigationBarHidden(true, false);
            }

            base.ViewWillAppear(animated);
        }

        protected override void CreatePresenter()
        {
            _presenter = new PreSearchPresenter();
            _presenter.SourceChanged += SourceChanged;
        }

        protected override void SameTabTapped()
        {
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
                    if (post.Author == BasePresenter.User.Login)
                        return;
                    var myViewController = new ProfileViewController();
                    myViewController.Username = post.Author;
                    NavigationController.PushViewController(myViewController, true);
                    break;
                case ActionType.Preview:
                    if(collectionView.Hidden)
                        //NavigationController.PushViewController(new PostViewController(post, _gridDelegate.Variables[_presenter.IndexOf(post)], _presenter), false);
                        NavigationController.PushViewController(new ImagePreviewViewController(post.Body) { HidesBottomBarWhenPushed = true }, true);
                    else
                    {
                        collectionView.Hidden = true;
                        sliderCollection.Hidden = false;
                        _sliderGridDelegate.GenerateVariables();
                        sliderCollection.ReloadData();
                        sliderCollection.ScrollToItem(NSIndexPath.FromRowSection(_presenter.IndexOf(post), 0), UICollectionViewScrollPosition.CenteredHorizontally, false);
                    }
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
                    collectionView.Hidden = false;
                    sliderCollection.Hidden = true;
                    _gridDelegate.GenerateVariables();
                    collectionView.ReloadData();
                    collectionView.ScrollToItem(NSIndexPath.FromRowSection(_presenter.IndexOf(post), 0), UICollectionViewScrollPosition.Top, false);
                    break;
                default:
                    break;
            }
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
            ErrorBase error;
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
                    error = await _presenter.TryLoadNextTopPosts();
                else
                {
                    _presenter.Tag = CurrentPostCategory;
                    error = await _presenter.TryGetSearchedPosts();
                }

                if (error is CanceledError)
                    return;

                if (_refreshControl.Refreshing)
                {
                    _refreshControl.EndRefreshing();
                }
                else
                    activityIndicator.StopAnimating();
            } while (error is RequestError);
            ShowAlert(error);
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
            if (!collectionView.Hidden)
            {
                _gridDelegate.GenerateVariables();
                collectionView.ReloadData();
            }
            else
            {
                _sliderGridDelegate.GenerateVariables();
                sliderCollection.ReloadData();
            }
        }
    }
}
