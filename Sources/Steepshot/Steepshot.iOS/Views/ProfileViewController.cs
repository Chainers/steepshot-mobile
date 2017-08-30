﻿using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using FFImageLoading;
using Foundation;
using Steepshot.Core.Models.Common;
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
    public partial class ProfileViewController : BaseViewController
    {
        protected ProfileViewController(IntPtr handle) : base(handle) { }

        public ProfileViewController()
        {
        }

        protected override void CreatePresenter()
        {
            _presenter = new UserProfilePresenter(Username);
        }

        UserProfilePresenter _presenter;
        private UserProfileResponse _userData;
        public string Username = BasePresenter.User.Login;
        private ProfileCollectionViewSource _collectionViewSource = new ProfileCollectionViewSource();
        private UIRefreshControl _refreshControl;
        private bool _isPostsLoading;
        private ProfileHeaderViewController _profileHeader;
        private CollectionViewFlowDelegate _gridDelegate;
        private int _lastRow;
        private UINavigationController _navController;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            if (TabBarController != null)
                TabBarController.NavigationController.NavigationBarHidden = true;
            if (Username != BasePresenter.User.Login)
                topViewHeight.Constant = 0;

            _navController = TabBarController != null ? TabBarController.NavigationController : NavigationController;
            _collectionViewSource.PhotoList = _presenter.Posts;
            _collectionViewSource.Voted += (vote, postUri, success) => Vote(vote, postUri, success);
            _collectionViewSource.Flagged += (vote, url, action) => Flagged(vote, url, action);
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
            _collectionViewSource.ImagePreview += PreviewPhoto;

            collectionView.RegisterClassForCell(typeof(PhotoCollectionViewCell), nameof(PhotoCollectionViewCell));
            collectionView.RegisterNibForCell(UINib.FromName(nameof(PhotoCollectionViewCell), NSBundle.MainBundle), nameof(PhotoCollectionViewCell));
            collectionView.RegisterClassForCell(typeof(FeedCollectionViewCell), nameof(FeedCollectionViewCell));
            collectionView.RegisterNibForCell(UINib.FromName(nameof(FeedCollectionViewCell), NSBundle.MainBundle), nameof(FeedCollectionViewCell));
            //collectioViewFlowLayout.EstimatedItemSize = Constants.CellSize;
            collectionView.Source = _collectionViewSource;

            _gridDelegate = new CollectionViewFlowDelegate((indexPath) =>
            {
                var collectionCell = (PhotoCollectionViewCell)collectionView.CellForItem(indexPath);
                PreviewPhoto(collectionCell.Image, collectionCell.ImageUrl);
            },
            () =>
            {
                if (collectionView.IndexPathsForVisibleItems.Count() != 0)
                {
                    var newlastRow = collectionView.IndexPathsForVisibleItems.Max(c => c.Row) + 2;

                    if (_collectionViewSource.PhotoList.Count <= _lastRow && !_refreshControl.Refreshing)
                        GetUserPosts();
                    _lastRow = newlastRow;
                }
            }, _collectionViewSource.FeedStrings);

            collectionView.Delegate = _gridDelegate;

            _profileHeader = new ProfileHeaderViewController(ProfileHeaderLoaded);
            collectionView.ContentInset = new UIEdgeInsets(300, 0, 0, 0);
            collectionView.AddSubview(_profileHeader.View);

            _refreshControl = new UIRefreshControl();
            _refreshControl.ValueChanged += RefreshControl_ValueChanged;
            collectionView.Add(_refreshControl);

            GetUserInfo();
            GetUserPosts();
        }

        async void RefreshControl_ValueChanged(object sender, EventArgs e)
        {
            await RefreshPage();
            _refreshControl.EndRefreshing();
        }

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
        }

        private void ProfileHeaderLoaded()
        {
            _profileHeader.SwitchButton.TouchDown += (sender, e) =>
            {
                if (!_collectionViewSource.IsGrid)
                {
                    //collectioViewFlowLayout.EstimatedItemSize = Constants.CellSize;
                    _profileHeader.SwitchButton.SetImage(UIImage.FromFile("list.png"), UIControlState.Normal);
                }
                else
                {
                    //collectioViewFlowLayout.EstimatedItemSize = new CGSize(UIScreen.MainScreen.Bounds.Width, 485);
                    _profileHeader.SwitchButton.SetImage(UIImage.FromFile("grid.png"), UIControlState.Normal);

                }
                _gridDelegate.IsGrid = _collectionViewSource.IsGrid = !_collectionViewSource.IsGrid;
                collectionView.ReloadData();
            };

            _profileHeader.FollowButton.TouchDown += (object sender, EventArgs e) =>
            {
                Follow();
            };

            _profileHeader.SettingsButton.TouchDown += (sender, e) =>
            {
                var myViewController = new SettingsViewController();
                TabBarController.NavigationController.PushViewController(myViewController, true);
            };

            _profileHeader.FollowingButton.TouchDown += (sender, e) =>
            {
                var myViewController = new FollowViewController();
                myViewController.Username = Username;
                myViewController.FriendsType = FriendsType.Following;
                NavigationController.PushViewController(myViewController, true);
            };

            _profileHeader.FollowersButton.TouchDown += (sender, e) =>
            {
                var myViewController = new FollowViewController();
                myViewController.Username = Username;
                myViewController.FriendsType = FriendsType.Followers;
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

        private async Task RefreshPage()
        {
            _collectionViewSource.FeedStrings.Clear();
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
            errorMessage.Hidden = true;
            try
            {
                var response = await _presenter.GetUserInfo(Username);
                if (response.Success)
                {
                    _userData = response.Result;
                    _profileHeader.Username.Text = !string.IsNullOrEmpty(_userData.Name) ? _userData.Name : _userData.Username;
                    var culture = new CultureInfo("en-US");
                    _profileHeader.Date.Text = $"Joined {_userData.Created.ToString("Y", culture)}";
                    if (!string.IsNullOrEmpty(_userData.Location))
                        _profileHeader.Location.Text = _userData.Location;
                    if (!string.IsNullOrEmpty(_userData.About))
                        _profileHeader.DescriptionLabel.Text = _userData.About;

                    if (!string.IsNullOrEmpty(_userData.ProfileImage))
                        ImageService.Instance.LoadUrl(_userData.ProfileImage, TimeSpan.FromDays(30))
                                             .Retry(2, 200)
                                             .FadeAnimation(false, false, 0)
                                             .DownSample(width: (int)_profileHeader.Avatar.Frame.Width)
                                             .Into(_profileHeader.Avatar);
                    else
                        _profileHeader.Avatar.Image = UIImage.FromBundle("ic_user_placeholder");

                    _profileHeader.Balance.SetTitle($"{_userData.EstimatedBalance.ToString()}{Currency}", UIControlState.Normal);
                    _profileHeader.SettingsButton.Hidden = Username != BasePresenter.User.Login;

                    var buttonsAttributes = new UIStringAttributes
                    {
                        Font = Constants.Bold12,
                        ForegroundColor = UIColor.FromRGB(51, 51, 51),
                        ParagraphStyle = new NSMutableParagraphStyle() { LineSpacing = 5, Alignment = UITextAlignment.Center }
                    };

                    var textAttributes = new UIStringAttributes
                    {
                        Font = Constants.Bold9,
                        ForegroundColor = UIColor.FromRGB(153, 153, 153),
                        ParagraphStyle = new NSMutableParagraphStyle() { LineSpacing = 5, Alignment = UITextAlignment.Center }
                    };

                    NSMutableAttributedString photosString = new NSMutableAttributedString();
                    photosString.Append(new NSAttributedString(_userData.PostCount.ToString(), buttonsAttributes));
                    photosString.Append(new NSAttributedString(Environment.NewLine));
                    photosString.Append(new NSAttributedString("PHOTOS", textAttributes));

                    _profileHeader.PhotosButton.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
                    _profileHeader.PhotosButton.TitleLabel.TextAlignment = UITextAlignment.Center;
                    _profileHeader.PhotosButton.SetAttributedTitle(photosString, UIControlState.Normal);

                    NSMutableAttributedString followingString = new NSMutableAttributedString();
                    followingString.Append(new NSAttributedString(_userData.FollowingCount.ToString(), buttonsAttributes));
                    followingString.Append(new NSAttributedString(Environment.NewLine));
                    followingString.Append(new NSAttributedString("FOLLOWING", textAttributes));

                    _profileHeader.FollowingButton.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
                    _profileHeader.FollowingButton.TitleLabel.TextAlignment = UITextAlignment.Center;
                    _profileHeader.FollowingButton.SetAttributedTitle(followingString, UIControlState.Normal);

                    NSMutableAttributedString followersString = new NSMutableAttributedString();
                    followersString.Append(new NSAttributedString(_userData.FollowersCount.ToString(), buttonsAttributes));
                    followersString.Append(new NSAttributedString(Environment.NewLine));
                    followersString.Append(new NSAttributedString("FOLLOWERS", textAttributes));

                    _profileHeader.FollowersButton.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
                    _profileHeader.FollowersButton.TitleLabel.TextAlignment = UITextAlignment.Center;
                    _profileHeader.FollowersButton.SetAttributedTitle(followersString, UIControlState.Normal);

                    ToogleFollowButton();

                    if (!_refreshControl.Refreshing)
                    {
                        _profileHeader.View.SetNeedsLayout();
                        _profileHeader.View.LayoutIfNeeded();
                        var size = _profileHeader.View.SystemLayoutSizeFittingSize(new CGSize(UIScreen.MainScreen.Bounds.Width, 300));

                        _profileHeader.View.Frame = new CGRect(0, -size.Height, UIScreen.MainScreen.Bounds.Width, size.Height);
                        collectionView.ContentInset = new UIEdgeInsets(size.Height, 0, 0, 0);
                        collectionView.Hidden = false;
                    }
                }
                else
                {
                    Reporter.SendCrash(response.Errors[0], BasePresenter.User.Login, AppVersion);
                }
            }
            catch (Exception ex)
            {
                errorMessage.Hidden = false;
                Reporter.SendCrash(ex, BasePresenter.User.Login, AppVersion);
            }
            finally
            {
                loading.StopAnimating();
            }
        }

        public async Task GetUserPosts(bool needRefresh = false)
        {
            if (_isPostsLoading)
                return;
            _isPostsLoading = true;

            await _presenter.GetUserPosts(needRefresh).ContinueWith((p) =>
            {
                foreach (var r in _presenter.Posts)
                {
                    var at = new NSMutableAttributedString();
                    at.Append(new NSAttributedString(r.Author, Constants.NicknameAttribute));
                    at.Append(new NSAttributedString($" {r.Title}"));
                    _collectionViewSource.FeedStrings.Add(at);
                }
                InvokeOnMainThread(() =>
                {
                    collectionView.ReloadData();
                    collectionView.CollectionViewLayout.InvalidateLayout();
                });
                _isPostsLoading = false;
            });
        }


        private async Task Vote(bool vote, string postUri, Action<string, OperationResult<VoteResponse>> success)
        {
            if (!BasePresenter.User.IsAuthenticated)
            {
                LoginTapped();
                return;
            }

            try
            {
                var voteResponse = await _presenter.Vote(_presenter.Posts.FindIndex(p => p.Url == postUri));
                if (voteResponse.Success)
                {
                    collectionView.ReloadData();
                    /*
                    var u = _presenter.Posts.FirstOrDefault(p => p.Url == postUri);
                    if (u != null)
                    {
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
                    }*/
                }
                else
                    ShowAlert(voteResponse.Errors[0]);
                
                //success.Invoke(postUri, voteResponse);
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex, BasePresenter.User.Login, AppVersion);
            }
        }

        private void Flagged(bool vote, string postUrl, Action<string, OperationResult<FlagResponse>> action)
        {
            if (!BasePresenter.User.IsAuthenticated)
            {
                LoginTapped();
                return;
            }
            UIAlertController actionSheetAlert = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            actionSheetAlert.AddAction(UIAlertAction.Create("Flag photo", UIAlertActionStyle.Default, (obj) => FlagPhoto(vote, postUrl, action)));
            actionSheetAlert.AddAction(UIAlertAction.Create("Hide photo", UIAlertActionStyle.Default, (obj) => HidePhoto(postUrl)));
            actionSheetAlert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, (obj) => action.Invoke(postUrl, new OperationResult<FlagResponse>())));
            PresentViewController(actionSheetAlert, true, null);
        }

        private void HidePhoto(string url)
        {
            try
            {
                BasePresenter.User.PostBlacklist.Add(url);
                BasePresenter.User.Save();
                var postToHide = _collectionViewSource.PhotoList.FirstOrDefault(p => p.Url == url);
                if (postToHide != null)
                {
                    var postIndex = _collectionViewSource.PhotoList.IndexOf(postToHide);
                    _collectionViewSource.PhotoList.Remove(postToHide);
                    _collectionViewSource.FeedStrings.RemoveAt(postIndex);
                    collectionView.ReloadData();
                    collectionView.CollectionViewLayout.InvalidateLayout();
                }
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex, BasePresenter.User.Login, AppVersion);
            }
        }

        private async Task FlagPhoto(bool vote, string postUrl, Action<string, OperationResult<FlagResponse>> action)
        {
            try
            {
                //var flagRequest = new FlagRequest(BasePresenter.User.UserInfo, vote, postUrl);
                //var flagResponse = await Api.Flag(flagRequest);
                var flagResponse = await _presenter.FlagPhoto(_presenter.Posts.FindIndex(p => p.Url == postUrl));
                if (flagResponse.Success)
                {
                    collectionView.ReloadData();
                    /*
                    var u = _collectionViewSource.PhotoList.FirstOrDefault(p => p.Url == postUrl);
                    if (u != null)
                    {
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
                    }*/
                }
                else
                    ShowAlert(flagResponse.Errors[0]);
                
                //action.Invoke(postUrl, flagResponse);
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex, BasePresenter.User.Login, AppVersion);
            }
        }

        public async Task Follow()
        {
            //var request = new FollowRequest(BasePresenter.User.UserInfo, (_userData.HasFollowed == 0) ? FollowType.Follow : FollowType.UnFollow, _userData.Username);
            var resp = await _presenter.Follow(_userData.HasFollowed);
            if (resp.Errors.Count == 0)
            {
                _userData.HasFollowed = (resp.Result.IsFollowed) ? 1 : 0;
                ToogleFollowButton();
            }
            else
                ShowAlert(resp.Errors[0]);
        }

        void LoginTapped()
        {
            var myViewController = new PreLoginViewController();
            NavigationController.PushViewController(myViewController, true);
        }

        private void ToogleFollowButton()
        {
            if (!BasePresenter.User.IsAuthenticated || Username == BasePresenter.User.Login || Convert.ToBoolean(_userData.HasFollowed))
            {
                _profileHeader.FollowButtonWidth.Constant = 0;
                _profileHeader.FollowButtonMargin.Constant = 0;
            }
        }
    }
}

