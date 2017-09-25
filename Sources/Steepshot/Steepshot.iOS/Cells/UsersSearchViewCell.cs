using System;
using FFImageLoading;
using FFImageLoading.Work;
using Foundation;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;
using UIKit;

namespace Steepshot.iOS.Cells
{
	public partial class UsersSearchViewCell : UITableViewCell
	{
		public static readonly NSString Key = new NSString(nameof(UsersSearchViewCell));
		public static readonly UINib Nib;
		private IScheduledWork _scheduledWorkAvatar;
		protected UsersSearchViewCell(IntPtr handle) : base(handle) { }
		private UserSearchResult _currentUser;
		private VotersResult _currentVoter;

		static UsersSearchViewCell()
		{
			Nib = UINib.FromName(nameof(UsersSearchViewCell), NSBundle.MainBundle);
		}

		public override void LayoutSubviews()
		{
			avatar.Layer.CornerRadius = avatar.Frame.Size.Width / 2;
			SelectionStyle = UITableViewCellSelectionStyle.None;
		}

		public void UpdateCell(VotersResult user)
		{
			_currentVoter = user;
			avatar.Image = UIImage.FromFile("ic_user_placeholder.png");

			_scheduledWorkAvatar?.Cancel();
			_scheduledWorkAvatar = ImageService.Instance.LoadUrl(_currentVoter.ProfileImage, TimeSpan.FromDays(30))
																						 .Retry(2, 200)
																						 .FadeAnimation(false, false, 0)
																						 .DownSample(width: (int)avatar.Frame.Width)
																						 .Into(avatar);
			if (!string.IsNullOrEmpty(_currentVoter.Name))
			{
				username.Text = _currentVoter.Name;
				usernameHeight.Constant = 18;
			}
			else
				usernameHeight.Constant = 0;

			powerLabel.Text = $"{_currentVoter.Percent}%";
			login.Text = _currentVoter.Username;
		}

		public void UpdateCell(UserSearchResult user)
		{
			_currentUser = user;
			avatar.Image = UIImage.FromFile("ic_user_placeholder.png");

			_scheduledWorkAvatar?.Cancel();
			_scheduledWorkAvatar = ImageService.Instance.LoadUrl(_currentUser.ProfileImage, TimeSpan.FromDays(30))
																						 .Retry(2, 200)
																						 .FadeAnimation(false, false, 0)
																						 .DownSample(width: (int)avatar.Frame.Width)
																						 .Into(avatar);
			if (!string.IsNullOrEmpty(_currentUser.Name))
			{
				username.Text = _currentUser.Name;
				usernameHeight.Constant = 18;
			}
			else
				usernameHeight.Constant = 0;
			
			login.Text = _currentUser.Username;
		}
	}
}
