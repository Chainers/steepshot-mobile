using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;
using UIKit;

namespace Steepshot.iOS
{
	public partial class ProfileViewController : BaseViewController
	{
		protected ProfileViewController(IntPtr handle) : base(handle) {}

		UserProfileResponse userData;
		public string Username = UserContext.Instanse.Username;
		ProfileCollectionViewSource collectionViewSource = new ProfileCollectionViewSource();
		private FeedTableViewSource tableSource = new FeedTableViewSource();

		private List<Post> photosList = new List<Post>();

		UINavigationController navController;
		private string _offsetUrl;
		private bool _hasItems = true;
		UIRefreshControl RefreshControl;
		private bool _isPostsLoading;
		private bool _isFeed = true;
		private ProfileHeaderViewController _profileHeader;

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			navController = NavigationController; //TabBarController != null ? TabBarController.NavigationController : NavigationController;
			if (TabBarController != null)
				TabBarController.NavigationController.NavigationBarHidden = true;
			
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
				if (_hasItems)
					GetUserPosts();
			});

			_profileHeader = new ProfileHeaderViewController(ProfileHeaderLoaded);
			_profileHeader.View.Frame = new CGRect(0, -_profileHeader.View.Frame.Height, _profileHeader.View.Frame.Width, _profileHeader.View.Frame.Height);
			collectionView.AddSubview(_profileHeader.View);
			collectionView.ContentInset = new UIEdgeInsets(_profileHeader.View.Frame.Height, 0, 0, 0);

			RefreshControl = new UIRefreshControl();
			RefreshControl.ValueChanged += async (sender, e) =>
						{
							photosList.Clear();
							//tableView.ReloadData();
							collectionView.ReloadData();
							_hasItems = true;
							await GetUserPosts();
							RefreshControl.EndRefreshing();

						};
			collectionView.Add(RefreshControl);

			GetUserInfo();
			GetUserPosts();
		}

		public override void ViewWillAppear(bool animated)
		{
			if (Username == UserContext.Instanse.Username)
			{
				navController.SetNavigationBarHidden(true, false);
				if (TabBarController != null)
					TabBarController.NavigationController.SetNavigationBarHidden(true, false);
			}
			base.ViewWillAppear(animated);
		}

		private void ProfileHeaderLoaded()
		{
			_profileHeader.SwitchButton.TouchDown += (sender, e) =>
			{
				if (!_isFeed)
				{
					_profileHeader.SwitchButton.SetImage(UIImage.FromFile("list.png"), UIControlState.Normal);
					collectionView.Delegate = new CollectionViewFlowDelegate();
				}
				else
				{
					_profileHeader.SwitchButton.SetImage(UIImage.FromFile("grid.png"), UIControlState.Normal);
					collectionView.Delegate = new NewCollectionViewFlowDelegate();
				}

				collectionView.ReloadData();
				_isFeed = !_isFeed;
			};

			_profileHeader.FollowButton.TouchDown += (object sender, EventArgs e) =>
			{
				Follow();
			};

			_profileHeader.SettingsButton.TouchDown += (sender, e) =>
			{
				var myViewController = Storyboard.InstantiateViewController(nameof(SettingsViewController)) as SettingsViewController;
				TabBarController.NavigationController.PushViewController(myViewController, true);
			};

			_profileHeader.FollowingButton.TouchDown += (sender, e) =>
			{
				var myViewController = Storyboard.InstantiateViewController(nameof(FollowViewController)) as FollowViewController;
				myViewController.Username = Username;
				myViewController.FriendsType = FriendsType.Following;
				NavigationController.PushViewController(myViewController, true);
			};

			_profileHeader.FollowersButton.TouchDown += (sender, e) =>
			{
				var myViewController = Storyboard.InstantiateViewController(nameof(FollowViewController)) as FollowViewController;
				myViewController.Username = Username;
				myViewController.FriendsType = FriendsType.Followers;
				NavigationController.PushViewController(myViewController, true);
			};
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

			_profileHeader.Username.Text = !string.IsNullOrEmpty(userData.Name) ? userData.Name : userData.Username;
			var culture = new CultureInfo("en-US");
			_profileHeader.Date.Text = $"Joined {userData.LastAccountUpdate.ToString("Y", culture)}";
			if(!string.IsNullOrEmpty(userData.Location))
				_profileHeader.Location.Text = userData.Location;
			if(!string.IsNullOrEmpty(userData.About))
				_profileHeader.DescriptionLabel.Text = userData.About;

			if (!string.IsNullOrEmpty(userData.ProfileImage))
				ImageDownloader.Download(userData.ProfileImage, _profileHeader.Avatar);
			else
				_profileHeader.Avatar.Image = UIImage.FromBundle("ic_user_placeholder");

			_profileHeader.Balance.SetTitle($"{userData.EstimatedBalance.ToString()}{Constants.Currency}", UIControlState.Normal);
			_profileHeader.SettingsButton.Hidden = Username != UserContext.Instanse.Username;

			var buttonsAttributes = new UIStringAttributes
			{
				Font = Constants.Bold12,
				ForegroundColor = UIColor.FromRGB(51,51,51),
				ParagraphStyle = new NSMutableParagraphStyle() { LineSpacing = 5, Alignment = UITextAlignment.Center }
			};

			var textAttributes = new UIStringAttributes
			{
				Font = Constants.Bold9,
				ForegroundColor = UIColor.FromRGB(153, 153, 153),
				ParagraphStyle = new NSMutableParagraphStyle() { LineSpacing = 5, Alignment = UITextAlignment.Center }
			};

			NSMutableAttributedString photosString = new NSMutableAttributedString();
			photosString.Append(new NSAttributedString(userData.PostCount.ToString(), buttonsAttributes));
			photosString.Append(new NSAttributedString(Environment.NewLine));
			photosString.Append(new NSAttributedString("PHOTOS", textAttributes));

			_profileHeader.PhotosButton.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
			_profileHeader.PhotosButton.TitleLabel.TextAlignment = UITextAlignment.Center;
			_profileHeader.PhotosButton.SetAttributedTitle(photosString, UIControlState.Normal);

			NSMutableAttributedString followingString = new NSMutableAttributedString();
			followingString.Append(new NSAttributedString(userData.FollowingCount.ToString(), buttonsAttributes));
			followingString.Append(new NSAttributedString(Environment.NewLine));
			followingString.Append(new NSAttributedString("FOLLOWING", textAttributes));

			_profileHeader.FollowingButton.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
			_profileHeader.FollowingButton.TitleLabel.TextAlignment = UITextAlignment.Center;
			_profileHeader.FollowingButton.SetAttributedTitle(followingString, UIControlState.Normal);

			NSMutableAttributedString followersString = new NSMutableAttributedString();
			followersString.Append(new NSAttributedString(userData.FollowersCount.ToString(), buttonsAttributes));
			followersString.Append(new NSAttributedString(Environment.NewLine));
			followersString.Append(new NSAttributedString("FOLLOWERS", textAttributes));

			_profileHeader.FollowersButton.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
			_profileHeader.FollowersButton.TitleLabel.TextAlignment = UITextAlignment.Center;
			_profileHeader.FollowersButton.SetAttributedTitle(followersString, UIControlState.Normal);

			ToogleFollowButton();

			loading.StopAnimating();
			collectionView.Hidden = false;
		}

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
					Offset = photosList.Count == 0 ? "0" : _offsetUrl,
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
				_profileHeader.FollowButtonWidth.Constant = 0;
				_profileHeader.FollowButtonMargin.Constant = 0;
			}
		}
	}
}

