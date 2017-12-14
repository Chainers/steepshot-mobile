using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Steepshot.Core;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class FeedViewController : BaseViewControllerWithPresenter<PreSearchPresenter>
    {
        private PostType _currentPostType = PostType.Top;
        private string _currentPostCategory;

        private ProfileCollectionViewSource _collectionViewSource;
        private CollectionViewFlowDelegate _gridDelegate;
        private int _lastRow;

        private UIView _dropdown;
        private UILabel _tw;
        private UIImageView _arrow;
        private bool IsDropDownOpen => _dropdown.Frame.Y > 0;

        UINavigationController _navController;
        UINavigationItem _navItem;

        private readonly bool _isHomeFeed;

        UIRefreshControl _refreshControl;
        private bool _isFeedRefreshing;

        /*
        public FeedViewController(bool isFeed = false)
        {
            _isHomeFeed = isFeed;
        }*/

        protected override void CreatePresenter()
        {
            _presenter = new PreSearchPresenter();
            _presenter.SourceChanged += SourceChanged;
            // _presenter = new FeedPresenter(_isHomeFeed);
        }

        private void SourceChanged(Status status)
        {
            feedCollection.ReloadData();
            flowLayout.InvalidateLayout();
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            _navController = TabBarController != null ? TabBarController.NavigationController : NavigationController;
            _navItem = NavigationItem;//TabBarController != null ? TabBarController.NavigationItem : NavigationItem;
            _collectionViewSource = new ProfileCollectionViewSource(_presenter);
            //TODO:KOA: rewrite as function
            _gridDelegate = new CollectionViewFlowDelegate(scrolled: async () =>
             {
                 var newlastRow = feedCollection.IndexPathsForVisibleItems.Max(c => c.Row) + 2;
                 if (_presenter.Count <= _lastRow && !_presenter.IsLastReaded && !_isFeedRefreshing)
                     await GetPosts();

                 _lastRow = newlastRow;
             }, commentString: _collectionViewSource.FeedStrings, presenter: _presenter);

            if (_navController != null)
                _navController.NavigationBar.Translucent = false;
            _collectionViewSource.IsGrid = false;
            _gridDelegate.IsGrid = false;
            feedCollection.Source = _collectionViewSource;
            feedCollection.RegisterClassForCell(typeof(FeedCollectionViewCell), nameof(FeedCollectionViewCell));
            feedCollection.RegisterNibForCell(UINib.FromName(nameof(FeedCollectionViewCell), NSBundle.MainBundle), nameof(FeedCollectionViewCell));
            //flowLayout.EstimatedItemSize = new CGSize(UIScreen.MainScreen.Bounds.Width, 485);

            _collectionViewSource.Voted += async (vote, post, action) =>
            {
                await Vote(post);
            };
            _collectionViewSource.Flagged += Flagged;

            _refreshControl = new UIRefreshControl();
            _refreshControl.ValueChanged += async (sender, e) =>
            {
                if (_isFeedRefreshing)
                    return;
                _isFeedRefreshing = true;
                await RefreshTable();
                _refreshControl.EndRefreshing();
            };
            feedCollection.Add(_refreshControl);
            feedCollection.Delegate = _gridDelegate;

            if (TabBarController != null)
            {
                TabBarController.NavigationController.NavigationBar.TintColor = UIColor.White;
                TabBarController.NavigationController.NavigationBar.BarTintColor = Helpers.Constants.NavBlue;
                TabBarController.NavigationController.SetNavigationBarHidden(true, false);
                TabBarController.TabBar.TintColor = Helpers.Constants.NavBlue;

                foreach (var controler in TabBarController.ViewControllers)
                {
                    controler.TabBarItem.ImageInsets = new UIEdgeInsets(5, 0, -5, 0);
                }
            }

            _collectionViewSource.GoToProfile += username =>
            {
                if (username == BasePresenter.User.Login)
                    return;
                var myViewController = new ProfileViewController();
                myViewController.Username = username;
                NavigationController.PushViewController(myViewController, true);
            };

            _collectionViewSource.GoToComments += postUrl =>
            {
                var myViewController = new CommentsViewController();
                myViewController.PostUrl = postUrl;
                _navController.PushViewController(myViewController, true);
            };

            _collectionViewSource.GoToVoters += postUrl =>
            {
                var myViewController = new VotersViewController();
                myViewController.PostUrl = postUrl;
                NavigationController.PushViewController(myViewController, true);
            };

            _collectionViewSource.ImagePreview += (image, url) =>
            {
                var myViewController = new ImagePreviewViewController();
                myViewController.ImageForPreview = image;
                myViewController.ImageUrl = url;
                _navController.PushViewController(myViewController, true);
            };

            //SetNavBar();
            await GetPosts();
        }

        public override async void ViewWillAppear(bool animated)
        {
            if (CurrentPostCategory != _currentPostCategory && !_isHomeFeed)
            {
                _currentPostCategory = CurrentPostCategory;
                _presenter.Clear();
                _collectionViewSource.FeedStrings.Clear();
                _tw.Text = CurrentPostCategory;
                await GetPosts();
            }

            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
        }

        private async Task RefreshTable()
        {
            _collectionViewSource.FeedStrings.Clear();
            _presenter.Clear();
            await GetPosts(false, true);
        }

        void LoginTapped(object sender, EventArgs e)
        {
            _navController.PushViewController(new PreLoginViewController(), true);
        }

        private async Task GetPosts(bool shouldStartAnimating = true, bool clearOld = false)
        {
            if (activityIndicator.IsAnimating)
                return;
            if (shouldStartAnimating)
                activityIndicator.StartAnimating();
            noFeedLabel.Hidden = true;

            List<string> errors;
            if (CurrentPostCategory == null)
            {
                if (clearOld)
                    _presenter.Clear();
                errors = await _presenter.TryLoadNextTopPosts();
            }
            else
            {
                _presenter.Tag = CurrentPostCategory;
                errors = await _presenter.TryGetSearchedPosts();
            }
            if (errors != null && errors.Count != 0)
                ShowAlert(errors);

            //TODO:KOA: ... doesn't look good
            for (var i = _collectionViewSource.FeedStrings.Count; i < _presenter.Count; i++)
            {
                var post = _presenter[i];
                if (post != null)
                {
                    var at = new NSMutableAttributedString();
                    //at.Append(new NSAttributedString(post.Author, Helpers.Constants.NicknameAttribute));
                    at.Append(new NSAttributedString($" {post.Title}"));
                    _collectionViewSource.FeedStrings.Add(at);
                }
            }

            feedCollection.ReloadData();
            feedCollection.CollectionViewLayout.InvalidateLayout();
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

        private async Task Vote(Post post)
        {
            if (!BasePresenter.User.IsAuthenticated)
            {
                LoginTapped(null, null);
                return;
            }

            var errors = await _presenter.TryVote(post);
            ShowAlert(errors);


            //feedCollection.ReloadData();
            //flowLayout.InvalidateLayout();

        }

        private void Flagged(bool vote, Post post, Action<Post, OperationResult<VoteResponse>> action)
        {
            if (!BasePresenter.User.IsAuthenticated)
            {
                LoginTapped(null, null);
                return;
            }
            UIAlertController actionSheetAlert = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            actionSheetAlert.AddAction(UIAlertAction.Create(Localization.Messages.FlagPhoto, UIAlertActionStyle.Default, obj => FlagPhoto(post)));
            actionSheetAlert.AddAction(UIAlertAction.Create(Localization.Messages.HidePhoto, UIAlertActionStyle.Default, obj => HidePhoto(post)));
            actionSheetAlert.AddAction(UIAlertAction.Create(Localization.Messages.Cancel, UIAlertActionStyle.Cancel, obj => action.Invoke(post, new OperationResult<VoteResponse>())));
            PresentViewController(actionSheetAlert, true, null);
        }

        private void HidePhoto(Post post)
        {
            BasePresenter.User.PostBlackList.Add(post.Url);
            BasePresenter.User.Save();

            _presenter.RemovePost(post);
            //_collectionViewSource.FeedStrings.RemoveAt(postIndex);
        }

        private async Task FlagPhoto(Post post)
        {
            if (post == null)
                return;

            var errors = await _presenter.TryFlag(post);
            ShowAlert(errors);
        }

        private void SetNavBar()
        {
            var barHeight = NavigationController.NavigationBar.Frame.Height;
            UIView titleView = new UIView();

            _tw = new UILabel(new CGRect(0, 0, 120, barHeight));
            _tw.TextColor = UIColor.White;
            _tw.Text = Localization.Messages.Feed;
            _tw.BackgroundColor = UIColor.Clear;
            _tw.TextAlignment = UITextAlignment.Center;
            _tw.Font = UIFont.SystemFontOfSize(17);
            titleView.Frame = new CGRect(0, 0, _tw.Frame.Right, barHeight);

            titleView.Add(_tw);
            if (!_isHomeFeed)
            {
                _tw.Text = Localization.Messages.Trending; // SET current post type
                //UITapGestureRecognizer tapGesture = new UITapGestureRecognizer(ToogleDropDownList);
                //titleView.AddGestureRecognizer(tapGesture);
                titleView.UserInteractionEnabled = true;

                var arrowSize = 15;
                _arrow = new UIImageView(new CGRect(_tw.Frame.Right, barHeight / 2 - arrowSize / 2, arrowSize, arrowSize));
                _arrow.Image = UIImage.FromBundle("white-arrow-down");
                titleView.Add(_arrow);
                titleView.Frame = new CGRect(0, 0, _arrow.Frame.Right, barHeight);

                //var rightBarButton = new UIBarButtonItem(UIImage.FromBundle("search"), UIBarButtonItemStyle.Plain, SearchTapped);
                //_navItem.SetRightBarButtonItem(rightBarButton, true);
            }

            _navItem.TitleView = titleView;
            if (!BasePresenter.User.IsAuthenticated)
            {
                var leftBarButton = new UIBarButtonItem(Localization.Messages.Login, UIBarButtonItemStyle.Plain, LoginTapped); //ToConstants name
                _navItem.SetLeftBarButtonItem(leftBarButton, true);
            }
            else
                _navItem.SetLeftBarButtonItem(null, false);

            NavigationController.NavigationBar.TintColor = UIColor.White;
            NavigationController.NavigationBar.BarTintColor = Helpers.Constants.NavBlue;
        }
    }

    public class LilLayout : UICollectionViewFlowLayout
    {
        public override CGPoint TargetContentOffsetForProposedContentOffset(CGPoint proposedContentOffset)
        {
            return new CGPoint();
        }

        public override CGPoint TargetContentOffset(CGPoint proposedContentOffset, CGPoint scrollingVelocity)
        {
            var bob = base.TargetContentOffset(proposedContentOffset, scrollingVelocity);
            return bob;
        }

        public override UICollectionViewLayoutAttributes[] LayoutAttributesForElementsInRect(CGRect rect)
        {
            var allAttributes = base.LayoutAttributesForElementsInRect(rect).ToList();
            var f = new List<UICollectionViewLayoutAttributes>();
            foreach (UICollectionViewLayoutAttributes att in allAttributes)
            {
                if (att.RepresentedElementCategory == UICollectionElementCategory.Cell)
                {
                    f.Add(LayoutAttributesForItem(att.IndexPath));
                }
                //f.Add(att);
            }
            return f.ToArray();
            /*return allAttributes.FlatMap { attributes in
				switch attributes.representedElementCategory {
				case .Cell: return LayoutAttributesForItem(attributes.indexPath)
				default: return attributes
				}*/
        }

        public override UICollectionViewLayoutAttributes LayoutAttributesForItem(NSIndexPath indexPath)
        {
            var attributes = base.LayoutAttributesForItem(indexPath);

            //guard let collectionView = collectionView else { return attributes }
            attributes.Bounds = new RectangleF((PointF)attributes.Bounds.Location, new SizeF((float)(CollectionView.Bounds.Width - SectionInset.Left - SectionInset.Right), (float)attributes.Bounds.Size.Height));
            return attributes;
        }
    }


    /*public override CGSize CollectionViewContentSize
    {
        get
        {
            return new CGSize(320,0);
        }
    }*/

}
