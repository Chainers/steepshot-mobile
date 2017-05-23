using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Foundation;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;
using UIKit;

namespace Steepshot.iOS
{
	public partial class ProfileViewController : BaseViewController
	{
		UserProfileResponse userData;
		public string Username = UserContext.Instanse.Username;
		ProfileCollectionViewSource collectionViewSource = new ProfileCollectionViewSource();
		private FeedTableViewSource tableSource = new FeedTableViewSource();

		private List<Post> photosList = new List<Post>();

		UINavigationController navController;
		private string _offsetUrl;
		private bool _hasItems = true;
		UIRefreshControl RefreshControl;

		protected ProfileViewController(IntPtr handle) : base(handle)
        {
			// Note: this .ctor should not contain any initialization logic.
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			navController = NavigationController; //TabBarController != null ? TabBarController.NavigationController : NavigationController;
			if(TabBarController != null)
				TabBarController.NavigationController.NavigationBarHidden = true;

			avatar.Layer.CornerRadius = avatar.Frame.Width / 2;
			headerView.BackgroundColor = Constants.Blue;

			balanceButton.ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
			balanceButton.ContentEdgeInsets = new UIEdgeInsets(0,11,0,4);
			balanceButton.ImageEdgeInsets = new UIEdgeInsets(0,-9,0,0);
			balanceButton.Layer.BorderColor = UIColor.White.CGColor;
			balanceButton.Layer.BorderWidth = 1;
			balanceButton.Layer.CornerRadius = 5;

			collectionViewSource.PhotoList = photosList;
			collectionView.RegisterClassForCell(typeof(PhotoCollectionViewCell), "PhotoCollectionViewCell");
			collectionView.RegisterNibForCell(UINib.FromName("PhotoCollectionViewCell", NSBundle.MainBundle), "PhotoCollectionViewCell");
			collectionView.Source = collectionViewSource;
			collectionView.Delegate = new CollectionViewFlowDelegate((indexPath) =>
			{
				var collectionCell = (PhotoCollectionViewCell)collectionView.CellForItem(indexPath);
				PreviewPhoto(collectionCell.Image);
			},
			() =>
			{
				if(_hasItems)
					GetUserPosts();
			});

			tableSource.TableItems = photosList;
			tableView.Source = tableSource;
            tableView.LayoutMargins = UIEdgeInsets.Zero;
            tableView.RegisterClassForCellReuse(typeof(FeedTableViewCell), "FeedTableViewCell");
            tableView.RegisterNibForCellReuse(UINib.FromName("FeedTableViewCell", NSBundle.MainBundle), "FeedTableViewCell");
            tableSource.Voted += (vote, url, action)  =>
            {
                Vote(vote, url, action);
            };
			tableSource.GoToComments += (postUrl)  =>
            {
				var myViewController = Storyboard.InstantiateViewController(nameof(CommentsViewController)) as CommentsViewController;
				myViewController.PostUrl = postUrl;
				navController.PushViewController(myViewController, true);
            };
			tableSource.ImagePreview += PreviewPhoto;
			tableSource.ScrolledToBottom += () => GetUserPosts();
            tableView.RowHeight = UITableView.AutomaticDimension;
            tableView.EstimatedRowHeight = 450f;

			switchButton.TouchDown += (object sender, EventArgs e) =>
			{
				collectionView.Hidden = !collectionView.Hidden;
				tableView.Hidden = !tableView.Hidden;
				if (collectionView.Hidden)
				{
					switchButton.SetBackgroundImage(UIImage.FromFile("ic_gray_grid.png"), UIControlState.Normal);
				}
				else
				{
					switchButton.SetBackgroundImage(UIImage.FromFile("ic_gray_list.png"), UIControlState.Normal);
				}
			};

			followButton.TouchDown += (object sender, EventArgs e) =>
			{
				Follow();
			};

			settingsButton.TouchDown += (sender, e) =>
			{
				var myViewController = Storyboard.InstantiateViewController(nameof(SettingsViewController)) as SettingsViewController;
				TabBarController.NavigationController.PushViewController(myViewController, true);
			};

			followingButton.TouchDown += (sender, e) =>
			{
				var myViewController = Storyboard.InstantiateViewController(nameof(FollowViewController)) as FollowViewController;
				myViewController.Username = Username;
				myViewController.FriendsType = FriendsType.Following;
				NavigationController.PushViewController(myViewController, true);
			};

			followersButton.TouchDown += (sender, e) =>
			{
				var myViewController = Storyboard.InstantiateViewController(nameof(FollowViewController)) as FollowViewController;
				myViewController.Username = Username;
				myViewController.FriendsType = FriendsType.Followers;
				NavigationController.PushViewController(myViewController, true);
			};

			settingsButton.Hidden = Username != UserContext.Instanse.Username;

			NavigationController.NavigationBar.TintColor = UIColor.White;
			NavigationController.NavigationBar.BarTintColor = Constants.NavBlue;

			RefreshControl = new UIRefreshControl();
			RefreshControl.ValueChanged += async (sender, e) =>
						{
							photosList.Clear();
							tableView.ReloadData();
							collectionView.ReloadData();
							_hasItems = true;
							await GetUserPosts();
							RefreshControl.EndRefreshing();
							
						};
			tableView.Add(RefreshControl);
			collectionView.Add(RefreshControl);

			GetUserInfo();
			GetUserPosts();
		}

		public override void ViewWillAppear(bool animated)
		{
			if (Username == UserContext.Instanse.Username)
			{
				navController.SetNavigationBarHidden(true, false);
				if(TabBarController != null)
					TabBarController.NavigationController.SetNavigationBarHidden(true, false);
			}
			base.ViewWillAppear(animated);
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			if (UserContext.Instanse.ShouldProfileUpdate)
			{
				photosList.Clear();
				_hasItems = true;
				GetUserInfo();
				GetUserPosts();
				UserContext.Instanse.ShouldProfileUpdate = false;
			}
		}

		private void PreviewPhoto(UIImage image)
		{
			var myViewController = Storyboard.InstantiateViewController(nameof(ImagePreviewViewController)) as ImagePreviewViewController;
			myViewController.imageForPreview = image;
			NavigationController.PushViewController(myViewController, true);
		}

		private async Task GetUserInfo()
		{
			var req = new UserProfileRequest(Username) { SessionId = UserContext.Instanse.Token };
			var response = await Api.GetUserProfile(req);
			userData = response.Result;

			nameLabel.Text = userData.Username;
			dateLabel.Text = userData.LastAccountUpdate.ToString();
			descriptionLabel.Text = userData.About;
			locationLabel.Text = userData.Location;

			if (!string.IsNullOrEmpty(userData.ProfileImage))
				ImageDownloader.Download(userData.ProfileImage, avatar);
			else
				avatar.Image = UIImage.FromBundle("ic_user_placeholder");

			balanceButton.SetTitle($"{Constants.Currency}{userData.EstimatedBalance.ToString()} ON BALANCE", UIControlState.Normal);
			
			var buttonsAttributes = new UIStringAttributes
			{
				Font = UIFont.SystemFontOfSize(10),
				ForegroundColor = UIColor.Black,
				ParagraphStyle = new NSMutableParagraphStyle() { LineSpacing = 5, Alignment = UITextAlignment.Center }
			};

			NSMutableAttributedString photosString = new NSMutableAttributedString();
			photosString.Append(new NSAttributedString(userData.PostCount.ToString(), buttonsAttributes));
			photosString.Append(new NSAttributedString(Environment.NewLine));
			photosString.Append(new NSAttributedString("PHOTOS"));

			photosButton.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
			photosButton.TitleLabel.TextAlignment = UITextAlignment.Center;
			photosButton.SetAttributedTitle(photosString, UIControlState.Normal);

			NSMutableAttributedString followingString = new NSMutableAttributedString();
			followingString.Append(new NSAttributedString(userData.FollowingCount.ToString(), buttonsAttributes));
			followingString.Append(new NSAttributedString(Environment.NewLine));
			followingString.Append(new NSAttributedString("FOLLOWING"));

			followingButton.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
			followingButton.TitleLabel.TextAlignment = UITextAlignment.Center;
			followingButton.SetAttributedTitle(followingString, UIControlState.Normal);

			NSMutableAttributedString followersString = new NSMutableAttributedString();
			followersString.Append(new NSAttributedString(userData.FollowersCount.ToString(), buttonsAttributes));
			followersString.Append(new NSAttributedString(Environment.NewLine));
			followersString.Append(new NSAttributedString("FOLLOWERS"));

			followersButton.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
			followersButton.TitleLabel.TextAlignment = UITextAlignment.Center;
			followersButton.SetAttributedTitle(followersString, UIControlState.Normal);

			ToogleFollowButton();

			loadingView.Hidden = true;
		}

		private bool _isPostsLoading;

		public async Task GetUserPosts()
		{
			if (_isPostsLoading)
				return;
			_isPostsLoading = true;
			try
			{
				var req = new UserPostsRequest(Username)
				{
					Limit = 20,
					Offset = tableSource.TableItems.Count == 0 ? "0" : _offsetUrl,
					SessionId = UserContext.Instanse.Token
				};
				var response = await Api.GetUserPosts(req);
				if (response.Success)
				{
					var lastItem = response.Result.Results.Last();
					_offsetUrl = lastItem.Url;
					if (response.Result.Results.Count == 1)
						_hasItems = false;
					else
						response.Result.Results.Remove(lastItem);

					photosList.AddRange(response.Result.Results);
					collectionView.ReloadData();
					tableView.ReloadData();

				}
			}
			catch (Exception ex)
			{
				//logging
			}
			finally
			{
				_isPostsLoading = false;
			}
		}


		private async Task Vote(bool vote, string postUrl, Action<string, VoteResponse> action)
		{
			try
			{
				if (UserContext.Instanse.Token == null)
				{
					var myViewController = Storyboard.InstantiateViewController(nameof(LoginViewController)) as LoginViewController;
					navController.PushViewController(myViewController, true);
					return;
				}

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

		public async Task Follow()
		{
			var request = new FollowRequest(UserContext.Instanse.Token, (userData.HasFollowed == 0) ? FollowType.Follow : FollowType.UnFollow, userData.Username);
			var resp = await Api.Follow(request);
			if (resp.Errors.Count == 0)
			{
				userData.HasFollowed = (resp.Result.IsFollowed) ? 1 : 0;
				ToogleFollowButton();
			}
		}

		private void ToogleFollowButton()
		{
			if (Username == UserContext.Instanse.Username || Convert.ToBoolean(userData.HasFollowed))
			{
				followButtonWidth.Constant = 0;
				followButtonMargin.Constant = 0;
			}
		}
	}
}

