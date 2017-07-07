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
	[Register ("UsersSearchViewCell")]
	partial class UsersSearchViewCell
	{
		[Outlet]
		UIKit.UIImageView avatar { get; set; }

		[Outlet]
		UIKit.UILabel login { get; set; }

		[Outlet]
		UIKit.UILabel username { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint usernameHeight { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (avatar != null) {
				avatar.Dispose ();
				avatar = null;
			}

			if (login != null) {
				login.Dispose ();
				login = null;
			}

			if (username != null) {
				username.Dispose ();
				username = null;
			}

			if (usernameHeight != null) {
				usernameHeight.Dispose ();
				usernameHeight = null;
			}
		}
	}
}
