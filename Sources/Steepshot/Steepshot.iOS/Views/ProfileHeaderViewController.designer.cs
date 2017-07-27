// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using Foundation;

namespace Steepshot.iOS.Views
{
	[Register ("ProfileHeaderViewController")]
	partial class ProfileHeaderViewController
	{
		[Outlet]
		UIKit.UIImageView avatar { get; set; }

		[Outlet]
		UIKit.UIButton balanceButton { get; set; }

		[Outlet]
		UIKit.UILabel dateLabel { get; set; }

		[Outlet]
		UIKit.UILabel descriptionLabel { get; set; }

		[Outlet]
		UIKit.UIButton followButton { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint followButtonMargin { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint followButtonWidth { get; set; }

		[Outlet]
		UIKit.UIButton followersButton { get; set; }

		[Outlet]
		UIKit.UIButton followingButton { get; set; }

		[Outlet]
		UIKit.UILabel locationLabel { get; set; }

		[Outlet]
		UIKit.UIButton photosButton { get; set; }

		[Outlet]
		UIKit.UIButton settingsButton { get; set; }

		[Outlet]
		UIKit.UIButton switchButton { get; set; }

		[Outlet]
		UIKit.UILabel username { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (avatar != null) {
				avatar.Dispose ();
				avatar = null;
			}

			if (balanceButton != null) {
				balanceButton.Dispose ();
				balanceButton = null;
			}

			if (dateLabel != null) {
				dateLabel.Dispose ();
				dateLabel = null;
			}

			if (descriptionLabel != null) {
				descriptionLabel.Dispose ();
				descriptionLabel = null;
			}

			if (followButton != null) {
				followButton.Dispose ();
				followButton = null;
			}

			if (followersButton != null) {
				followersButton.Dispose ();
				followersButton = null;
			}

			if (followingButton != null) {
				followingButton.Dispose ();
				followingButton = null;
			}

			if (locationLabel != null) {
				locationLabel.Dispose ();
				locationLabel = null;
			}

			if (photosButton != null) {
				photosButton.Dispose ();
				photosButton = null;
			}

			if (switchButton != null) {
				switchButton.Dispose ();
				switchButton = null;
			}

			if (username != null) {
				username.Dispose ();
				username = null;
			}

			if (settingsButton != null) {
				settingsButton.Dispose ();
				settingsButton = null;
			}

			if (followButtonWidth != null) {
				followButtonWidth.Dispose ();
				followButtonWidth = null;
			}

			if (followButtonMargin != null) {
				followButtonMargin.Dispose ();
				followButtonMargin = null;
			}
		}
	}
}
