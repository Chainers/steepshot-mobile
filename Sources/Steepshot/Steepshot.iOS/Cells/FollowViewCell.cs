using System;
using FFImageLoading;
using FFImageLoading.Work;
using Foundation;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public delegate void FollowEventHandler(FollowType followType, string authorName, Action<string, bool?> success);

    public partial class FollowViewCell : UITableViewCell
    {
        protected FollowViewCell(IntPtr handle) : base(handle) { }
        public static readonly NSString Key = new NSString("FollowViewCell");
        public static readonly UINib Nib;
        private bool _isButtonBinded;
        public event FollowEventHandler Follow;
        public event HeaderTappedHandler GoToProfile;
        private UserFriend _currentUser;
        public bool IsFollowSet => Follow != null;
        public bool IsGoToProfileSet => GoToProfile != null;
        private IScheduledWork _scheduledWorkAvatar;

        static FollowViewCell()
        {
            Nib = UINib.FromName("FollowViewCell", NSBundle.MainBundle);
        }

        public override void LayoutSubviews()
        {
            avatar.Layer.CornerRadius = avatar.Frame.Size.Width / 2;
            followButton.Layer.CornerRadius = 5;
            followButton.Layer.BorderWidth = 2;
            followButton.Layer.BorderColor = Constants.Blue.CGColor;
            followButton.ContentEdgeInsets = new UIEdgeInsets(10, 10, 10, 10);
            followButton.Hidden = !BaseViewController.User.IsAuthenticated || _currentUser.Author == BaseViewController.User.Login;
            base.LayoutSubviews();
        }

        public void UpdateCell(UserFriend user)
        {
            _currentUser = user;
            avatar.Image = null;
            _scheduledWorkAvatar?.Cancel();
            _scheduledWorkAvatar = ImageService.Instance.LoadUrl(_currentUser.Avatar, TimeSpan.FromDays(30))
                                                                                         .Retry(2, 200)
                                                                                         .FadeAnimation(false, false, 0)
                                                                                         .DownSample(width: (int)avatar.Frame.Width)
                                                                                         .Into(avatar);

            userName.Text = _currentUser.Author;
            followButton.SetTitle(_currentUser.HasFollowed ? "UNFOLLOW" : "FOLLOW", UIControlState.Normal);

            followButton.Enabled = true;
            progressBar.StopAnimating();

            if (!_isButtonBinded)
            {
                UITapGestureRecognizer tap = new UITapGestureRecognizer(() =>
                {
                    GoToProfile?.Invoke(_currentUser.Author);
                });
                avatar.AddGestureRecognizer(tap);
            }

            if (!_isButtonBinded)
            {
                followButton.TouchDown += (sender, e) =>
                {
                    followButton.Enabled = false;
                    progressBar.StartAnimating();
                    Follow?.Invoke(_currentUser.HasFollowed ? FollowType.UnFollow : FollowType.Follow, _currentUser.Author,
                        (author, success) =>
                        {
                            if (author == _currentUser.Author && success != null)
                            {
                                followButton.SetTitle((bool) success ? "UNFOLLOW" : "FOLLOW",
                                    UIControlState.Normal);
                                followButton.Enabled = true;
                                progressBar.StopAnimating();
                            }
                        });
                };
                _isButtonBinded = true;
            }
        }
    }
}
