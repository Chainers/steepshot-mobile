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
		UIKit.UIImageView avatar { get; set; }

		[Outlet]
		UIKit.UIButton eyeButton { get; set; }

		[Outlet]
		UIKit.UIButton loginButton { get; set; }

		[Outlet]
		UIKit.UILabel loginTitle { get; set; }

		[Outlet]
		UIKit.UITextField password { get; set; }

		[Outlet]
		UIKit.UIButton postingKeyButton { get; set; }

		[Outlet]
		UIKit.UILabel postingLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (avatar != null) {
				avatar.Dispose ();
				avatar = null;
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
		}
	}
}
