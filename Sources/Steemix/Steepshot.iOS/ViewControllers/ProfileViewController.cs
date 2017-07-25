using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CoreGraphics;
using FFImageLoading;
using Foundation;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;
using UIKit;

namespace Steepshot.iOS
{
    public partial class ProfileViewController : BaseViewController
    {
        private UserProfileResponse _userData;
        public string Username;
        private readonly ProfileCollectionViewSource _collectionViewSource;
        private readonly List<Post> _photosList;
        private string _offsetUrl;
        private bool _hasItems = true;
        private UIRefreshControl _refreshControl;
        private bool _isPostsLoading;
        private ProfileHeaderViewController _profileHeader;
        private CollectionViewFlowDelegate _gridDelegate;
        private int _lastRow;
        private const int Limit = 40;


        protected ProfileViewController(IntPtr handle) : base(handle)
        {
            _collectionViewSource = new ProfileCollectionViewSource();
            _photosList = new List<Post>();
            Username = User.Login;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            if (TabBarController != null)
                TabBarController.NavigationController.NavigationBarHidden = true;

            _collectionViewSource.PhotoList = _photosList;
            _collectionViewSource.Voted += Vote;
            _collectionViewSource.Flagged += Flagged;
            _collectionViewSource.GoToComments += postUrl =>
            {
                var myViewController = Storyboard.InstantiateViewController(nameof(CommentsViewController)) as CommentsViewController;
                if (myViewController != null)
                {
                    myViewController.PostUrl = postUrl;
                    NavigationController.PushViewController(myViewController, true);
                }
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
                if (collectionView.IndexPathsForVisibleItems.Length != 0)
                {
                    var newlastRow = collectionView.IndexPathsForVisibleItems.Max(c => c.Row) + 2;

                    if (_collectionViewSource.PhotoList.Count <= _lastRow && _hasItems && !_refreshControl.Refreshing)
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

        void RefreshControl_ValueChanged(object sender, EventArgs e)
        {
            RefreshPage();
            _refreshControl.EndRefreshing();
        }

        public override void ViewWillAppear(bool animated)
        {
            if (Username == User.Login)
            {
                NavigationController.SetNavigationBarHidden(true, false);
                if (TabBarController != null)
                    TabBarController.NavigationController.SetNavigationBarHidden(true, false);
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

            _profileHeader.FollowButton.TouchDown += Follow;

            _profileHeader.SettingsButton.TouchDown += (sender, e) =>
            {
                var myViewController = Storyboard.InstantiateViewController(nameof(SettingsViewController)) as SettingsViewController;
                TabBarController.NavigationController.PushViewController(myViewController, true);
            };

            _profileHeader.FollowingButton.TouchDown += (sender, e) =>
            {
                var myViewController = Storyboard.InstantiateViewController(nameof(FollowViewController)) as FollowViewController;
                if (myViewController != null)
                {
                    myViewController.Username = Username;
                    myViewController.FriendsType = FriendsType.Following;
                    NavigationController.PushViewController(myViewController, true);
                }
            };

            _profileHeader.FollowersButton.TouchDown += (sender, e) =>
            {
                var myViewController = Storyboard.InstantiateViewController(nameof(FollowViewController)) as FollowViewController;
                if (myViewController != null)
                {
                    myViewController.Username = Username;
                    myViewController.FriendsType = FriendsType.Followers;
                    NavigationController.PushViewController(myViewController, true);
                }
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

        private void RefreshPage()
        {
            _photosList.Clear();
            _collectionViewSource.FeedStrings.Clear();
            _hasItems = true;
            GetUserInfo();
            GetUserPosts();
        }

        private void PreviewPhoto(UIImage image, string url)
        {
            var myViewController = Storyboard.InstantiateViewController(nameof(ImagePreviewViewController)) as ImagePreviewViewController;
            if (myViewController != null)
            {
                myViewController.ImageForPreview = image;
                myViewController.ImageUrl = url;
                NavigationController.PushViewController(myViewController, true);
            }
        }

        private async void GetUserInfo()
        {
            errorMessage.Hidden = true;
            try
            {
                var req = new UserProfileRequest(Username) { SessionId = User.SessionId };
                var response = await Api.GetUserProfile(req);
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

                    _profileHeader.Balance.SetTitle($"{_userData.EstimatedBalance}{Constants.Currency}", UIControlState.Normal);
                    _profileHeader.SettingsButton.Hidden = Username != User.Login;

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
                    Reporter.SendCrash(response.Errors[0]);
                }
            }
            catch (Exception ex)
            {
                errorMessage.Hidden = false;
                Reporter.SendCrash(ex);
            }
            finally
            {
                loading.StopAnimating();
            }
        }

        public async void GetUserPosts()
        {
            if (_isPostsLoading || !_hasItems)
                return;
            _isPostsLoading = true;
            try
            {
                var req = new UserPostsRequest(Username)
                {
                    Limit = Limit,
                    Offset = _photosList.Count == 0 ? "0" : _offsetUrl,
                    SessionId = User.SessionId
                };
                var response = await Api.GetUserPosts(req);
                if (response.Success)
                {
                    if (response.Result?.Results?.Count != 0)
                    {
                        response.Result?.Results?.FilterHided();
                        var lastItem = response.Result?.Results?.Last();
                        _offsetUrl = lastItem?.Url;

                        if (response.Result?.Results?.Count < Limit / 2)
                            _hasItems = false;
                        else
                            response.Result?.Results?.Remove(lastItem);

                        if (response.Result?.Results != null)
                        {
                            foreach (var r in response.Result?.Results)
                            {
                                var at = new NSMutableAttributedString();
                                at.Append(new NSAttributedString(r.Author, Constants.NicknameAttribute));
                                at.Append(new NSAttributedString($" {r.Title}"));
                                _collectionViewSource.FeedStrings.Add(at);
                            }
                            _photosList.AddRange(response.Result?.Results);
                        }
                    }
                    collectionView.ReloadData();
                    collectionView.CollectionViewLayout.InvalidateLayout();
                }
                else
                {
                    Reporter.SendCrash("Profile page get posts erorr: " + response.Errors[0]);
                    ShowAlert(response.Errors[0]);
                }
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex);
            }
            finally
            {
                _isPostsLoading = false;
            }
        }


        private async void Vote(bool vote, string postUri, Action<string, OperationResult<VoteResponse>> success)
        {
            if (!User.IsAuthenticated)
            {
                LoginTapped();
                return;
            }

            try
            {
                if (User.SessionId == null)
                {
                    var myViewController = Storyboard.InstantiateViewController(nameof(LoginViewController)) as LoginViewController;
                    NavigationController.PushViewController(myViewController, true);
                    return;
                }

                var voteRequest = new VoteRequest(User.SessionId, vote, postUri);
                var voteResponse = await Api.Vote(voteRequest);
                if (voteResponse.Success)
                {
                    var u = _photosList.FirstOrDefault(p => p.Url == postUri);
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
                    }
                }
                else
                {
                    Reporter.SendCrash("Profile page vote erorr: " + voteResponse.Errors[0]);
                    ShowAlert(voteResponse.Errors[0]);
                }
                success.Invoke(postUri, voteResponse);
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex);
            }
        }

        private void Flagged(bool vote, string postUrl, Action<string, OperationResult<FlagResponse>> action)
        {
            if (!User.IsAuthenticated)
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
                User.Postblacklist.Add(url);
                User.Save();
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
                Reporter.SendCrash(ex);
            }
        }

        private async void FlagPhoto(bool vote, string postUrl, Action<string, OperationResult<FlagResponse>> action)
        {
            try
            {
                var flagRequest = new FlagRequest(User.SessionId, vote, postUrl);
                var flagResponse = await Api.Flag(flagRequest);
                if (flagResponse.Success)
                {
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

        public async void Follow(object sender = null, EventArgs eventArgs = null)
        {
            var request = new FollowRequest(User.SessionId, (_userData.HasFollowed == 0) ? FollowType.Follow : FollowType.UnFollow, _userData.Username);
            var resp = await Api.Follow(request);
            if (resp.Errors.Count == 0)
            {
                _userData.HasFollowed = (resp.Result.IsFollowed) ? 1 : 0;
                ToogleFollowButton();
            }
            else
                Reporter.SendCrash("Profile page follow error: " + resp.Errors[0]);

        }

        void LoginTapped()
        {
            var myViewController = Storyboard.InstantiateViewController(nameof(PreLoginViewController)) as PreLoginViewController;
            NavigationController.PushViewController(myViewController, true);
        }

        private void ToogleFollowButton()
        {
            if (!User.IsAuthenticated || Username == User.Login || Convert.ToBoolean(_userData.HasFollowed))
            {
                _profileHeader.FollowButtonWidth.Constant = 0;
                _profileHeader.FollowButtonMargin.Constant = 0;
            }
        }
    }
}

