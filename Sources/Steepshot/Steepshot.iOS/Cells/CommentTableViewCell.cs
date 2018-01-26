using System;
using FFImageLoading;
using FFImageLoading.Work;
using Foundation;
using Steepshot.Core.Models.Common;
using Steepshot.iOS.ViewControllers;
using Steepshot.Core.Extensions;
using UIKit;
using Steepshot.Core;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;

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

        public override void LayoutSubviews()
        {
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
                replyButton.AddGestureRecognizer(replyTap);
                avatar.AddGestureRecognizer(tap);
                costLabel.AddGestureRecognizer(costTap);

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
                flagLabel.Text = $"{_currentPost.NetFlags} {(_currentPost.NetFlags == 1 ? Localization.Messages.Flag : Localization.Messages.Flags)}";
            }
            else
            {
                flagVisibleConstraint.Active = false;
                flagHiddenConstraint.Active = true;
                flagLabel.Hidden = true;
            }
            base.LayoutSubviews();
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
            
            commentText.Text = _currentPost.Body;
            loginLabel.Text = _currentPost.Author;
            likeLabel.Text = _currentPost.NetVotes.ToString();
            costLabel.Text = BaseViewController.ToFormatedCurrencyString(_currentPost.TotalPayoutReward);
            //costLabel.Hidden = true; //!BasePresenter.User.IsNeedRewards;
            likeButton.Selected = _currentPost.Vote;
            likeButton.Enabled = true;
            timestamp.Text = _currentPost.Created.ToPostTime();

            likeLabel.Text = $"{_currentPost.NetLikes} {(_currentPost.NetLikes == 1 ? Localization.Messages.Like : Localization.Messages.Likes)}";
            LayoutIfNeeded();
        }

        private void LikeTap(object sender, EventArgs e)
        {
            CellAction?.Invoke(ActionType.Like, _currentPost);
        }

        private void MoreTap(object sender, EventArgs e)
        {
            CellAction?.Invoke(ActionType.More, _currentPost);
        }
    }
}
