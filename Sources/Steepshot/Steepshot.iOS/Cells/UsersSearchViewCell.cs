using System;
using FFImageLoading;
using FFImageLoading.Work;
using Steepshot.Core.Models.Common;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public partial class UsersSearchViewCell : UITableViewCell
    {
        private IScheduledWork _scheduledWorkAvatar;
        protected UsersSearchViewCell(IntPtr handle) : base(handle) { }
        private UserFriend _current;



        public override void LayoutSubviews()
        {
            avatar.Layer.CornerRadius = avatar.Frame.Size.Width / 2;
            SelectionStyle = UITableViewCellSelectionStyle.None;
        }

        public void UpdateCell(UserFriend user)
        {
            _current = user;
            avatar.Image = UIImage.FromFile("ic_user_placeholder.png");

            _scheduledWorkAvatar?.Cancel();
            _scheduledWorkAvatar = ImageService.Instance.LoadUrl(_current.Avatar, TimeSpan.FromDays(30))
                                                                                         .Retry(2, 200)
                                                                                         .FadeAnimation(false, false, 0)
                                                                                         .DownSample(width: (int)avatar.Frame.Width)
                                                                                         .Into(avatar);
            if (!string.IsNullOrEmpty(_current.Name))
            {
                username.Text = _current.Name;
                usernameHeight.Constant = 18;
            }
            else
                usernameHeight.Constant = 0;

            powerLabel.Text = $"{_current.Reputation}%";
            login.Text = _current.Author;
        }
    }
}
