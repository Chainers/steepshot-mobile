using System;
using System.Threading.Tasks;
using CoreGraphics;
using PureLayout.Net;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using Steepshot.iOS.Delegates;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;
using Steepshot.iOS.Popups;

namespace Steepshot.iOS.Views
{
    public partial class ProfileViewController : BasePostController<UserProfilePresenter>
    {
        private UserProfileResponse _userData;
        private FeedCollectionViewSource _collectionViewSource;
        private UIRefreshControl _refreshControl;
        private bool _isPostsLoading;
        private UINavigationController _navController;
        private readonly UIBarButtonItem _switchButton = new UIBarButtonItem();
        private readonly UIBarButtonItem _leftBarButton = new UIBarButtonItem();
        private readonly UIBarButtonItem _settingsButton = new UIBarButtonItem();
        private bool _userDataLoaded;
        private UIView powerPopup;
        private UILabel powerText;
        private bool isPowerOpen;
        private bool UserIsWatched => AppDelegate.User.WatchedUsers.Contains(Username);

        public string Username = AppDelegate.User.Login;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Presenter.UserName = Username;
            _navController = TabBarController != null ? TabBarController.NavigationController : NavigationController;

            FeedCollection = collectionView;
            SliderCollection = sliderCollection;

            collectionView.RegisterClassForCell(typeof(ProfileHeaderViewCell), nameof(ProfileHeaderViewCell));
            collectionView.RegisterClassForCell(typeof(LoaderCollectionCell), nameof(LoaderCollectionCell));
            collectionView.RegisterClassForCell(typeof(NewFeedCollectionViewCell), nameof(NewFeedCollectionViewCell));
            collectionView.RegisterClassForCell(typeof(PhotoCollectionViewCell), nameof(PhotoCollectionViewCell));

            collectionView.SetCollectionViewLayout(new UICollectionViewFlowLayout()
            {
                MinimumLineSpacing = 0,
                MinimumInteritemSpacing = 0,
            }, false);

            FeedCollectionViewDelegate = new ProfileCollectionViewFlowDelegate(collectionView, Presenter);
            FeedCollectionViewDelegate.IsGrid = false;
            FeedCollectionViewDelegate.IsProfile = true;
            
            _collectionViewSource = new ProfileCollectionViewSource(Presenter, FeedCollectionViewDelegate);

            collectionView.Source = _collectionViewSource;
            collectionView.Delegate = FeedCollectionViewDelegate;

            SliderCollectionViewDelegate = new SliderCollectionViewFlowDelegate(sliderCollection, Presenter);

            SliderViewSource = new SliderCollectionViewSource(Presenter, SliderCollectionViewDelegate);

            sliderCollection.DecelerationRate = UIScrollView.DecelerationRateFast;
            sliderCollection.ShowsHorizontalScrollIndicator = false;

            sliderCollection.SetCollectionViewLayout(new SliderFlowLayout()
            {
                MinimumLineSpacing = 10,
                MinimumInteritemSpacing = 0,
                ScrollDirection = UICollectionViewScrollDirection.Horizontal,
                SectionInset = new UIEdgeInsets(0, 15, 0, 15),
            }, false);

            sliderCollection.Source = SliderViewSource;
            sliderCollection.RegisterClassForCell(typeof(LoaderCollectionCell), nameof(LoaderCollectionCell));
            sliderCollection.RegisterClassForCell(typeof(SliderFeedCollectionViewCell), nameof(SliderFeedCollectionViewCell));
            sliderCollection.Delegate = SliderCollectionViewDelegate;

            _refreshControl = new UIRefreshControl();
            collectionView.Add(_refreshControl);

            SetBackButton();

            if (Username == AppDelegate.User.Login && AppDelegate.User.HasPostingPermission)
                SetVotePowerView();
            GetUserInfo();
            GetPosts();
        }

        public override void ViewWillAppear(bool animated)
        {
            SliderAction += ProfileViewController_SliderAction;
            if (!IsMovingToParentViewController)
                HandleAction(new Status());
            else
            {
                Presenter.SourceChanged += SourceChanged;
                FeedCollectionViewDelegate.ScrolledToBottom += ScrolledToBottom;
                FeedCollectionViewDelegate.CellClicked += CellAction;
                _collectionViewSource.CellAction += CellAction;
                _collectionViewSource.TagAction += TagAction;
                _collectionViewSource.ProfileAction += ProfileAction;
                SliderCollectionViewDelegate.ScrolledToBottom += ScrolledToBottom;
                SliderViewSource.CellAction += CellAction;
                SliderViewSource.TagAction += TagAction;
                _refreshControl.ValueChanged += RefreshControl_ValueChanged;
                _switchButton.Clicked += SwitchLayout;
                if (Username == AppDelegate.User.Login)
                    _settingsButton.Clicked += GoToSettings;
                else
                {
                    _leftBarButton.Clicked += GoBack;
                    _settingsButton.Clicked += ShowPushSetting;
                }
            }

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

                PostCreatedPopup.Show(View, AppDelegate.Localization.GetText(LocalizationKeys.PostDelay));
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            StopPlayingVideo(sliderCollection, collectionView);
            SliderAction = null;

            if (IsMovingFromParentViewController)
            {
                Presenter.SourceChanged -= SourceChanged;
                FeedCollectionViewDelegate.ScrolledToBottom = null;
                FeedCollectionViewDelegate.CellClicked = null;
                _collectionViewSource.CellAction -= CellAction;
                _collectionViewSource.TagAction -= TagAction;
                _collectionViewSource.ProfileAction -= ProfileAction;
                SliderCollectionViewDelegate.ScrolledToBottom = null;
                SliderViewSource.CellAction -= CellAction;
                SliderViewSource.TagAction -= TagAction;
                _refreshControl.ValueChanged -= RefreshControl_ValueChanged;
                _switchButton.Clicked -= SwitchLayout;
                if (Username == AppDelegate.User.Login)
                    _settingsButton.Clicked -= GoToSettings;
                else
                {
                    _leftBarButton.Clicked -= GoBack;
                    _settingsButton.Clicked -= ShowPushSetting;
                }
                _collectionViewSource.FreeAllCells();
                SliderViewSource.FreeAllCells();
                Presenter.TasksCancel();
            }
            if (TabBarController != null)
                ((MainTabBarController)TabBarController).SameTabTapped -= SameTabTapped;
            base.ViewWillDisappear(animated);
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
            _switchButton.Image = UIImage.FromBundle("ic_grid_nonactive");
            _switchButton.TintColor = Constants.R231G72B0;

            if (Username == AppDelegate.User.Login)
            {
                NavigationItem.Title = "My Profile";
                _settingsButton.Image = UIImage.FromBundle("ic_settings");
                _settingsButton.TintColor = Constants.R151G155B158;
                NavigationItem.RightBarButtonItems = new UIBarButtonItem[] { _settingsButton, _switchButton };
            }
            else
            {
                NavigationItem.Title = Username;
                _leftBarButton.Image = UIImage.FromBundle("ic_back_arrow");
                _leftBarButton.TintColor = Constants.R15G24B30;
                NavigationItem.LeftBarButtonItem = _leftBarButton;

                _settingsButton.Image = UIImage.FromBundle("ic_more");
                _settingsButton.Enabled = AppDelegate.User.HasPostingPermission;
                _settingsButton.TintColor = Constants.R151G155B158;
                NavigationItem.RightBarButtonItems = new UIBarButtonItem[] { _settingsButton, _switchButton };
            }
        }

        private void ProfileViewController_SliderAction(bool isOpening)
        {
            if (!sliderCollection.Hidden)
                sliderCollection.ScrollEnabled = !isOpening;
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
                case ActionType.Balance:
                    if (_userData.Username == AppDelegate.User.Login && TabBarController != null)
                        TabBarController.NavigationController.PushViewController(new WalletViewController(), true);
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

        private async void RefreshControl_ValueChanged(object sender, EventArgs e)
        {
            await Presenter.TryUpdateUserPostsAsync(AppDelegate.User.Login);

            await RefreshPage();
            _refreshControl.EndRefreshing();
        }

        protected override void SourceChanged(Status status)
        {
            if (status.Sender == nameof(Presenter.TryDeletePostAsync))
                StopPlayingVideo(sliderCollection, collectionView);
            InvokeOnMainThread(() => HandleAction(status));
        }

        private void HandleAction(Status status)
        {

            if(status.Sender == nameof(Presenter.TryGetUserInfoAsync))
                FeedCollectionViewDelegate.UpdateProfile();

            if (sliderCollection.Hidden)
            {
                FeedCollectionViewDelegate.GenerateVariables();
                collectionView.ReloadData();
            }
            else
            {
                SliderCollectionViewDelegate.GenerateVariables();
                sliderCollection.ReloadData();
            }
        }

        private void ShowPowerPopup()
        {
            if (isPowerOpen || Username != AppDelegate.User.Login)
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

        private void CellAction(ActionType type, Post post)
        {
            switch (type)
            {
                case ActionType.Profile:
                    if (post.Author == AppDelegate.User.Login)
                        return;
                    var myViewController = new ProfileViewController();
                    myViewController.Username = post.Author;
                    NavigationController.PushViewController(myViewController, true);
                    break;
                case ActionType.Preview:
                    if (collectionView.Hidden)
                        NavigationController.PushViewController(new ImagePreviewViewController(post.Media[post.PageIndex].Url) { HidesBottomBarWhenPushed = true }, true);
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
                    NavigationController.PushViewController(new CommentsViewController(post) { HidesBottomBarWhenPushed = true }, true);
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

        public async Task GetUserInfo()
        {
            //if (errorMessage == null)
                //return _userData;
            _userDataLoaded = false;
            errorMessage.Hidden = true;
            try
            {
                var result = await Presenter.TryGetUserInfoAsync(Username);
                _refreshControl.EndRefreshing();

                if (result.IsSuccess)
                {
                    _userData = Presenter.UserProfileResponse;
                    if (_userData.IsSubscribed)
                    {
                        if (!AppDelegate.User.WatchedUsers.Contains(_userData.Username))
                            AppDelegate.User.WatchedUsers.Add(_userData.Username);
                    }
                    else
                        AppDelegate.User.WatchedUsers.Remove(_userData.Username);

                    if (powerText != null)
                        powerText.Text = AppDelegate.Localization.GetText(LocalizationKeys.Mana, _userData.VotingPower);

                    if (!_refreshControl.Refreshing)
                    {
                        collectionView.ContentInset = new UIEdgeInsets(0, 0, 0, 0);

                        if (collectionView.Hidden)
                            collectionView.ContentOffset = new CGPoint(0, 0);
                        collectionView.Hidden = false;
                    }
                }
                //return _userData;
            }
            catch (Exception ex)
            {
                errorMessage.Hidden = false;
                AppDelegate.Logger.ErrorAsync(ex);
            }
            finally
            {
                _userDataLoaded = true;
                loading.StopAnimating();
            }
            //return _userData;
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
                actionSheetAlert.AddAction(UIAlertAction.Create(AppDelegate.Localization.GetText(LocalizationKeys.UnwatchUser),
                                                                UIAlertActionStyle.Default, PushesOnClick));
            }
            else
            {
                actionSheetAlert.AddAction(UIAlertAction.Create(AppDelegate.Localization.GetText(LocalizationKeys.WatchUser),
                                                                UIAlertActionStyle.Default, PushesOnClick));
            }

            actionSheetAlert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
            PresentViewController(actionSheetAlert, true, null);
        }

        private async void PushesOnClick(object sender)
        {
            var model = new PushNotificationsModel(AppDelegate.User.UserInfo, !UserIsWatched)
            {
                WatchedUser = Username
            };
            var response = await Presenter.TrySubscribeForPushesAsync(model);
            if (response.IsSuccess)
            {
                if (UserIsWatched)
                    AppDelegate.User.WatchedUsers.Remove(Username);
                else
                    AppDelegate.User.WatchedUsers.Add(Username);
            }
        }

        private void SwitchLayout(object sender, EventArgs e)
        {
            FeedCollectionViewDelegate.IsGrid = _collectionViewSource.IsGrid = !_collectionViewSource.IsGrid;
            if (_collectionViewSource.IsGrid)
            {
                _switchButton.Image = UIImage.FromBundle("ic_grid_active");
                collectionView.SetCollectionViewLayout(new UICollectionViewFlowLayout()
                {
                    MinimumLineSpacing = 1,
                    MinimumInteritemSpacing = 1,
                }, false);

                foreach (var item in collectionView.IndexPathsForVisibleItems)
                {
                    if (collectionView.CellForItem(item) is NewFeedCollectionViewCell cell)
                        cell.Cell.Playback(false);
                }
            }
            else
            {
                _switchButton.Image = UIImage.FromBundle("ic_grid_nonactive");
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
                Presenter.Clear();
                FeedCollectionViewDelegate.ClearPosition();
                SliderCollectionViewDelegate.ClearPosition();
            }

            var exception = await Presenter.TryLoadNextPostsAsync();

            if (exception == null)
            {
                _isPostsLoading = false;
            }
            else
            {
                ShowAlert(exception);
            }

            if (_userDataLoaded)
            {
                loading.StopAnimating();
                collectionView.Hidden = false;
            }
        }

        private async Task Follow()
        {
            FeedCollectionViewDelegate.profileCell.DecorateFollowButton();
            var result = await Presenter.TryFollowAsync();

            if (result.IsSuccess)
                FeedCollectionViewDelegate.profileCell.DecorateFollowButton();
            else
                ShowAlert(result);
        }
    }
}
