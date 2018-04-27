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
	[Register ("PostViewController")]
	partial class PostViewController
	{
		[Outlet]
		UIKit.UIActivityIndicatorView loader { get; set; }

		[Outlet]
		UIKit.UIScrollView scrollView { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint scrollViewHeight { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint scrollViewWidth { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (scrollView != null) {
				scrollView.Dispose ();
				scrollView = null;
			}

			if (scrollViewHeight != null) {
				scrollViewHeight.Dispose ();
				scrollViewHeight = null;
			}

			if (scrollViewWidth != null) {
				scrollViewWidth.Dispose ();
				scrollViewWidth = null;
			}

			if (loader != null) {
				loader.Dispose ();
				loader = null;
			}
		}
	}
}
