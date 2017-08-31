using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;
using Steepshot.iOS.Cells;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class FeedViewController : BaseViewController
    {
        protected FeedViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public FeedViewController()
        {
        }

        private PostType _currentPostType = PostType.Top;
        private string _currentPostCategory;

        private ProfileCollectionViewSource _collectionViewSource = new ProfileCollectionViewSource();
        //private FeedTableViewSource tableSource = new FeedTableViewSource();
        private UIView _dropdown;
        private nfloat _dropDownListOffsetFromTop;
        private UILabel _tw;
        private UIImageView _arrow;
        private string _offsetUrl;

        UINavigationController _navController;
        UINavigationItem _navItem;

        private bool _hasItems = true;
        private bool _isHomeFeed;

        UIRefreshControl _refreshControl;

        private bool _isFeedRefreshing;

        private bool IsDropDownOpen => _dropdown.Frame.Y > 0;
        private CollectionViewFlowDelegate _gridDelegate;
        private int _lastRow;
        private const int Limit = 40;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _navController = TabBarController != null ? TabBarController.NavigationController : NavigationController;
            _navItem = NavigationItem;//TabBarController != null ? TabBarController.NavigationItem : NavigationItem;

            _gridDelegate = new CollectionViewFlowDelegate(scrolled: () =>
             {
                 try
                 {
                     var newlastRow = feedCollection.IndexPathsForVisibleItems.Max(c => c.Row) + 2;
                     if (_collectionViewSource.PhotoList.Count <= _lastRow && _hasItems && !_isFeedRefreshing)
                         GetPosts();
                     _lastRow = newlastRow;

                 }
                 catch (Exception ex) { }
             }, commentString: _collectionViewSource.FeedStrings);
            if (_navController != null)
                _navController.NavigationBar.Translucent = false;
            _collectionViewSource.IsGrid = false;
            _gridDelegate.IsGrid = false;
            feedCollection.Source = _collectionViewSource;
            feedCollection.RegisterClassForCell(typeof(FeedCollectionViewCell), nameof(FeedCollectionViewCell));
            feedCollection.RegisterNibForCell(UINib.FromName(nameof(FeedCollectionViewCell), NSBundle.MainBundle), nameof(FeedCollectionViewCell));
            //flowLayout.EstimatedItemSize = new CGSize(UIScreen.MainScreen.Bounds.Width, 485);

            _collectionViewSource.Voted += (vote, url, action) =>
            {
                Vote(vote, url, action);
            };
            _collectionViewSource.Flagged += Flagged;// (vote, url, action)  =>
                                                     //{
                                                     //Flagged(vote, url, action);
                                                     //};

            _refreshControl = new UIRefreshControl();
            _refreshControl.ValueChanged += async (sender, e) =>
            {
                if (_isFeedRefreshing)
                    return;
                _isFeedRefreshing = true;
                await RefreshTable();
                _refreshControl.EndRefreshing();
                _isFeedRefreshing = false;
            };
            feedCollection.Add(_refreshControl);
            feedCollection.Delegate = _gridDelegate;

            if (TabBarController != null)
            {
                TabBarController.NavigationController.NavigationBar.TintColor = UIColor.White;
                TabBarController.NavigationController.NavigationBar.BarTintColor = Constants.NavBlue;
                TabBarController.NavigationController.SetNavigationBarHidden(true, false);
                TabBarController.TabBar.TintColor = Constants.NavBlue;

                foreach (var controler in TabBarController.ViewControllers)
                {
                    controler.TabBarItem.ImageInsets = new UIEdgeInsets(5, 0, -5, 0);
                };
            }

            if (!IsHomeFeedLoaded)
            {
                _isHomeFeed = true;
                IsHomeFeedLoaded = true;
            }
            _collectionViewSource.GoToProfile += (username) =>
            {
                if (username == User.Login)
                    return;
                var myViewController = new ProfileViewController();
                myViewController.Username = username;
                NavigationController.PushViewController(myViewController, true);
            };

            _collectionViewSource.GoToComments += (postUrl) =>
            {
                var myViewController = new CommentsViewController();
                myViewController.PostUrl = postUrl;
                _navController.PushViewController(myViewController, true);
            };

            _collectionViewSource.GoToVoters += (postUrl) =>
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

            if (!_isHomeFeed)
            {
                _dropdown = CreateDropDownList();
            }
            SetNavBar();
            GetPosts();
        }

        public override void ViewWillAppear(bool animated)
        {
            //navController.SetNavigationBarHidden(false, true);
            /*if (TabBarController != null)
			{
				TabBarController.NavigationController.SetNavigationBarHidden(true, false);
				TabBarController.NavigationController.NavigationBar.TintColor = UIColor.White;
				TabBarController.NavigationController.NavigationBar.BarTintColor = Constants.NavBlue;
			}*/

            if (CurrentPostCategory != _currentPostCategory && !_isHomeFeed)
            {
                _currentPostCategory = CurrentPostCategory;
                _collectionViewSource.PhotoList.Clear();
                _collectionViewSource.FeedStrings.Clear();
                _tw.Text = CurrentPostCategory;
                GetPosts();
            }

            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            if (!_isHomeFeed && IsDropDownOpen)
                ToogleDropDownList();
            base.ViewWillDisappear(animated);
        }

        private async Task RefreshTable()
        {
            _collectionViewSource.PhotoList.Clear();
            _collectionViewSource.FeedStrings.Clear();
            _hasItems = true;
            await GetPosts(false);
        }

        void LoginTapped(object sender, EventArgs e)
        {
            _navController.PushViewController(new PreLoginViewController(), true);
        }

        void SearchTapped(object sender, EventArgs e)
        {
            var myViewController = new TagsSearchViewController();
            _navController.PushViewController(myViewController, true);
        }

        private void ToogleDropDownList()
        {
            if (_dropdown.Frame.Y < 0)
            {
                UIView.Animate(0.3, 0, UIViewAnimationOptions.CurveEaseIn,
                    () =>
                    {
                        _dropdown.Frame = new CGRect(_dropdown.Frame.X, 0, _dropdown.Frame.Width, _dropdown.Frame.Height);
                        _arrow.Transform = CGAffineTransform.MakeRotation((nfloat)(-180 * (Math.PI / 180)));
                    }, null);
            }
            else
            {
                UIView.Animate(0.2,
                    () =>
                    {
                        _dropdown.Frame = new CGRect(_dropdown.Frame.X, -_dropdown.Frame.Height, _dropdown.Frame.Width, _dropdown.Frame.Height);
                        _arrow.Transform = CGAffineTransform.MakeRotation((nfloat)(0 * (Math.PI / 180)));
                    }
                );
            }
        }

        public async Task GetPosts(bool shouldStartAnimating = true)
        {
            if (activityIndicator.IsAnimating)
                return;
            if (shouldStartAnimating)
                activityIndicator.StartAnimating();
            noFeedLabel.Hidden = true;
            try
            {
                OperationResult<UserPostResponse> posts;
                string offset = _collectionViewSource.PhotoList.Count == 0 ? "0" : _offsetUrl;

                if (!_isHomeFeed)
                {
                    if (CurrentPostCategory == null)
                    {
                        var postrequest = new PostsRequest(_currentPostType)
                        {
                            Login = User.Login,
                            Limit = Limit,
                            Offset = offset,
                            ShowNsfw = User.IsNsfw,
                            ShowLowRated = User.IsLowRated
                        };
                        posts = await Api.GetPosts(postrequest);
                    }
                    else
                    {
                        var postrequest = new PostsByCategoryRequest(_currentPostType, CurrentPostCategory)
                        {
                            Login = User.Login,
                            Limit = Limit,
                            Offset = offset,
                            ShowNsfw = User.IsNsfw,
                            ShowLowRated = User.IsLowRated
                        };
                        posts = await Api.GetPostsByCategory(postrequest);
                    }
                }
                else
                {
                    var f = new CensoredPostsRequests()
                    {
                        Login = User.Login,
                        Limit = Limit,
                        Offset = offset,
                        ShowNsfw = User.IsNsfw,
                        ShowLowRated = User.IsLowRated
                    };
                    posts = await Api.GetUserRecentPosts(f);
                }

                if (posts.Success)
                {
                    if (posts.Result == null || posts.Result.Results == null)
                    {
                        _hasItems = false;
                        _collectionViewSource.PhotoList.Clear();
                        _collectionViewSource.FeedStrings.Clear();
                        feedCollection.ReloadData();
                        feedCollection.CollectionViewLayout.InvalidateLayout();
                        return;
                    }

                    if (posts.Result.Results.Count == 0 && _isHomeFeed)
                        noFeedLabel.Hidden = false;

                    if (posts.Result.Results.Count != 0)
                    {
                        posts.Result.Results.FilterHided();
                        var lastItem = posts.Result.Results.Last();
                        _offsetUrl = lastItem.Url;

                        if (posts.Result.Results.Count < Limit / 2)
                            _hasItems = false;
                        else
                            posts.Result.Results.Remove(lastItem);

                        foreach (var r in posts.Result.Results)
                        {
                            var at = new NSMutableAttributedString();
                            at.Append(new NSAttributedString(r.Author, Constants.NicknameAttribute));
                            at.Append(new NSAttributedString($" {r.Title}"));
                            _collectionViewSource.FeedStrings.Add(at);
                        }

                        _collectionViewSource.PhotoList.AddRange(posts.Result.Results);
                    }
                    else
                        _hasItems = false;

                    feedCollection.ReloadData();
                    feedCollection.CollectionViewLayout.InvalidateLayout();
                }
                else
                {
                    ShowAlert(posts.Errors[0]);
                }

            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex, User.Login, "");
            }
            finally
            {
                activityIndicator.StopAnimating();
            }
        }

        private async Task Vote(bool vote, string postUrl, Action<string, OperationResult<VoteResponse>> action)
        {

            if (!User.IsAuthenticated)
            {
                LoginTapped(null, null);
                return;
            }
            try
            {
                var voteRequest = new VoteRequest(User.UserInfo, vote ? VoteType.Up : VoteType.Down, postUrl);
                var voteResponse = await Api.Vote(voteRequest);
                if (voteResponse.Success)
                {
                    var u = _collectionViewSource.PhotoList.First(p => p.Url == postUrl);
                    u.Vote = vote;
                    if (vote)
                    {
                        u.Flag = false;
                        if (u.NetVotes == -1)
                            u.NetVotes = 1;
                        else
                            u.NetVotes++;
                    }
                    else
                        u.NetVotes--;
                }
                else
                {
                    ShowAlert(voteResponse.Errors[0]);
                }

                action.Invoke(postUrl, voteResponse);
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex, User.Login, AppVersion);
            }
        }

        private void Flagged(bool vote, string postUrl, Action<string, OperationResult<VoteResponse>> action)
        {
            if (!User.IsAuthenticated)
            {
                LoginTapped(null, null);
                return;
            }
            UIAlertController actionSheetAlert = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            actionSheetAlert.AddAction(UIAlertAction.Create("Flag photo", UIAlertActionStyle.Default, (obj) => FlagPhoto(vote, postUrl, action)));
            actionSheetAlert.AddAction(UIAlertAction.Create("Hide photo", UIAlertActionStyle.Default, (obj) => HidePhoto(postUrl)));
            actionSheetAlert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, (obj) => action.Invoke(postUrl, new OperationResult<VoteResponse>())));
            PresentViewController(actionSheetAlert, true, null);
        }

        private void HidePhoto(string url)
        {
            try
            {
                User.PostBlacklist.Add(url);
                User.Save();
                var postToHide = _collectionViewSource.PhotoList.First(p => p.Url == url);
                var postIndex = _collectionViewSource.PhotoList.IndexOf(postToHide);
                _collectionViewSource.PhotoList.Remove(postToHide);
                _collectionViewSource.FeedStrings.RemoveAt(postIndex);
                feedCollection.ReloadData();
                flowLayout.InvalidateLayout();
            }
            catch (Exception ex)
            {

            }
        }

        private async Task FlagPhoto(bool vote, string postUrl, Action<string, OperationResult<VoteResponse>> action)
        {
            try
            {
                var flagRequest = new VoteRequest(User.UserInfo, vote ? VoteType.Flag : VoteType.Down, postUrl);
                var flagResponse = await Api.Vote(flagRequest);
                if (flagResponse.Success)
                {
                    var u = _collectionViewSource.PhotoList.First(p => p.Url == postUrl);
                    u.Flag = flagResponse.Result.IsSucces;
                    if (flagResponse.Result.IsSucces)
                    {
                        if (u.Vote)
                            if (u.NetVotes == 1)
                                u.NetVotes = -1;
                            else
                                u.NetVotes--;
                        u.Vote = false;
                    }
                }
                else
                {
                    ShowAlert(flagResponse.Errors[0]);
                }
                action.Invoke(postUrl, flagResponse);
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex, User.Login, AppVersion);
            }
        }

        private void SetNavBar()
        {
            var barHeight = NavigationController.NavigationBar.Frame.Height;
            UIView titleView = new UIView();

            _tw = new UILabel(new CGRect(0, 0, 120, barHeight));
            _tw.TextColor = UIColor.White;
            _tw.Text = "FEED"; //ToConstants name
            _tw.BackgroundColor = UIColor.Clear;
            _tw.TextAlignment = UITextAlignment.Center;
            _tw.Font = UIFont.SystemFontOfSize(17);
            titleView.Frame = new CGRect(0, 0, _tw.Frame.Right, barHeight);

            titleView.Add(_tw);
            if (!_isHomeFeed)
            {
                _tw.Text = "TRENDING"; // SET current post type
                UITapGestureRecognizer tapGesture = new UITapGestureRecognizer(ToogleDropDownList);
                titleView.AddGestureRecognizer(tapGesture);
                titleView.UserInteractionEnabled = true;

                var arrowSize = 15;
                _arrow = new UIImageView(new CGRect(_tw.Frame.Right, barHeight / 2 - arrowSize / 2, arrowSize, arrowSize));
                _arrow.Image = UIImage.FromBundle("white-arrow-down");
                titleView.Add(_arrow);
                titleView.Frame = new CGRect(0, 0, _arrow.Frame.Right, barHeight);

                var rightBarButton = new UIBarButtonItem(UIImage.FromBundle("search"), UIBarButtonItemStyle.Plain, SearchTapped);
                _navItem.SetRightBarButtonItem(rightBarButton, true);
            }

            _navItem.TitleView = titleView;
            if (!User.IsAuthenticated)
            {
                var leftBarButton = new UIBarButtonItem("Login", UIBarButtonItemStyle.Plain, LoginTapped); //ToConstants name
                _navItem.SetLeftBarButtonItem(leftBarButton, true);
            }
            else
                _navItem.SetLeftBarButtonItem(null, false);

            NavigationController.NavigationBar.TintColor = UIColor.White;
            NavigationController.NavigationBar.BarTintColor = Constants.NavBlue;
        }

        private UIView CreateDropDownList()
        {
            _dropDownListOffsetFromTop = _navController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
            var view = new UIView();
            view.BackgroundColor = UIColor.White;

            var buttonColor = UIColor.FromRGB(66, 165, 245); // To constants

            var newPhotosButton = new UIButton(new CGRect(0, 0, _navController.NavigationBar.Frame.Width, 50));
            newPhotosButton.SetTitle("NEW PHOTOS", UIControlState.Normal); //ToConstants name
            newPhotosButton.BackgroundColor = buttonColor;
            newPhotosButton.TouchDown += ((e, obj) =>
               {
                   if (_currentPostType == PostType.New && CurrentPostCategory != null)
                       return;
                   ToogleDropDownList();
                   _collectionViewSource.PhotoList.Clear();
                   _collectionViewSource.FeedStrings.Clear();
                   feedCollection.ReloadData();
                   _currentPostType = PostType.New;
                   _tw.Text = newPhotosButton.TitleLabel.Text;
                   CurrentPostCategory = _currentPostCategory = null;
                   GetPosts();
                   feedCollection.SetContentOffset(new CGPoint(0, 0), false);
               });

            var hotButton = new UIButton(new CGRect(0, newPhotosButton.Frame.Bottom + 1, _navController.NavigationBar.Frame.Width, 50));
            hotButton.SetTitle("HOT", UIControlState.Normal); //ToConstants name
            hotButton.BackgroundColor = buttonColor;

            hotButton.TouchDown += ((e, obj) =>
               {
                   if (_currentPostType == PostType.Hot && CurrentPostCategory != null)
                       return;
                   ToogleDropDownList();
                   _collectionViewSource.PhotoList.Clear();
                   _collectionViewSource.FeedStrings.Clear();
                   feedCollection.ReloadData();
                   _currentPostType = PostType.Hot;
                   _tw.Text = hotButton.TitleLabel.Text;
                   CurrentPostCategory = _currentPostCategory = null;
                   GetPosts();
                   feedCollection.SetContentOffset(new CGPoint(0, 0), false);
               });

            var trendingButton = new UIButton(new CGRect(0, hotButton.Frame.Bottom + 1, NavigationController.NavigationBar.Frame.Width, 50));
            trendingButton.SetTitle("TRENDING", UIControlState.Normal); //ToConstants name
            trendingButton.BackgroundColor = buttonColor;

            trendingButton.TouchDown += ((e, obj) =>
               {
                   if (_currentPostType == PostType.Top && CurrentPostCategory != null)
                       return;
                   ToogleDropDownList();
                   _collectionViewSource.PhotoList.Clear();
                   _collectionViewSource.FeedStrings.Clear();
                   feedCollection.ReloadData();
                   _currentPostType = PostType.Top;
                   _tw.Text = trendingButton.TitleLabel.Text;
                   CurrentPostCategory = _currentPostCategory = null;
                   GetPosts();
                   feedCollection.SetContentOffset(new CGPoint(0, 0), false);
               });

            view.Add(newPhotosButton);
            view.Add(hotButton);
            view.Add(trendingButton);
            view.Frame = new CGRect(0, -trendingButton.Frame.Bottom, _navController.NavigationBar.Frame.Width, trendingButton.Frame.Bottom);

            View.Add(view);
            return view;
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
