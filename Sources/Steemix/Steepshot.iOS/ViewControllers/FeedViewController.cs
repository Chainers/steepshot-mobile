using System;
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

		private FeedTableViewSource tableSource = new FeedTableViewSource();
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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			navController = NavigationController; //TabBarController != null ? TabBarController.NavigationController : NavigationController;
			navItem = NavigationItem;//TabBarController != null ? TabBarController.NavigationItem : NavigationItem;

            FeedTable.Source = tableSource;
            FeedTable.LayoutMargins = UIEdgeInsets.Zero;
			FeedTable.RegisterClassForCellReuse(typeof(FeedTableViewCell), nameof(FeedTableViewCell));
            FeedTable.RegisterNibForCellReuse(UINib.FromName(nameof(FeedTableViewCell), NSBundle.MainBundle), nameof(FeedTableViewCell));
            tableSource.ScrolledToBottom += TableSource_ScrolledToBottom;
            tableSource.Voted += (vote, url, action)  =>
            {
                Vote(vote, url, action);
            };

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
			FeedTable.Add(RefreshControl);

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
			tableSource.GoToProfile += (username)  =>
            {
				if (username == UserContext.Instanse?.Username)
					return;
				var myViewController = Storyboard.InstantiateViewController(nameof(ProfileViewController)) as ProfileViewController;
				myViewController.Username = username;
				NavigationController.PushViewController(myViewController, true);
            };

			tableSource.GoToComments += (postUrl)  =>
            {
				var myViewController = Storyboard.InstantiateViewController(nameof(CommentsViewController)) as CommentsViewController;
				myViewController.PostUrl = postUrl;
				navController.PushViewController(myViewController, true);
            };

			tableSource.ImagePreview += (image) =>
			{
				var myViewController = Storyboard.InstantiateViewController(nameof(ImagePreviewViewController)) as ImagePreviewViewController;
				myViewController.imageForPreview = image;
				NavigationController.PushViewController(myViewController, true);
			};

            FeedTable.RowHeight = UITableView.AutomaticDimension;
            FeedTable.EstimatedRowHeight = 450f;
			if (!isHomeFeed)
			{
				dropdown = CreateDropDownList();
			}
            SetNavBar();
            GetPosts();
        }

		public override void ViewWillAppear(bool animated)
		{
			//SetNavBar();
			if (UserContext.Instanse.CurrentPostCategory != currentPostCategory && !isHomeFeed)
			{
				currentPostCategory = UserContext.Instanse.CurrentPostCategory;
				tableSource.TableItems.Clear();
				FeedTable.ReloadData();
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
			tableSource.TableItems.Clear();
			FeedTable.ReloadData();
			_hasItems = true;
			await GetPosts(false);
		}

        private void TableSource_ScrolledToBottom()
        {
			if(_hasItems)
            	GetPosts();
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
				string offset = tableSource.TableItems.Count == 0 ? "0" : _offsetUrl;

				if (!isHomeFeed)
				{
					if (UserContext.Instanse.CurrentPostCategory == null)
					{
						var postrequest = new PostsRequest(currentPostType)
						{
							SessionId = UserContext.Instanse.Token,
							Limit = 20,
							Offset = offset
						};
						posts = await Api.GetPosts(postrequest);
					}
					else
					{
						var postrequest = new PostsByCategoryRequest(currentPostType, UserContext.Instanse.CurrentPostCategory)
						{
							SessionId = UserContext.Instanse.Token,
							Limit = 20,
							Offset = offset
						};
						posts = await Api.GetPostsByCategory(postrequest);
					}
				}
				else
				{
					var f = new UserRecentPostsRequest(UserContext.Instanse.Token)
					{
						Limit = 20,
						Offset = offset
					};
					posts = await Api.GetUserRecentPosts(f);
				}
				if (posts.Result.Results.Count == 0 && isHomeFeed)
					noFeedLabel.Hidden = false;

				var lastItem = posts.Result.Results.Last();
				_offsetUrl = lastItem.Url;

				if (posts.Result.Results.Count == 1)
					_hasItems = false;
				else
					posts.Result.Results.Remove(lastItem);
				

				if (posts.Result.Results.Count != 0)
				{
					tableSource.TableItems.AddRange(posts.Result.Results);
					/*if (UserContext.Instanse.NetworkChanged)
					{
						FeedTable.SetContentOffset(new CGPoint(0, 0), false);
					}*/
					FeedTable.ReloadData();
				}
				else
					_hasItems = false;
				
            }
            catch (Exception ex)
            {
                //logging
            }
            finally
            {
                activityIndicator.StopAnimating();
            }
        }

		private async Task Vote(bool vote, string postUrl, Action<string, VoteResponse> action)
        {
			if (UserContext.Instanse.Token == null)
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
					var u = tableSource.TableItems.First(p => p.Url == postUrl);
					u.Vote = vote;
					u.NetVotes++;
					action.Invoke(postUrl, voteResponse.Result);
                }
            }
            catch (Exception ex)
            {
                //logging
            }
        }

		private void SetNavBar()
		{
			navController.SetNavigationBarHidden(false, false);
			var barHeight = navController.NavigationBar.Frame.Height;
			UIView titleView = new UIView();

			tw = new UILabel(new CoreGraphics.CGRect(0, 0, 120, barHeight));
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
				titleView.Frame = new CoreGraphics.CGRect(0, 0, arrow.Frame.Right, barHeight);

				var rightBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_search_gray"), UIBarButtonItemStyle.Plain, SearchTapped); //ToConstants name
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
			navController.NavigationBar.BarTintColor = Constants.NavBlue; // To constants
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
                if(currentPostType == PostType.New)
                       return;
                   ToogleDropDownList();
                   tableSource.TableItems.Clear();
                   FeedTable.ReloadData();
                   currentPostType = PostType.New;
                   tw.Text = newPhotosButton.TitleLabel.Text;
				   UserContext.Instanse.CurrentPostCategory = null;
                   GetPosts();
               });

            var hotButton = new UIButton(new CGRect(0, newPhotosButton.Frame.Bottom + 1, navController.NavigationBar.Frame.Width, 50));
            hotButton.SetTitle("HOT", UIControlState.Normal); //ToConstants name
            hotButton.BackgroundColor = buttonColor;

			hotButton.TouchDown += ((e, obj) =>
			   {
				   if (currentPostType == PostType.Hot)
					   return;
				   ToogleDropDownList();
				   tableSource.TableItems.Clear();
				   FeedTable.ReloadData();
				   currentPostType = PostType.Hot;
				   tw.Text = hotButton.TitleLabel.Text;
				   UserContext.Instanse.CurrentPostCategory = null;
				   GetPosts();
			   });

            var trendingButton = new UIButton(new CGRect(0, hotButton.Frame.Bottom + 1, NavigationController.NavigationBar.Frame.Width, 50));
            trendingButton.SetTitle("TRENDING", UIControlState.Normal); //ToConstants name
            trendingButton.BackgroundColor = buttonColor;

			trendingButton.TouchDown += ((e, obj) =>
			   {
				   if (currentPostType == PostType.Top)
					   return;
				   ToogleDropDownList();
				   tableSource.TableItems.Clear();
				   FeedTable.ReloadData();
				   currentPostType = PostType.Top;
				   tw.Text = trendingButton.TitleLabel.Text;
				   UserContext.Instanse.CurrentPostCategory = null;
				   GetPosts();
			   });

            view.Add(newPhotosButton);
            view.Add(hotButton);
            view.Add(trendingButton);
            view.Frame = new CGRect(0, dropDownListOffsetFromTop - navController.NavigationBar.Frame.Width, navController.NavigationBar.Frame.Width, trendingButton.Frame.Bottom);

            View.Add(view);
            return view;
        }
    }
}

