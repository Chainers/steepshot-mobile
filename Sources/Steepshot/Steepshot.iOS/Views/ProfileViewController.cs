using System;
using System.Threading.Tasks;
using CoreGraphics;
using FFImageLoading;
using Foundation;
using PureLayout.Net;
using Steepshot.Core.Extensions;
using Steepshot.Core.Localization;
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
    public partial class ProfileViewController : BasePostController<UserProfilePresenter> //BaseViewControllerWithPresenter<UserProfilePresenter>
    {
        protected override void CreatePresenter()
        {
            _presenter = new UserProfilePresenter() { UserName = Username };
            _presenter.SourceChanged += SourceChanged;
        }

        private UserProfileResponse _userData;
        public string Username = AppSettings.User.Login;
        private ProfileCollectionViewSource _collectionViewSource;
        private UIRefreshControl _refreshControl;
        private bool _isPostsLoading;
        private ProfileHeaderViewController _profileHeader;
        private CollectionViewFlowDelegate _gridDelegate;
        private SliderCollectionViewFlowDelegate _sliderGridDelegate;
        private UINavigationController _navController;
        private UIBarButtonItem switchButton;
        private bool _userDataLoaded;
        private UIView powerPopup;
        private UILabel powerText;
        private bool isPowerOpen;
        private bool UserIsWatched => AppSettings.User.WatchedUsers.Contains(Username);

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _navController = TabBarController != null ? TabBarController.NavigationController : NavigationController;

            collectionView.RegisterClassForCell(typeof(LoaderCollectionCell), nameof(LoaderCollectionCell));
            collectionView.RegisterClassForCell(typeof(NewFeedCollectionViewCell), nameof(NewFeedCollectionViewCell));
            collectionView.RegisterClassForCell(typeof(PhotoCollectionViewCell), nameof(PhotoCollectionViewCell));

            collectionView.SetCollectionViewLayout(new UICollectionViewFlowLayout()
            {
                MinimumLineSpacing = 0,
                MinimumInteritemSpacing = 0,
            }, false);

            _gridDelegate = new CollectionViewFlowDelegate(collectionView, _presenter);
            _gridDelegate.IsGrid = false;
            _gridDelegate.ScrolledToBottom += ScrolledToBottom;
            _gridDelegate.CellClicked += CellAction;

            _collectionViewSource = new ProfileCollectionViewSource(_presenter, _gridDelegate);
            _collectionViewSource.CellAction += CellAction;
            _collectionViewSource.TagAction += TagAction;
            collectionView.Source = _collectionViewSource;
            collectionView.Delegate = _gridDelegate;

            _sliderGridDelegate = new SliderCollectionViewFlowDelegate(sliderCollection, _presenter);
            _sliderGridDelegate.ScrolledToBottom += ScrolledToBottom;

            var _sliderCollectionViewSource = new SliderCollectionViewSource(_presenter, _sliderGridDelegate);
            _sliderCollectionViewSource.CellAction += CellAction;
            _sliderCollectionViewSource.TagAction += TagAction;
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
            sliderCollection.Delegate = _sliderGridDelegate;

            SliderAction += (isOpening) =>
            {
                if (!sliderCollection.Hidden)
                    sliderCollection.ScrollEnabled = !isOpening;
            };

            _profileHeader = new ProfileHeaderViewController(ProfileHeaderLoaded);
            collectionView.ContentInset = new UIEdgeInsets(300, 0, 0, 0);
            collectionView.AddSubview(_profileHeader.View);

            _refreshControl = new UIRefreshControl();
            _refreshControl.ValueChanged += RefreshControl_ValueChanged;
            collectionView.Add(_refreshControl);

            if (TabBarController != null)
                ((MainTabBarController)TabBarController).SameTabTapped += SameTabTapped;
            SetBackButton();

            if (Username == AppSettings.User.Login && AppSettings.User.IsAuthenticated)
                SetVotePowerView();
            GetUserInfo();
            GetPosts();
        }

        private void SetVotePowerView()
        {
            powerPopup = new UIView();
            powerPopup.Frame = new CGRect(0, -NavigationController.NavigationBar.Frame.Bottom, UIScreen.MainScreen.Bounds.Width, NavigationController.NavigationBar.Frame.Bottom);

            powerText = new UILabel();
            powerText.TextAlignment = UITextAlignment.Center;
            powerText.TextColor = UIColor.White;
            powerText.Font = Constants.Semibold14;
            powerPopup.AddSubview(powerText);

            var pseudoBar = new UIView();
            powerPopup.AddSubview(pseudoBar);
            pseudoBar.AutoSetDimension(ALDimension.Height, 20f);
            pseudoBar.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
            pseudoBar.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            pseudoBar.AutoPinEdgeToSuperviewEdge(ALEdge.Right);

            powerText.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, pseudoBar);
            powerText.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            powerText.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            powerText.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            Constants.CreateGradient(powerPopup, 0);

            NavigationController.View.AddSubview(powerPopup);
        }

        private void SetBackButton()
        {
            switchButton = new UIBarButtonItem(UIImage.FromBundle("ic_grid_nonactive"), UIBarButtonItemStyle.Plain, SwitchLayout);
            switchButton.TintColor = Constants.R231G72B0;

            if (Username == AppSettings.User.Login)
            {
                NavigationItem.Title = "My Profile";
                var settingsButton = new UIBarButtonItem(UIImage.FromBundle("ic_settings"), UIBarButtonItemStyle.Plain, GoToSettings);
                settingsButton.TintColor = Constants.R151G155B158;
                NavigationItem.RightBarButtonItems = new UIBarButtonItem[] { settingsButton, switchButton };
            }
            else
            {
                NavigationItem.Title = Username;
                var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
                leftBarButton.TintColor = Constants.R15G24B30;
                NavigationItem.LeftBarButtonItem = leftBarButton;
                var settingsButton = new UIBarButtonItem(UIImage.FromBundle("ic_more"), UIBarButtonItemStyle.Plain, ShowPushSetting);
                settingsButton.TintColor = Constants.R151G155B158;
                NavigationItem.RightBarButtonItems = new UIBarButtonItem[] { settingsButton, switchButton };
            }
        }

        protected override void SameTabTapped()
        {
            collectionView.SetContentOffset(new CGPoint(0, -_profileHeader.View.Frame.Height), true);
        }

        async void RefreshControl_ValueChanged(object sender, EventArgs e)
        {
            await RefreshPage();
            _refreshControl.EndRefreshing();
        }

        protected override void SourceChanged(Status status)
        {
            if (sliderCollection.Hidden)
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

            var avatarTap = new UITapGestureRecognizer(() =>
            {
                if (isPowerOpen || Username != AppSettings.User.Login)
                    return;

                UIView.Animate(0.3f, 0f, UIViewAnimationOptions.CurveEaseOut, () =>
                 {
                     isPowerOpen = true;
                     powerPopup.Frame = new CGRect(new CGPoint(powerPopup.Frame.X, 0), powerPopup.Frame.Size);
                 }, () =>
                 {
                     UIView.Animate(0.2f, 2f, UIViewAnimationOptions.CurveEaseIn, () =>
                     {
                         powerPopup.Frame = new CGRect(new CGPoint(powerPopup.Frame.X, -NavigationController.NavigationBar.Frame.Bottom), powerPopup.Frame.Size);
                     }, () =>
                     {
                         isPowerOpen = false;
                     });
                 });
            });
            _profileHeader.Avatar.AddGestureRecognizer(avatarTap);
        }

        public override void ViewWillAppear(bool animated)
        {
            if (!IsMovingToParentViewController)
                collectionView.ReloadData();
            base.ViewWillAppear(animated);
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
                    if (post.Author == AppSettings.User.Login)
                        return;
                    var myViewController = new ProfileViewController();
                    myViewController.Username = post.Author;
                    NavigationController.PushViewController(myViewController, true);
                    break;
                case ActionType.Preview:
                    if (collectionView.Hidden)
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

        private async Task RefreshPage()
        {
            GetUserInfo();
            await GetPosts(clearOld: true);
        }

        public async Task<UserProfileResponse> GetUserInfo()
        {
            if (errorMessage == null)
                return _userData;
            _userDataLoaded = false;
            errorMessage.Hidden = true;
            try
            {
                var error = await _presenter.TryGetUserInfo(Username);
                _refreshControl.EndRefreshing();

                if (error == null)
                {
                    _userData = _presenter.UserProfileResponse;

                    if (Username == AppSettings.User.Login)
                        _profileHeader.PowerFrame.ChangePercents((int)_userData.VotingPower);
                    else
                        _profileHeader.PowerFrame.ChangePercents(0);

                    if (powerText != null)
                        powerText.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PowerOfLike, _userData.VotingPower);

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

                    if (!string.IsNullOrEmpty(_userData.ProfileImage.GetProxy(300, 300)))
                        ImageService.Instance.LoadUrl(_userData.ProfileImage, TimeSpan.FromDays(30))
                                             .FadeAnimation(false, false, 0)
                                             .LoadingPlaceholder("ic_noavatar.png")
                                             .ErrorPlaceholder("ic_noavatar.png").Error((f) =>
                    {
                        ImageService.Instance.LoadUrl(_userData.ProfileImage, TimeSpan.FromDays(30))
                                             .FadeAnimation(false, false, 0)
                                             .LoadingPlaceholder("ic_noavatar.png")
                                             .ErrorPlaceholder("ic_noavatar.png")
                                             .DownSample(width: (int)300)
                                             .Into(_profileHeader.Avatar);
                    }).Into(_profileHeader.Avatar);
                    else
                        _profileHeader.Avatar.Image = UIImage.FromBundle("ic_noavatar");

                    var buttonsAttributes = new UIStringAttributes
                    {
                        Font = Constants.Semibold20,
                        ForegroundColor = Constants.R15G24B30,
                    };

                    var textAttributes = new UIStringAttributes
                    {
                        Font = Constants.Regular12,
                        ForegroundColor = Constants.R151G155B158,
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
                        if (collectionView.Hidden)
                            collectionView.ContentOffset = new CGPoint(0, -size.Height);
                        collectionView.Hidden = false;
                    }
                }
                return _userData;
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
            return _userData;
        }

        private void GoToSettings(object sender, EventArgs e)
        {
            var myViewController = new SettingsViewController();
            TabBarController.NavigationController.PushViewController(myViewController, true);
        }

        private void ShowPushSetting(object sender, EventArgs e)
        {
            var actionSheetAlert = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            if (UserIsWatched)
            {
                actionSheetAlert.AddAction(UIAlertAction.Create(AppSettings.LocalizationManager.GetText(LocalizationKeys.UnwatchUser),
                                                                UIAlertActionStyle.Default, PushesOnClick));
            }
            else
            {
                actionSheetAlert.AddAction(UIAlertAction.Create(AppSettings.LocalizationManager.GetText(LocalizationKeys.WatchUser),
                                                                UIAlertActionStyle.Default, PushesOnClick));
            }

            actionSheetAlert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
            PresentViewController(actionSheetAlert, true, null);
        }

        private async void PushesOnClick(object sender)
        {
            var model = new PushNotificationsModel(AppSettings.User.UserInfo, !UserIsWatched)
            {
                WatchedUser = Username
            };
            var response = await BasePresenter.TrySubscribeForPushes(model);
            if (response.IsSuccess)
            {
                if (UserIsWatched)
                    AppSettings.User.WatchedUsers.Remove(Username);
                else
                    AppSettings.User.WatchedUsers.Add(Username);
            }
        }

        private void SwitchLayout(object sender, EventArgs e)
        {
            _gridDelegate.IsGrid = _collectionViewSource.IsGrid = !_collectionViewSource.IsGrid;
            if (_collectionViewSource.IsGrid)
            {
                switchButton.Image = UIImage.FromBundle("ic_grid_active");
                collectionView.SetCollectionViewLayout(new UICollectionViewFlowLayout()
                {
                    MinimumLineSpacing = 1,
                    MinimumInteritemSpacing = 1,
                }, false);
            }
            else
            {
                switchButton.Image = UIImage.FromBundle("ic_grid_nonactive");
                collectionView.SetCollectionViewLayout(new UICollectionViewFlowLayout()
                {
                    MinimumLineSpacing = 0,
                    MinimumInteritemSpacing = 0,
                }, false);
            }

            collectionView.ReloadData();
            collectionView.ContentOffset = new CGPoint(0, -_profileHeader.View.Frame.Height);
        }

        protected override async Task GetPosts(bool shouldStartAnimating = true, bool clearOld = false)
        {
            if (_isPostsLoading)
                return;
            _isPostsLoading = true;

            if (clearOld)
            {
                _presenter.Clear();
                _gridDelegate.ClearPosition();
                _sliderGridDelegate.ClearPosition();
            }

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

        private async Task Follow()
        {
            _profileHeader.DecorateFollowButton(null, Username);
            var error = await _presenter.TryFollow();

            if (error == null)
                _profileHeader.DecorateFollowButton(_userData.HasFollowed, Username);
            else
                ShowAlert(error);
        }
    }
}
