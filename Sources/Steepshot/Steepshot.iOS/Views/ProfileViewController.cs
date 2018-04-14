﻿using System;
using System.Threading.Tasks;
using CoreGraphics;
using FFImageLoading;
using Foundation;
using PureLayout.Net;
using Steepshot.Core.Errors;
using Steepshot.Core.Localization;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
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
            _presenter.SourceChanged += SourceChanged;
        }

        private UserProfileResponse _userData;
        public string Username = BasePresenter.User.Login;
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

            _profileHeader = new ProfileHeaderViewController(ProfileHeaderLoaded);
            collectionView.ContentInset = new UIEdgeInsets(300, 0, 0, 0);
            collectionView.AddSubview(_profileHeader.View);

            _refreshControl = new UIRefreshControl();
            _refreshControl.ValueChanged += RefreshControl_ValueChanged;
            collectionView.Add(_refreshControl);

            if (TabBarController != null)
                ((MainTabBarController)TabBarController).SameTabTapped += SameTabTapped;
            SetBackButton();

            if(Username == BasePresenter.User.Login && BasePresenter.User.IsAuthenticated)
                SetVotePowerView();
            GetUserInfo();
            GetUserPosts();
        }

        private void SetVotePowerView()
        {
            powerPopup = new UIView();
            powerPopup.Frame = new CGRect(0, -NavigationController.NavigationBar.Frame.Bottom, UIScreen.MainScreen.Bounds.Width, NavigationController.NavigationBar.Frame.Bottom);

            var heart = new UIImageView();
            heart.Image = UIImage.FromBundle("ic_white_heart");
            powerPopup.AddSubview(heart);

            powerText = new UILabel();
            powerText.TextColor = UIColor.White;
            powerText.Font = Constants.Semibold14;
            powerPopup.AddSubview(powerText);

            heart.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, 20);
            heart.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            powerText.AutoAlignAxis(ALAxis.Horizontal, heart);
            powerText.AutoPinEdge(ALEdge.Left, ALEdge.Right, heart, 10f);
            Constants.CreateGradient(powerPopup, 0);

            NavigationController.View.AddSubview(powerPopup);
        }

        private void SetBackButton()
        {
            switchButton = new UIBarButtonItem(UIImage.FromBundle("ic_grid_nonactive"), UIBarButtonItemStyle.Plain, SwitchLayout);
            switchButton.TintColor = Constants.R231G72B0;

            if (Username == BasePresenter.User.Login)
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
                NavigationItem.RightBarButtonItem = switchButton;
            }
        }

        private void SameTabTapped()
        {
            collectionView.SetContentOffset(new CGPoint(0, -_profileHeader.View.Frame.Height), true);
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
                if (isPowerOpen || Username != BasePresenter.User.Login)
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
                    if (post.Author == BasePresenter.User.Login)
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

        private void TagAction(string tag)
        {
            var myViewController = new PreSearchViewController();
            myViewController.CurrentPostCategory = tag;
            _navController.PushViewController(myViewController, true);
        }

        private async Task RefreshPage()
        {
            GetUserInfo();
            await GetUserPosts(true);
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

                    if (Username == BasePresenter.User.Login)
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

                    if (!string.IsNullOrEmpty(_userData.ProfileImage))
                        ImageService.Instance.LoadUrl(_userData.ProfileImage, TimeSpan.FromDays(30))
                                             .FadeAnimation(false, false, 0)
                                             .LoadingPlaceholder("ic_noavatar.png")
                                             .ErrorPlaceholder("ic_noavatar.png")
                                             .DownSample(width: (int)_profileHeader.Avatar.Frame.Width)
                                             .Into(_profileHeader.Avatar);
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

        private async Task GetUserPosts(bool needRefresh = false)
        {
            if (_isPostsLoading)
                return;
            _isPostsLoading = true;

            if (needRefresh)
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

        private async void Vote(Post post)
        {
            if (!BasePresenter.User.IsAuthenticated)
            {
                LoginTapped();
                return;
            }

            if (post == null)
                return;

            var error = await _presenter.TryVote(post);
            if (error is CanceledError)
                return;

            ShowAlert(error);
            if (error == null)
                ((MainTabBarController)TabBarController)?.UpdateProfile();
        }

        private void Flagged(Post post)
        {
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

                _presenter.HidePost(post);

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
            if (!BasePresenter.User.IsAuthenticated)
            {
                LoginTapped();
                return;
            }

            if (post == null)
                return;

            var error = await _presenter.TryFlag(post);
            ShowAlert(error);
            if (error == null)
                ((MainTabBarController)TabBarController)?.UpdateProfile();
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

        private void LoginTapped()
        {
            var myViewController = new WelcomeViewController();
            NavigationController.PushViewController(myViewController, true);
        }
    }
}
