// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Steepshot.iOS.Cells
{
	[Register ("FollowViewCell")]
	partial class FollowViewCell
	{
		[Outlet]
		UIKit.UIImageView avatar { get; set; }

		[Outlet]
		UIKit.UIButton followButton { get; set; }

		[Outlet]
		UIKit.UILabel name { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint nameHiddenConstraint { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint nameVisibleConstraint { get; set; }

		[Outlet]
		UIKit.UIView profileTapZone { get; set; }

		[Outlet]
		UIKit.UIActivityIndicatorView progressBar { get; set; }

		[Outlet]
		UIKit.UILabel userName { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (avatar != null) {
				avatar.Dispose ();
				avatar = null;
			}

			if (followButton != null) {
				followButton.Dispose ();
				followButton = null;
			}

			if (name != null) {
				name.Dispose ();
				name = null;
			}

			if (nameHiddenConstraint != null) {
				nameHiddenConstraint.Dispose ();
				nameHiddenConstraint = null;
			}

			if (nameVisibleConstraint != null) {
				nameVisibleConstraint.Dispose ();
				nameVisibleConstraint = null;
			}

			if (progressBar != null) {
				progressBar.Dispose ();
				progressBar = null;
			}

			if (userName != null) {
				userName.Dispose ();
				userName = null;
			}

			if (profileTapZone != null) {
				profileTapZone.Dispose ();
				profileTapZone = null;
			}
		}
	}
}
