using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using FFImageLoading;
using Foundation;
using Steepshot.Core;
using Steepshot.Core.Errors;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.Cells;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class ProfileViewController : BaseViewControllerWithPresenter<UserProfilePresenter>
    {
        protected override void CreatePresenter()
        {
            _presenter = new UserProfilePresenter() { UserName = Username };
        }

        private UserProfileResponse _userData;
        public string Username = BasePresenter.User.Login;
        private ProfileCollectionViewSource _collectionViewSource;
        private UIRefreshControl _refreshControl;
        private bool _isPostsLoading;
        private ProfileHeaderViewController _profileHeader;
        private CollectionViewFlowDelegate _gridDelegate;
        private int _lastRow;
        private UINavigationController _navController;
        private UIBarButtonItem switchButton;
        private bool _userDataLoaded;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _navController = TabBarController != null ? TabBarController.NavigationController : NavigationController;

            _collectionViewSource = new ProfileCollectionViewSource(_presenter);

            _presenter.SourceChanged += (Core.Models.Status obj) =>
            {
                var offset = collectionView.ContentOffset;
                collectionView.ReloadData();
                collectionView.LayoutIfNeeded();
                collectionView.SetContentOffset(offset, false);
            };

            _collectionViewSource.CellAction += CellAction;

            collectionView.RegisterClassForCell(typeof(PhotoCollectionViewCell), nameof(PhotoCollectionViewCell));
            collectionView.RegisterNibForCell(UINib.FromName(nameof(PhotoCollectionViewCell), NSBundle.MainBundle), nameof(PhotoCollectionViewCell));
            collectionView.RegisterClassForCell(typeof(FeedCollectionViewCell), nameof(FeedCollectionViewCell));
            collectionView.RegisterNibForCell(UINib.FromName(nameof(FeedCollectionViewCell), NSBundle.MainBundle), nameof(FeedCollectionViewCell));
            collectionView.Source = _collectionViewSource;

            collectionView.SetCollectionViewLayout(new UICollectionViewFlowLayout()
            {
                EstimatedItemSize = new CGSize(UIScreen.MainScreen.Bounds.Width, 1),
                MinimumLineSpacing = 1,
                MinimumInteritemSpacing = 1,
            }, false);

            _gridDelegate = new CollectionViewFlowDelegate(collectionView, _presenter);
            _gridDelegate.IsGrid = false;
            _gridDelegate.ScrolledToBottom += ScrolledToBottom;
            collectionView.Delegate = _gridDelegate;

            _profileHeader = new ProfileHeaderViewController(ProfileHeaderLoaded);
            collectionView.ContentInset = new UIEdgeInsets(300, 0, 0, 0);
            collectionView.AddSubview(_profileHeader.View);

            _refreshControl = new UIRefreshControl();
            _refreshControl.ValueChanged += RefreshControl_ValueChanged;
            collectionView.Add(_refreshControl);

            SetBackButton();

            GetUserInfo();
            GetUserPosts();
        }

        private void SetBackButton()
        {
            switchButton = new UIBarButtonItem(UIImage.FromBundle("ic_grid_nonactive"), UIBarButtonItemStyle.Plain, SwitchLayout);
            switchButton.TintColor = Helpers.Constants.R151G155B158;

            if (Username == BasePresenter.User.Login)
            {
                NavigationItem.Title = "My Profile";
                var settingsButton = new UIBarButtonItem(UIImage.FromBundle("ic_settings"), UIBarButtonItemStyle.Plain, GoToSettings);
                settingsButton.TintColor = Helpers.Constants.R151G155B158;
                NavigationItem.RightBarButtonItems = new UIBarButtonItem[] { settingsButton, switchButton };
            }
            else
            {
                NavigationItem.Title = Username;
                var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
                leftBarButton.TintColor = Helpers.Constants.R15G24B30;
                NavigationItem.LeftBarButtonItem = leftBarButton;
                NavigationItem.RightBarButtonItem = switchButton;
            }
        }

        private async void ScrolledToBottom()
        {
            await GetUserPosts();
        }

        private void GoBack(object sender, EventArgs e)
        {
            NavigationController.PopViewController(true);
        }

        async void RefreshControl_ValueChanged(object sender, EventArgs e)
        {
            await RefreshPage();
            _refreshControl.EndRefreshing();
        }

        private void SourceChanged(Status status)
        {
            var offset = collectionView.ContentOffset;
            collectionView.ReloadData();
            collectionView.LayoutIfNeeded();
            collectionView.SetContentOffset(offset, false);
        }

        /*
        public override void ViewWillAppear(bool animated)
        {
            if (Username == BasePresenter.User.Login)
            {
                NavigationController.SetNavigationBarHidden(true, false);
                if (TabBarController != null)
                    TabBarController.NavigationController.SetNavigationBarHidden(true, false);
            }
            else
            {
                NavigationController.SetNavigationBarHidden(false, false);
            }
            base.ViewWillAppear(animated);
        }*/

        private void ProfileHeaderLoaded()
        {
            _profileHeader.FollowButton.TouchDown += (object sender, EventArgs e) =>
            {
                Follow();
            };

            _profileHeader.FollowingButton.TouchDown += (sender, e) =>
            {
                var myViewController = new FollowViewController(FriendsType.Following, _userData);
                NavigationController.PushViewController(myViewController, true);
            };

            _profileHeader.FollowersButton.TouchDown += (sender, e) =>
            {
                var myViewController = new FollowViewController(FriendsType.Followers, _userData);
                NavigationController.PushViewController(myViewController, true);
            };
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            if (ShouldProfileUpdate)
            {
                RefreshPage();
                ShouldProfileUpdate = false;
            }
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

        private async Task RefreshPage()
        {
            GetUserInfo();
            await GetUserPosts(true);
        }

        private void PreviewPhoto(UIImage image, string url)
        {
            var myViewController = new ImagePreviewViewController();
            myViewController.ImageForPreview = image;
            myViewController.ImageUrl = url;
            _navController.PushViewController(myViewController, true);
        }

        private async Task GetUserInfo()
        {
            _userDataLoaded = false;
            errorMessage.Hidden = true;
            try
            {
                var error = await _presenter.TryGetUserInfo(Username);
                _refreshControl.EndRefreshing();

                if (error == null)
                {
                    _userData = _presenter.UserProfileResponse;

                    if (string.IsNullOrEmpty(_userData.Name))
                        _profileHeader.Username.Hidden = true;
                    else
                    {
                        _profileHeader.Username.Hidden = false;
                        _profileHeader.Username.Text = _userData.Name;
                    }

                    _profileHeader.Balance.Text = $"$ {_userData.EstimatedBalance}";
                    _profileHeader.Location.Text = _userData.Location;

                    if (string.IsNullOrEmpty(_userData.About))
                        _profileHeader.DescriptionView.Hidden = true;
                    else
                    {
                        _profileHeader.DescriptionView.Hidden = false;
                        _profileHeader.DescriptionLabel.Text = _userData.About;
                    }

                    if (!string.IsNullOrEmpty(_userData.ProfileImage))
                        ImageService.Instance.LoadUrl(_userData.ProfileImage, TimeSpan.FromDays(30))
                                             .FadeAnimation(false, false, 0)
                                             .DownSample(width: (int)_profileHeader.Avatar.Frame.Width)
                                             .Into(_profileHeader.Avatar);
                    else
                        _profileHeader.Avatar.Image = UIImage.FromBundle("ic_noavatar");

                    var buttonsAttributes = new UIStringAttributes
                    {
                        Font = Steepshot.iOS.Helpers.Constants.Semibold20,
                        ForegroundColor = Steepshot.iOS.Helpers.Constants.R15G24B30,
                    };

                    var textAttributes = new UIStringAttributes
                    {
                        Font = Steepshot.iOS.Helpers.Constants.Regular12,
                        ForegroundColor = Steepshot.iOS.Helpers.Constants.R151G155B158,
                    };

                    NSMutableAttributedString photosString = new NSMutableAttributedString();
                    photosString.Append(new NSAttributedString(_userData.PostCount.ToString("N0"), buttonsAttributes));
                    photosString.Append(new NSAttributedString(Environment.NewLine));
                    photosString.Append(new NSAttributedString("Photos", textAttributes));

                    _profileHeader.PhotosButton.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
                    _profileHeader.PhotosButton.TitleLabel.TextAlignment = UITextAlignment.Left;
                    _profileHeader.PhotosButton.SetAttributedTitle(photosString, UIControlState.Normal);

                    NSMutableAttributedString followingString = new NSMutableAttributedString();
                    followingString.Append(new NSAttributedString(_userData.FollowingCount.ToString("N0"), buttonsAttributes));
                    followingString.Append(new NSAttributedString(Environment.NewLine));
                    followingString.Append(new NSAttributedString("Following", textAttributes));

                    _profileHeader.FollowingButton.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
                    _profileHeader.FollowingButton.TitleLabel.TextAlignment = UITextAlignment.Left;
                    _profileHeader.FollowingButton.SetAttributedTitle(followingString, UIControlState.Normal);

                    NSMutableAttributedString followersString = new NSMutableAttributedString();
                    followersString.Append(new NSAttributedString(_userData.FollowersCount.ToString("N0"), buttonsAttributes));
                    followersString.Append(new NSAttributedString(Environment.NewLine));
                    followersString.Append(new NSAttributedString("Followers", textAttributes));

                    _profileHeader.FollowersButton.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
                    _profileHeader.FollowersButton.TitleLabel.TextAlignment = UITextAlignment.Left;
                    _profileHeader.FollowersButton.SetAttributedTitle(followersString, UIControlState.Normal);

                    if (string.IsNullOrEmpty(_userData.Website))
                        _profileHeader.WebsiteHeight.Constant = 0;
                    else
                    {
                        _profileHeader.Website.Text = _userData.Website;
                        _profileHeader.WebsiteHeight.Constant = _profileHeader.Website.SizeThatFits(new CGSize(UIScreen.MainScreen.Bounds.Width, 300)).Height;
                    }

                    _profileHeader.DecorateFollowButton(_userData.HasFollowed, Username);

                    if (!_refreshControl.Refreshing)
                    {
                        _profileHeader.View.Frame = new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, 0);
                        var size = _profileHeader.View.SystemLayoutSizeFittingSize(new CGSize(UIScreen.MainScreen.Bounds.Width, 300));

                        _profileHeader.View.Frame = new CGRect(0, -size.Height, UIScreen.MainScreen.Bounds.Width, size.Height);
                        collectionView.ContentInset = new UIEdgeInsets(size.Height, 0, 0, 0);
                        collectionView.Hidden = false;
                    }
                }
                else
                {
                    //Reporter.SendCrash(response.Errors[0], BasePresenter.User.Login, AppVersion);
                }
            }
            catch (Exception ex)
            {
                errorMessage.Hidden = false;
                AppSettings.Reporter.SendCrash(ex);
            }
            finally
            {
                _userDataLoaded = true;
                loading.StopAnimating();
            }
        }

        private void GoToSettings(object sender, EventArgs e)
        {
            var myViewController = new SettingsViewController();
            TabBarController.NavigationController.PushViewController(myViewController, true);
        }

        private void SwitchLayout(object sender, EventArgs e)
        {
            _collectionViewSource.IsGrid = !_collectionViewSource.IsGrid;
            if (_collectionViewSource.IsGrid)
                switchButton.TintColor = Helpers.Constants.R231G72B0;
            else
                switchButton.TintColor = Helpers.Constants.R151G155B158;

            collectionView.ReloadData();
            collectionView.SetContentOffset(new CGPoint(0, 0), false);
        }

        private async Task GetUserPosts(bool needRefresh = false)
        {
            if (_isPostsLoading)
                return;
            _isPostsLoading = true;

            if (needRefresh)
                _presenter.Clear();

            var error = await _presenter.TryLoadNextPosts();

            if (error == null)
            {
                _isPostsLoading = false;
            }
            else
            {
                ShowAlert(error);
            }

            if (_userDataLoaded)
            {
                loading.StopAnimating();
                collectionView.Hidden = false;
            }
        }

        private async Task Vote(Post post)
        {
            if (!BasePresenter.User.IsAuthenticated)
            {
                LoginTapped();
                return;
            }

            if (post == null)
                return;

            var error = await _presenter.TryVote(post);
            if (error is TaskCanceledError)
                return;

            ShowAlert(error);
            //collectionView.ReloadData();
            //collectionView.CollectionViewLayout.InvalidateLayout();
        }

        private void Flagged(Post post)
        {
            if (!BasePresenter.User.IsAuthenticated)
            {
                LoginTapped();
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
            //collectionView.ReloadData();
            //collectionView.CollectionViewLayout.InvalidateLayout();
        }

        private async Task Follow()
        {
            _profileHeader.DecorateFollowButton(null, Username);
            var error = await _presenter.TryFollow();

            if (error == null)
                _profileHeader.DecorateFollowButton(_userData.HasFollowed, Username);
            else
                ShowAlert(error);
        }

        void LoginTapped()
        {
            var myViewController = new PreLoginViewController();
            NavigationController.PushViewController(myViewController, true);
        }
    }
}
