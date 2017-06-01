using System;
using UIKit;

namespace Steepshot.iOS
{
	public partial class ProfileHeaderViewController : UIViewController
	{
		private Action _viewLoaded;

		public UIButton SwitchButton => switchButton;
		public UIButton SettingsButton => settingsButton;
		public UIImageView Avatar => avatar;
		public UIButton Balance => balanceButton;
		public UIButton FollowButton => followButton;
		public UIButton PhotosButton => photosButton;
		public UIButton FollowingButton => followingButton;
		public UIButton FollowersButton => followersButton;
		public UILabel Username => username;
		public UILabel Date => dateLabel;
		public UILabel DescriptionLabel => descriptionLabel;
		public UILabel Location => locationLabel;

		public NSLayoutConstraint FollowButtonWidth => followButtonWidth;
		public NSLayoutConstraint FollowButtonMargin => followButtonMargin;

		public ProfileHeaderViewController(Action viewLoaded) : base("ProfileHeaderViewController", null)
		{
			_viewLoaded = viewLoaded;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
            _viewLoaded();
			username.Font = Constants.Heavy135;
			dateLabel.Font = Constants.Semibold10;

			avatar.Layer.CornerRadius = avatar.Frame.Width / 2;

			balanceButton.ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
			balanceButton.ContentEdgeInsets = new UIEdgeInsets(0,11,0,4);
			balanceButton.ImageEdgeInsets = new UIEdgeInsets(0,-9,0,0);
			balanceButton.Layer.BorderColor = UIColor.White.CGColor;
			balanceButton.Layer.BorderWidth = 1;
			balanceButton.Layer.CornerRadius = 3;
			balanceButton.Font = Constants.Bold12;

			followButton.Layer.BorderColor = UIColor.White.CGColor;
			followButton.Layer.BorderWidth = 1;
			followButton.Layer.CornerRadius = 3;

			descriptionLabel.Font = Constants.Regular12;
			locationLabel.Font = Constants.Regular12;
			switchButton.ImageEdgeInsets = new UIEdgeInsets(0,11,0,0);
		}
	}
}

