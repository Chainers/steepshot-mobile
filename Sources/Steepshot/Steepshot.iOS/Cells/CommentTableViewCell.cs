using System;
using FFImageLoading;
using FFImageLoading.Work;
using Foundation;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Extensions;
using UIKit;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using CoreGraphics;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;

namespace Steepshot.iOS.Cells
{
    public partial class CommentTableViewCell : UITableViewCell
    {
        protected CommentTableViewCell(IntPtr handle) : base(handle) { }
        public static readonly NSString Key = new NSString(nameof(CommentTableViewCell));
        public static readonly UINib Nib;
        public bool IsCellActionSet => CellAction != null;
        public Action<ActionType, Post> CellAction;
        private bool _isInitialized;
        private Post _currentPost;
        private IScheduledWork _scheduledWorkAvatar;

        static CommentTableViewCell()
        {
            Nib = UINib.FromName(nameof(CommentTableViewCell), NSBundle.MainBundle);
        }

        public void UpdateCell(Post post)
        {
            _currentPost = post;

            _scheduledWorkAvatar?.Cancel();
            if (!string.IsNullOrEmpty(_currentPost.Avatar))
            {
                _scheduledWorkAvatar = ImageService.Instance.LoadUrl(_currentPost.Avatar, TimeSpan.FromDays(30))
                                                   .LoadingPlaceholder("ic_noavatar.png")
                                                   .ErrorPlaceholder("ic_noavatar.png")
                                                   .FadeAnimation(false, false, 0)
                                                   .DownSample(200)
                                                   .Into(avatar);
            }
            else
                avatar.Image = UIImage.FromBundle("ic_noavatar");

            if (_currentPost.VoteChanging)
                Animate();
            else
            {
                likeButton.Transform = CGAffineTransform.MakeScale(1f, 1f);
                likeButton.Selected = _currentPost.Vote;
                likeButton.Enabled = true;
            }

            commentText.Text = _currentPost.Body;
            loginLabel.Text = _currentPost.Author;
            //costLabel.Text = BaseViewController.ToFormatedCurrencyString(_currentPost.TotalPayoutReward);

            timestamp.Text = _currentPost.Created.ToPostTime();

            likeLabel.Text = AppSettings.LocalizationManager.GetText(_currentPost.NetLikes == 1 ? LocalizationKeys.Like : LocalizationKeys.Likes, _currentPost.NetLikes);
            flagLabel.Text = AppSettings.LocalizationManager.GetText(_currentPost.NetFlags == 1 ? LocalizationKeys.Flag : LocalizationKeys.Flags, _currentPost.NetFlags);

            if (!_isInitialized)
            {
                avatar.Layer.CornerRadius = avatar.Frame.Size.Width / 2;

                var tap = new UITapGestureRecognizer(() =>
                {
                    CellAction?.Invoke(ActionType.Profile, _currentPost);
                });
                var costTap = new UITapGestureRecognizer(() =>
                {
                    CellAction?.Invoke(ActionType.Profile, _currentPost);
                });
                var replyTap = new UITapGestureRecognizer(() =>
                {
                    CellAction?.Invoke(ActionType.Reply, _currentPost);
                });
                var likersTap = new UITapGestureRecognizer(() =>
                {
                    CellAction?.Invoke(ActionType.Voters, _currentPost);
                });
                var flagersTap = new UITapGestureRecognizer(() =>
                {
                    CellAction?.Invoke(ActionType.Flagers, _currentPost);
                });
                replyButton.AddGestureRecognizer(replyTap);
                profileTapView.AddGestureRecognizer(tap);
                costLabel.AddGestureRecognizer(costTap);
                likeLabel.AddGestureRecognizer(likersTap);
                flagLabel.AddGestureRecognizer(flagersTap);

                commentText.Font = Helpers.Constants.Regular14;
                loginLabel.Font = Helpers.Constants.Semibold14;
                likeLabel.Font = Helpers.Constants.Regular12;
                costLabel.Font = Helpers.Constants.Regular12;
                replyButton.Font = Helpers.Constants.Regular12;
                timestamp.Font = Helpers.Constants.Regular12;

                likeButton.TouchDown += LikeTap;
                otherActionButton.TouchDown += MoreTap;
                _isInitialized = true;
                if (!BasePresenter.User.IsAuthenticated)
                {
                    replyButton.Hidden = true;
                    replyHiddenConstraint.Active = true;
                    replyVisibleConstraint.Active = false;
                }
            }
            if (_currentPost.NetFlags > 0)
            {
                flagLabel.Hidden = false;
                flagVisibleConstraint.Active = true;
                flagHiddenConstraint.Active = false;
            }
            else
            {
                flagVisibleConstraint.Active = false;
                flagHiddenConstraint.Active = true;
                flagLabel.Hidden = true;
            }

            if (_currentPost.NetLikes > 0)
            {
                likeLabel.Hidden = false;
                flagVisibleConstraint.Active = true;
                likeHiddenConstraint.Active = false;
            }
            else
            {
                likeLabel.Hidden = true;
                flagVisibleConstraint.Active = false;
                likeHiddenConstraint.Active = true;
            }
        }

        private void LikeTap(object sender, EventArgs e)
        {
            if (!BasePostPresenter.IsEnableVote)
                return;
            CellAction?.Invoke(ActionType.Like, _currentPost);
        }

        private void MoreTap(object sender, EventArgs e)
        {
            CellAction?.Invoke(ActionType.More, _currentPost);
        }

        private void Animate()
        {
            likeButton.Selected = true;
            UIView.Animate(0.4, 0, UIViewAnimationOptions.Autoreverse | UIViewAnimationOptions.Repeat | UIViewAnimationOptions.CurveEaseIn, () =>
            {
                likeButton.Transform = CGAffineTransform.MakeScale(0.5f, 0.5f);
            }, null);
        }
    }
}
