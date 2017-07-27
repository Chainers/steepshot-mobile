using System;
using FFImageLoading;
using FFImageLoading.Work;
using Foundation;
using Steepshot.Core.Models.Responses;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public partial class CommentTableViewCell : UITableViewCell
    {
        protected CommentTableViewCell(IntPtr handle) : base(handle) { }
        public static readonly NSString Key = new NSString("CommentTableViewCell");
        public static readonly UINib Nib;
        private bool _isButtonBinded;
        public event VoteEventHandler<VoteResponse> Voted;
        public event HeaderTappedHandler GoToProfile;
        private Post _currentPost;
        private IScheduledWork _scheduledWorkAvatar;

        public bool IsVotedSet => Voted != null;
        public bool IsGoToProfileSet => GoToProfile != null;

        static CommentTableViewCell()
        {
            Nib = UINib.FromName("CommentTableViewCell", NSBundle.MainBundle);
        }

        public override void LayoutSubviews()
        {
            avatar.Layer.CornerRadius = avatar.Frame.Size.Width / 2;
            base.LayoutSubviews();
        }

        public void UpdateCell(Post post)
        {
            _currentPost = post;

            avatar.Image = null;
            _scheduledWorkAvatar?.Cancel();
            _scheduledWorkAvatar = ImageService.Instance.LoadUrl(_currentPost.Avatar, TimeSpan.FromDays(30))
                                                                             .Retry(2, 200)
                                                                             .FadeAnimation(false, false, 0)
                                                                             .DownSample(width: (int)avatar.Frame.Width)
                                                                             .Into(avatar);
            bodyLabel.Text = _currentPost.Body;
            loginLabel.Text = _currentPost.Author;
            likeLabel.Text = _currentPost.NetVotes.ToString();
            costLabel.Text = $"${_currentPost.TotalPayoutReward}";
            likeButton.Selected = _currentPost.Vote;
            likeButton.Enabled = true;

            if (!_isButtonBinded)
            {
                UITapGestureRecognizer tap = new UITapGestureRecognizer(() =>
                {
                    GoToProfile(_currentPost.Author);
                });
                avatar.AddGestureRecognizer(tap);

                likeButton.TouchDown += LikeTap;
                _isButtonBinded = true;
            }
        }

        private void LikeTap(object sender, EventArgs e)
        {
            likeButton.Enabled = false;
            Voted(!likeButton.Selected, _currentPost.Url, (postUrl, post) =>
            {
                if (postUrl == _currentPost.Url)
                {
                    likeButton.Selected = post.IsVoted;
                    likeButton.Enabled = true;
                }
            });
        }
    }
}
