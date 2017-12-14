// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Steepshot.iOS.Views
{
	[Register ("PreLoginViewController")]
	partial class PreLoginViewController
	{
		[Outlet]
		UIKit.UIActivityIndicatorView activityIndicator { get; set; }

		[Outlet]
		UIKit.UIButton loginButton { get; set; }

		[Outlet]
		UIKit.UITextField loginText { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (activityIndicator != null) {
				activityIndicator.Dispose ();
				activityIndicator = null;
			}

			if (loginButton != null) {
				loginButton.Dispose ();
				loginButton = null;
			}

			if (loginText != null) {
				loginText.Dispose ();
				loginText = null;
			}
		}
	}
}
