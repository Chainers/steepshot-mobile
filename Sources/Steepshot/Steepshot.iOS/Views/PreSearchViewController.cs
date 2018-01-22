using System;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Steepshot.Core.Errors;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.Cells;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class PreSearchViewController : BaseViewControllerWithPresenter<PreSearchPresenter>
    {
        private PostType _currentPostType = PostType.Hot;
        private string _currentPostCategory;

        private ProfileCollectionViewSource _collectionViewSource;
        private CollectionViewFlowDelegate _gridDelegate;

        private UINavigationController _navController;
        private UINavigationItem _navItem;
        private UIRefreshControl _refreshControl;
        private bool _isFeedRefreshing;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _navController = TabBarController != null ? TabBarController.NavigationController : NavigationController;
            _collectionViewSource = new ProfileCollectionViewSource(_presenter);

            _gridDelegate = new CollectionViewFlowDelegate(collectionView, _presenter);
            _gridDelegate.IsGrid = false;
            _gridDelegate.ScrolledToBottom += ScrolledToBottom;

            if (_navController != null)
                _navController.NavigationBar.Translucent = false;

            _collectionViewSource.IsGrid = false;
            collectionView.Source = _collectionViewSource;
            collectionView.RegisterClassForCell(typeof(LoaderCollectionCell), nameof(LoaderCollectionCell));
            collectionView.RegisterClassForCell(typeof(PhotoCollectionViewCell), nameof(PhotoCollectionViewCell));
            collectionView.RegisterNibForCell(UINib.FromName(nameof(PhotoCollectionViewCell), NSBundle.MainBundle), nameof(PhotoCollectionViewCell));
            collectionView.RegisterClassForCell(typeof(FeedCollectionViewCell), nameof(FeedCollectionViewCell));
            collectionView.RegisterNibForCell(UINib.FromName(nameof(FeedCollectionViewCell), NSBundle.MainBundle), nameof(FeedCollectionViewCell));

            _refreshControl = new UIRefreshControl();
            _refreshControl.ValueChanged += async (sender, e) =>
            {
                if (_isFeedRefreshing)
                    return;
                _isFeedRefreshing = true;
                await RefreshTable();
                _refreshControl.EndRefreshing();
            };
            collectionView.Add(_refreshControl);

            _collectionViewSource.CellAction += CellAction;

            collectionView.SetCollectionViewLayout(new UICollectionViewFlowLayout()
            {
                EstimatedItemSize = new CGSize(UIScreen.MainScreen.Bounds.Width, 1),
                MinimumLineSpacing = 1,
                MinimumInteritemSpacing = 1,
            }, false);

            collectionView.Delegate = _gridDelegate;

            if (!BasePresenter.User.IsAuthenticated)
            {
                loginButton.Hidden = false;
                loginButton.Layer.CornerRadius = 20;
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

            var searchTap = new UITapGestureRecognizer(SearchTapped);
            searchButton.AddGestureRecognizer(searchTap);

            GetPosts();
        }

        public override void ViewWillDisappear(bool animated)
        {
            NavigationController.SetNavigationBarHidden(false, false);
            base.ViewWillDisappear(animated);
        }


        public override void ViewWillAppear(bool animated)
        {
            NavigationController.SetNavigationBarHidden(true, false);
            /*
            if (CurrentPostCategory != _currentPostCategory && !_isHomeFeed)
            {
                _currentPostCategory = CurrentPostCategory;
                _presenter.Clear();
                _collectionViewSource.FeedStrings.Clear();
                _tw.Text = CurrentPostCategory;
                await GetPosts();
            }*/

            base.ViewWillAppear(animated);
        }

        protected override void CreatePresenter()
        {
            _presenter = new PreSearchPresenter();
            _presenter.SourceChanged += SourceChanged;
        }

        private async void SwitchSearchType(PostType postType)
        {
            if (postType == _presenter.PostType)
                return;
            _presenter.PostType = postType;
            await GetPosts(true, true);
        }

        private async void ScrolledToBottom()
        {
            await GetPosts();
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
                    NavigationController.PushViewController(new VotersViewController(post, VotersType.Likes), true);
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
                    Flagged(post);
                    break;
                default:
                    break;
            }
        }

        private async Task Vote(Post post)
        {
            if (!BasePresenter.User.IsAuthenticated)
            {
                LoginTapped(null, null);
                return;
            }

            if (post == null)
                return;

            var error = await _presenter.TryVote(post);
            if (error is TaskCanceledError)
                return;

            ShowAlert(error);
        }

        private void Flagged(Post post)
        {
            if (!BasePresenter.User.IsAuthenticated)
            {
                LoginTapped(null, null);
                return;
            }
            UIAlertController actionSheetAlert = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            actionSheetAlert.AddAction(UIAlertAction.Create("Flag photo", UIAlertActionStyle.Default, (obj) => FlagPhoto(post)));
            actionSheetAlert.AddAction(UIAlertAction.Create("Hide photo", UIAlertActionStyle.Default, (obj) => HidePhoto(post)));
            actionSheetAlert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
            PresentViewController(actionSheetAlert, true, null);
        }

        private void HidePhoto(Post post)
        {
            try
            {
                if (post == null || BasePresenter.User.PostBlackList.Contains(post.Url))
                    return;

                BasePresenter.User.PostBlackList.Add(post.Url);
                BasePresenter.User.Save();

                _presenter.RemovePost(post);
                collectionView.ReloadData();
                collectionView.CollectionViewLayout.InvalidateLayout();
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
        }

        private async Task FlagPhoto(Post post)
        {
            if (post == null)
                return;

            var error = await _presenter.TryFlag(post);
            ShowAlert(error);
        }

        private async Task GetPosts(bool shouldStartAnimating = true, bool clearOld = false)
        {
            if (shouldStartAnimating)
                activityIndicator.StartAnimating();
            noFeedLabel.Hidden = true;

            if (clearOld)
            {
                _gridDelegate.ClearPosition();
                _presenter.Clear();
            }

            ErrorBase error;
            if (CurrentPostCategory == null)
            {
                error = await _presenter.TryLoadNextTopPosts();
            }
            else
            {
                _presenter.Tag = CurrentPostCategory;
                error = await _presenter.TryGetSearchedPosts();
            }

            ShowAlert(error);

            if (_refreshControl.Refreshing)
            {
                _refreshControl.EndRefreshing();
                _isFeedRefreshing = false;
            }
            else
            {
                activityIndicator.StopAnimating();
            }
        }

        private async Task RefreshTable()
        {
            _presenter.Clear();
            await GetPosts(false, true);
        }

        void LoginTapped(object sender, EventArgs e)
        {
            _navController.PushViewController(new WelcomeViewController(), true);
        }

        void SearchTapped()
        {
            var myViewController = new TagsSearchViewController();
            _navController.PushViewController(myViewController, true);
        }

        private void SwitchButtonClick()
        {

        }

        private void SourceChanged(Status status)
        {
            var offset = collectionView.ContentOffset;
            collectionView.ReloadData();
            collectionView.LayoutIfNeeded();
            collectionView.SetContentOffset(offset, false);
        }
    }
}
