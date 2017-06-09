// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Steepshot.iOS
{
	[Register ("LoginViewController")]
	partial class LoginViewController
	{
		[Outlet]
		UIKit.UIActivityIndicatorView activityIndicator { get; set; }

		[Outlet]
		UIKit.UIImageView avatar { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint bottomMargin { get; set; }

		[Outlet]
		UIKit.UIButton eyeButton { get; set; }

		[Outlet]
		UIKit.UIButton loginButton { get; set; }

		[Outlet]
		UIKit.UILabel loginTitle { get; set; }

		[Outlet]
		UIKit.UITextField password { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint photoBottomMargin { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint photoMargin { get; set; }

		[Outlet]
		UIKit.UIButton postingKeyButton { get; set; }

		[Outlet]
		UIKit.UILabel postingLabel { get; set; }

		[Outlet]
		UIKit.UIButton qrButton { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint topMargin { get; set; }

		[Outlet]
		UIKit.UIButton tosButton { get; set; }

		[Outlet]
		UIKit.UISwitch tosSwitch { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (activityIndicator != null) {
				activityIndicator.Dispose ();
				activityIndicator = null;
			}

			if (avatar != null) {
				avatar.Dispose ();
				avatar = null;
			}

			if (bottomMargin != null) {
				bottomMargin.Dispose ();
				bottomMargin = null;
			}

			if (eyeButton != null) {
				eyeButton.Dispose ();
				eyeButton = null;
			}

			if (loginButton != null) {
				loginButton.Dispose ();
				loginButton = null;
			}

			if (loginTitle != null) {
				loginTitle.Dispose ();
				loginTitle = null;
			}

			if (password != null) {
				password.Dispose ();
				password = null;
			}

			if (postingKeyButton != null) {
				postingKeyButton.Dispose ();
				postingKeyButton = null;
			}

			if (postingLabel != null) {
				postingLabel.Dispose ();
				postingLabel = null;
			}

			if (qrButton != null) {
				qrButton.Dispose ();
				qrButton = null;
			}

			if (topMargin != null) {
				topMargin.Dispose ();
				topMargin = null;
			}

			if (photoMargin != null) {
				photoMargin.Dispose ();
				photoMargin = null;
			}

			if (photoBottomMargin != null) {
				photoBottomMargin.Dispose ();
				photoBottomMargin = null;
			}

			if (tosButton != null) {
				tosButton.Dispose ();
				tosButton = null;
			}

			if (tosSwitch != null) {
				tosSwitch.Dispose ();
				tosSwitch = null;
			}
		}
	}
}
