using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;
using UIKit;

namespace Steepshot.iOS
{
	public partial class FeedViewController : BaseViewController
	{
		protected FeedViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		private PostType currentPostType = PostType.Top;
		private string currentPostCategory;

		private ProfileCollectionViewSource collectionViewSource = new ProfileCollectionViewSource();
		//private FeedTableViewSource tableSource = new FeedTableViewSource();
		private UIView dropdown;
		private nfloat dropDownListOffsetFromTop;
		private UILabel tw;
		private UIImageView arrow;
		private string _offsetUrl;

		UINavigationController navController;
		UINavigationItem navItem;

		private bool _hasItems = true;
		private bool isHomeFeed;

		UIRefreshControl RefreshControl;

		private bool _isFeedRefreshing = false;

		private bool IsDropDownOpen => dropdown.Frame.Y > 0;
		private CollectionViewFlowDelegate gridDelegate;
		private int _lastRow;
		private const int limit = 40;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			navController = NavigationController; //TabBarController != null ? TabBarController.NavigationController : NavigationController;
			navItem = NavigationItem;//TabBarController != null ? TabBarController.NavigationItem : NavigationItem;

			gridDelegate = new CollectionViewFlowDelegate(scrolled: () =>
			 {
				 try
				 {
					 var newlastRow = feedCollection.IndexPathsForVisibleItems.Max(c => c.Row) + 2;
					 if (collectionViewSource.PhotoList.Count <= _lastRow && _hasItems && !_isFeedRefreshing)
						 GetPosts();
					 _lastRow = newlastRow;
			 
				 }
				 catch (Exception ex) { }
			}, commentString: collectionViewSource.FeedStrings);
			collectionViewSource.IsGrid = false;
			gridDelegate.isGrid = false;
			feedCollection.Source = collectionViewSource;
			feedCollection.RegisterClassForCell(typeof(FeedCollectionViewCell), nameof(FeedCollectionViewCell));
			feedCollection.RegisterNibForCell(UINib.FromName(nameof(FeedCollectionViewCell), NSBundle.MainBundle), nameof(FeedCollectionViewCell));
			//flowLayout.EstimatedItemSize = new CGSize(UIScreen.MainScreen.Bounds.Width, 485);
            
            collectionViewSource.Voted += (vote, url, action)  =>
            {
                Vote(vote, url, action);
            };
			collectionViewSource.Flagged += Flagged;// (vote, url, action)  =>
            //{
                //Flagged(vote, url, action);
            //};

			RefreshControl = new UIRefreshControl();
			RefreshControl.ValueChanged += async (sender, e) =>
			{
				if (_isFeedRefreshing)
					return;
				_isFeedRefreshing = true;
				await RefreshTable();
				RefreshControl.EndRefreshing();
				_isFeedRefreshing = false;
			};
			feedCollection.Add(RefreshControl);
			feedCollection.Delegate = gridDelegate;

			if (TabBarController != null)
			{
				TabBarController.TabBar.TintColor = Constants.NavBlue;
				foreach(var controler in TabBarController.ViewControllers)
				{
					controler.TabBarItem.ImageInsets = new UIEdgeInsets(5, 0, -5, 0);
				};
			}

			if (!UserContext.Instanse.IsHomeFeedLoaded)
			{
				isHomeFeed = true;
				UserContext.Instanse.IsHomeFeedLoaded = true;
				NavigationController.TabBarItem.Image = UIImage.FromBundle("home");
				NavigationController.TabBarItem.SelectedImage = UIImage.FromBundle("home");
			}
			if (TabBarController != null)
			{
				TabBarController.NavigationController.NavigationBarHidden = true;
				TabBarController.NavigationController.NavigationBar.TintColor = UIColor.White;
				TabBarController.NavigationController.NavigationBar.BarTintColor = Constants.NavBlue;
			}
			collectionViewSource.GoToProfile += (username)  =>
            {
				if (username == UserContext.Instanse?.Username)
					return;
				var myViewController = Storyboard.InstantiateViewController(nameof(ProfileViewController)) as ProfileViewController;
				myViewController.Username = username;
				NavigationController.PushViewController(myViewController, true);
            };

			collectionViewSource.GoToComments += (postUrl)  =>
            {
				var myViewController = Storyboard.InstantiateViewController(nameof(CommentsViewController)) as CommentsViewController;
				myViewController.PostUrl = postUrl;
				navController.PushViewController(myViewController, true);
            };

			collectionViewSource.ImagePreview += (image, url) =>
			{
				var myViewController = Storyboard.InstantiateViewController(nameof(ImagePreviewViewController)) as ImagePreviewViewController;
				myViewController.imageForPreview = image;
				myViewController.ImageUrl = url;

				NavigationController.PushViewController(myViewController, true);
			};

			if (!isHomeFeed)
			{
				dropdown = CreateDropDownList();
			}
            SetNavBar();
			GetPosts();
        }

		public override void ViewWillAppear(bool animated)
		{
			if (UserContext.Instanse.CurrentPostCategory != currentPostCategory && !isHomeFeed)
			{
				currentPostCategory = UserContext.Instanse.CurrentPostCategory;
				collectionViewSource.PhotoList.Clear();
				collectionViewSource.FeedStrings.Clear();
				tw.Text = UserContext.Instanse.CurrentPostCategory;
				GetPosts();
			}

			base.ViewWillAppear(animated);
		}

		public override void ViewWillDisappear(bool animated)
		{
			if(!isHomeFeed && IsDropDownOpen)
				ToogleDropDownList();
			base.ViewWillDisappear(animated);
		}

		private async Task RefreshTable()
		{
			collectionViewSource.PhotoList.Clear();
			collectionViewSource.FeedStrings.Clear();
			_hasItems = true;
			await GetPosts(false);
		}

        void LoginTapped(object sender, EventArgs e)
        {
			var myViewController = Storyboard.InstantiateViewController(nameof(PreLoginViewController)) as PreLoginViewController;
            navController.PushViewController(myViewController, true);
        }

		void SearchTapped(object sender, EventArgs e)
		{
			var myViewController = Storyboard.InstantiateViewController(nameof(TagsSearchViewController)) as TagsSearchViewController;
			navController.PushViewController(myViewController, true);
		}

        private async Task LogoutTapped(object sender, EventArgs e)
        {
            //Handle logout
            /*var request = new LogoutRequest(UserContext.Instanse.Token);
            var lol = await Api.Logout(request);*/

            //UserContext.Instanse.Token = null;
            UserContext.Save();
            var myViewController = Storyboard.InstantiateViewController(nameof(FeedViewController)) as FeedViewController;
            this.NavigationController.ViewControllers = new UIViewController[2] { myViewController, this };
            this.NavigationController.PopViewController(false);
        }

        private void ToogleDropDownList()
        {
            if (dropdown.Frame.Y < 0)
            {
                UIView.Animate(0.3, 0, UIViewAnimationOptions.CurveEaseIn,
                    () =>
                    {
                        dropdown.Frame = new CGRect(dropdown.Frame.X, dropDownListOffsetFromTop, dropdown.Frame.Width, dropdown.Frame.Height);
                        arrow.Transform = CGAffineTransform.MakeRotation((nfloat)(-180 * (Math.PI/ 180)));
                }, null);
            }
            else
            {
                UIView.Animate(0.2,
                    () =>
                    {
                        dropdown.Frame = new CGRect(dropdown.Frame.X, dropdown.Frame.Y - dropdown.Frame.Height, dropdown.Frame.Width, dropdown.Frame.Height);
                        arrow.Transform = CGAffineTransform.MakeRotation((nfloat)(0 * (Math.PI / 180)));
                    }
                );
            }
        }

        public async Task GetPosts(bool shouldStartAnimating = true)
        {
            if (activityIndicator.IsAnimating)
                return;
			if(shouldStartAnimating)
            	activityIndicator.StartAnimating();
			noFeedLabel.Hidden = true;
			try
			{
				OperationResult<UserPostResponse> posts;
				string offset = collectionViewSource.PhotoList.Count == 0 ? "0" : _offsetUrl;

				if (!isHomeFeed)
				{
					if (UserContext.Instanse.CurrentPostCategory == null)
					{
						var postrequest = new PostsRequest(currentPostType)
						{
							SessionId = UserContext.Instanse.Token,
							Limit = limit,
							Offset = offset
						};
						posts = await Api.GetPosts(postrequest);
					}
					else
					{
						var postrequest = new PostsByCategoryRequest(currentPostType, UserContext.Instanse.CurrentPostCategory)
						{
							SessionId = UserContext.Instanse.Token,
							Limit = limit,
							Offset = offset
						};
						posts = await Api.GetPostsByCategory(postrequest);
					}
				}
				else
				{
					var f = new UserRecentPostsRequest(UserContext.Instanse.Token)
					{
						Limit = limit,
						Offset = offset
					};
					posts = await Api.GetUserRecentPosts(f);
				}

				if (posts.Success)
				{
					if (posts.Result == null || posts.Result.Results == null)
					{
						_hasItems = false;
						collectionViewSource.PhotoList.Clear();
						collectionViewSource.FeedStrings.Clear();
						feedCollection.ReloadData();
						feedCollection.CollectionViewLayout.InvalidateLayout();
						return;
					}

					if (posts.Result.Results.Count == 0 && isHomeFeed)
						noFeedLabel.Hidden = false;

					if (posts.Result.Results.Count != 0)
					{
						posts.Result.Results.FilterHided();
						var lastItem = posts.Result.Results.Last();
						_offsetUrl = lastItem.Url;

						if (posts.Result.Results.Count < limit / 2)
							_hasItems = false;
						else
							posts.Result.Results.Remove(lastItem);

						foreach (var r in posts.Result.Results)
						{
							var at = new NSMutableAttributedString();
							at.Append(new NSAttributedString(r.Author, Constants.NicknameAttribute));
							at.Append(new NSAttributedString($" {r.Title}"));
							collectionViewSource.FeedStrings.Add(at);
						}

						collectionViewSource.PhotoList.AddRange(posts.Result.Results);
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
				Reporter.SendCrash(ex);
            }
            finally
            {
                activityIndicator.StopAnimating();
            }
        }

		private async Task Vote(bool vote, string postUrl, Action<string, OperationResult<VoteResponse>> action)
        {
			
			if (!UserContext.Instanse.IsAuthorized)
			{
                LoginTapped(null, null);
				return;
			}
            try
            {
                var voteRequest = new VoteRequest(UserContext.Instanse.Token, vote, postUrl);
                var voteResponse = await Api.Vote(voteRequest);
				if (voteResponse.Success)
                {
					var u = collectionViewSource.PhotoList.First(p => p.Url == postUrl);
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
				Reporter.SendCrash(ex);
            }
        }

		private void Flagged(bool vote, string postUrl, Action<string, OperationResult<FlagResponse>> action)
		{
            if (!UserContext.Instanse.IsAuthorized)
			{
                LoginTapped(null, null);
				return;
			}
			UIAlertController actionSheetAlert = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
			actionSheetAlert.AddAction(UIAlertAction.Create("Flag photo",UIAlertActionStyle.Default, (obj) => FlagPhoto(vote, postUrl, action)));
			actionSheetAlert.AddAction(UIAlertAction.Create("Hide photo",UIAlertActionStyle.Default, (obj) =>  HidePhoto(postUrl)));
			actionSheetAlert.AddAction(UIAlertAction.Create("Cancel",UIAlertActionStyle.Cancel, (obj) => action.Invoke(postUrl, new OperationResult<FlagResponse>() {
				Success = false
			})));
			this.PresentViewController(actionSheetAlert,true,null);
		}

		private void HidePhoto(string url)
		{
			try
			{
				if (UserContext.Instanse.CurrentAccount.Postblacklist == null)
					UserContext.Instanse.CurrentAccount.Postblacklist = new List<string>();
				UserContext.Instanse.CurrentAccount.Postblacklist.Add(url);
				UserContext.Save();
				var postToHide = collectionViewSource.PhotoList.First(p => p.Url == url);
				var postIndex = collectionViewSource.PhotoList.IndexOf(postToHide);
				collectionViewSource.PhotoList.Remove(postToHide);
				collectionViewSource.FeedStrings.RemoveAt(postIndex);
				feedCollection.ReloadData();
				flowLayout.InvalidateLayout();
			}
			catch (Exception ex)
			{
				
			}
		}

		private async Task FlagPhoto(bool vote, string postUrl, Action<string, OperationResult<FlagResponse>> action)
		{
			try
			{
				var flagRequest = new FlagRequest(UserContext.Instanse.Token, vote, postUrl);
				var flagResponse = await Api.Flag(flagRequest);
				if (flagResponse.Success)
				{
					var u = collectionViewSource.PhotoList.First(p => p.Url == postUrl);
					u.Flag = flagResponse.Result.IsFlagged;
					if (flagResponse.Result.IsFlagged)
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
				Reporter.SendCrash(ex);
			}
		}

		private void SetNavBar()
		{
			navController.SetNavigationBarHidden(false, false);
			var barHeight = navController.NavigationBar.Frame.Height;
			UIView titleView = new UIView();

			tw = new UILabel(new CGRect(0, 0, 120, barHeight));
			tw.TextColor = UIColor.White;
			tw.Text = "FEED"; //ToConstants name
			tw.BackgroundColor = UIColor.Clear;
			tw.TextAlignment = UITextAlignment.Center;
			tw.Font = UIFont.SystemFontOfSize(17);
			titleView.Frame = new CoreGraphics.CGRect(0, 0, tw.Frame.Right, barHeight);

			titleView.Add(tw);
			if (!isHomeFeed)
			{
				tw.Text = "TRENDING"; // SET current post type
				UITapGestureRecognizer tapGesture = new UITapGestureRecognizer(ToogleDropDownList);
				titleView.AddGestureRecognizer(tapGesture);
				titleView.UserInteractionEnabled = true;

				var arrowSize = 15;
				arrow = new UIImageView(new CoreGraphics.CGRect(tw.Frame.Right, barHeight / 2 - arrowSize / 2, arrowSize, arrowSize));
				arrow.Image = UIImage.FromBundle("white-arrow-down");
				titleView.Add(arrow);
				titleView.Frame = new CGRect(0, 0, arrow.Frame.Right, barHeight);

				var rightBarButton = new UIBarButtonItem(UIImage.FromBundle("search"), UIBarButtonItemStyle.Plain, SearchTapped);
				navItem.SetRightBarButtonItem(rightBarButton, true);
			}

			navItem.TitleView = titleView;
			if (UserContext.Instanse.Token == null)
			{
				var leftBarButton = new UIBarButtonItem("Login", UIBarButtonItemStyle.Plain, LoginTapped); //ToConstants name
				navItem.SetLeftBarButtonItem(leftBarButton, true);
			}
			else
				navItem.SetLeftBarButtonItem(null, false);

            navController.NavigationBar.TintColor = UIColor.White;
			navController.NavigationBar.BarTintColor = Constants.NavBlue;
        }

        private UIView CreateDropDownList()
        {
            dropDownListOffsetFromTop = navController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
            var view = new UIView();
            view.BackgroundColor = UIColor.White;

            var buttonColor = UIColor.FromRGB(66, 165, 245); // To constants

            var newPhotosButton = new UIButton(new CGRect(0, 0, navController.NavigationBar.Frame.Width, 50));
            newPhotosButton.SetTitle("NEW PHOTOS", UIControlState.Normal); //ToConstants name
            newPhotosButton.BackgroundColor = buttonColor;
            newPhotosButton.TouchDown += ((e, obj) =>
               {
                   if(currentPostType == PostType.New && UserContext.Instanse.CurrentPostCategory != null)
                       return;
                   ToogleDropDownList();
                   collectionViewSource.PhotoList.Clear();
				   collectionViewSource.FeedStrings.Clear();
				   feedCollection.ReloadData();
                   currentPostType = PostType.New;
                   tw.Text = newPhotosButton.TitleLabel.Text;
				   UserContext.Instanse.CurrentPostCategory = currentPostCategory = null;
                   GetPosts();
				   feedCollection.SetContentOffset(new CGPoint(0, 0), false);
               });

            var hotButton = new UIButton(new CGRect(0, newPhotosButton.Frame.Bottom + 1, navController.NavigationBar.Frame.Width, 50));
            hotButton.SetTitle("HOT", UIControlState.Normal); //ToConstants name
            hotButton.BackgroundColor = buttonColor;

			hotButton.TouchDown += ((e, obj) =>
			   {
				   if (currentPostType == PostType.Hot && UserContext.Instanse.CurrentPostCategory != null)
					   return;
				   ToogleDropDownList();
				   collectionViewSource.PhotoList.Clear();
				   collectionViewSource.FeedStrings.Clear();
				   feedCollection.ReloadData();
				   currentPostType = PostType.Hot;
				   tw.Text = hotButton.TitleLabel.Text;
				   UserContext.Instanse.CurrentPostCategory = currentPostCategory = null;
				   GetPosts();
				   feedCollection.SetContentOffset(new CGPoint(0, 0), false);
			   });

            var trendingButton = new UIButton(new CGRect(0, hotButton.Frame.Bottom + 1, NavigationController.NavigationBar.Frame.Width, 50));
            trendingButton.SetTitle("TRENDING", UIControlState.Normal); //ToConstants name
            trendingButton.BackgroundColor = buttonColor;

			trendingButton.TouchDown += ((e, obj) =>
			   {
				   if (currentPostType == PostType.Top && UserContext.Instanse.CurrentPostCategory != null)
					   return;
				   ToogleDropDownList();
				   collectionViewSource.PhotoList.Clear();
				   collectionViewSource.FeedStrings.Clear();
				   feedCollection.ReloadData();
				   currentPostType = PostType.Top;
				   tw.Text = trendingButton.TitleLabel.Text;
				   UserContext.Instanse.CurrentPostCategory = currentPostCategory = null;
				   GetPosts();
				   feedCollection.SetContentOffset(new CGPoint(0, 0), false);
			   });

            view.Add(newPhotosButton);
            view.Add(hotButton);
            view.Add(trendingButton);
            view.Frame = new CGRect(0, dropDownListOffsetFromTop - navController.NavigationBar.Frame.Width, navController.NavigationBar.Frame.Width, trendingButton.Frame.Bottom);

            View.Add(view);
            return view;
        }
    }

	public class lilLayout : UICollectionViewFlowLayout
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

