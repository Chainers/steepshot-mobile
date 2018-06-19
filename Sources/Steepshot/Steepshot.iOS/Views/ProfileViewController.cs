using System;
using System.Threading.Tasks;
using CoreGraphics;
using FFImageLoading;
using Foundation;
using PureLayout.Net;
using Steepshot.Core.Localization;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.Cells;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;
using Steepshot.Core.Extensions;

namespace Steepshot.iOS.Views
{
    public partial class ProfileViewController : BasePostController<UserProfilePresenter>, IPageCloser //BaseViewControllerWithPresenter<UserProfilePresenter>
    {
        private UserProfileResponse _userData;
        private ProfileCollectionViewSource _collectionViewSource;
        private UIRefreshControl _refreshControl;
        private bool _isPostsLoading;
        private CollectionViewFlowDelegate _gridDelegate;
        private SliderCollectionViewFlowDelegate _sliderGridDelegate;
        private UINavigationController _navController;
        private UIBarButtonItem switchButton;
        private bool _userDataLoaded;
        private UIView powerPopup;
        private UILabel powerText;
        private bool isPowerOpen;
        private bool UserIsWatched => AppSettings.User.WatchedUsers.Contains(Username);

        public string Username = AppSettings.User.Login;


        protected override void CreatePresenter()
        {
            _presenter = new UserProfilePresenter() { UserName = Username };
            _presenter.SourceChanged += SourceChanged;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _navController = TabBarController != null ? TabBarController.NavigationController : NavigationController;

            collectionView.RegisterClassForCell(typeof(ProfileHeaderViewCell), nameof(ProfileHeaderViewCell));
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
            _gridDelegate.IsProfile = true;
            _gridDelegate.ScrolledToBottom += ScrolledToBottom;
            _gridDelegate.CellClicked += CellAction;

            _collectionViewSource = new ProfileCollectionViewSource(_presenter, _gridDelegate);
            _collectionViewSource.CellAction += CellAction;
            _collectionViewSource.TagAction += TagAction;
            _collectionViewSource.ProfileAction += ProfileAction;
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

            _refreshControl = new UIRefreshControl();
            _refreshControl.ValueChanged += RefreshControl_ValueChanged;
            collectionView.Add(_refreshControl);

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
            if (NavigationController?.ViewControllers.Length == 1)
                collectionView.SetContentOffset(new CGPoint(0, 0), true);
        }

        private void ProfileAction(ActionType actionType)
        {
            switch (actionType)
            {
                case ActionType.Follow:
                    Follow();
                    break;
                case ActionType.Followers:
                    OpenFollowPage(FriendsType.Followers);
                    break;
                case ActionType.Following:
                    OpenFollowPage(FriendsType.Following);
                    break;
                case ActionType.ProfilePower:
                    ShowPowerPopup();
                    break;
                default:
                    break;
            }
        }

        private void OpenFollowPage(FriendsType type)
        {
            var myViewController = new FollowViewController(type, _userData);
            NavigationController.PushViewController(myViewController, true);
        }

        public override void ViewWillDisappear(bool animated)
        {
            if (TabBarController != null)
                ((MainTabBarController)TabBarController).SameTabTapped -= SameTabTapped;
            base.ViewWillDisappear(animated);
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

        private void ShowPowerPopup()
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
        }

        public override void ViewWillAppear(bool animated)
        {
            if (!IsMovingToParentViewController)
                collectionView.ReloadData();

            if (TabBarController != null)
                ((MainTabBarController)TabBarController).SameTabTapped += SameTabTapped;

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
                        OpenPost(post);
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
                    ClosePost();
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
                    _collectionViewSource.user = _userData;
                    _gridDelegate.UpdateProfile(_userData);

                    if (powerText != null)
                        powerText.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PowerOfLike, _userData.VotingPower);

                    //if (!string.IsNullOrEmpty(_userData.ProfileImage.GetProxy(300, 300)))
                    //    ImageService.Instance.LoadUrl(_userData.ProfileImage, TimeSpan.FromDays(30))
                    //                         .FadeAnimation(false, false, 0)
                    //                         .LoadingPlaceholder("ic_noavatar.png")
                    //                         .ErrorPlaceholder("ic_noavatar.png").Error((f) =>
                    //{
                    //    ImageService.Instance.LoadUrl(_userData.ProfileImage, TimeSpan.FromDays(30))
                    //                         .FadeAnimation(false, false, 0)
                    //                         .LoadingPlaceholder("ic_noavatar.png")
                    //                         .ErrorPlaceholder("ic_noavatar.png")
                    //                         .DownSample(width: (int)300)
                    //                         .Into(_profileHeader.Avatar);
                    //}).Into(_profileHeader.Avatar);
                    //else
                    //_profileHeader.Avatar.Image = UIImage.FromBundle("ic_noavatar");

                    if (!_refreshControl.Refreshing)
                    {
                        collectionView.ContentInset = new UIEdgeInsets(0, 0, 0, 0);

                        if (collectionView.Hidden)
                            collectionView.ContentOffset = new CGPoint(0, 0);
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
            _gridDelegate.profileCell.DecorateFollowButton(null, Username);
            var error = await _presenter.TryFollow();

            if (error == null)
                _gridDelegate.profileCell.DecorateFollowButton(_userData.HasFollowed, Username);
            else
                ShowAlert(error);
        }

        public void OpenPost(Post post)
        {
            collectionView.Hidden = true;
            sliderCollection.Hidden = false;
            _sliderGridDelegate.GenerateVariables();
            sliderCollection.ReloadData();
            var test = NSIndexPath.FromRowSection(_presenter.IndexOf(post), 0);
            sliderCollection.ScrollToItem(NSIndexPath.FromRowSection(_presenter.IndexOf(post), 0), UICollectionViewScrollPosition.CenteredHorizontally, false);
        }

        public bool ClosePost()
        {
            if (!sliderCollection.Hidden)
            {
                var visibleRect = new CGRect();
                visibleRect.Location = sliderCollection.ContentOffset;
                visibleRect.Size = sliderCollection.Bounds.Size;
                var visiblePoint = new CGPoint(visibleRect.GetMidX(), visibleRect.GetMidY());
                var index = sliderCollection.IndexPathForItemAtPoint(visiblePoint);

                collectionView.ScrollToItem(index, UICollectionViewScrollPosition.Top, false);
                collectionView.Hidden = false;
                sliderCollection.Hidden = true;
                _gridDelegate.GenerateVariables();
                collectionView.ReloadData();
                return true;
            }
            return false;
        }
    }
}
