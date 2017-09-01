// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using Foundation;

namespace Steepshot.iOS.Views
{
	[Register ("ImagePreviewViewController")]
	partial class ImagePreviewViewController
	{
		[Outlet]
		UIKit.UIView backgroundView { get; set; }

		[Outlet]
		UIKit.UIScrollView scrollView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (backgroundView != null) {
				backgroundView.Dispose ();
				backgroundView = null;
			}

			if (scrollView != null) {
				scrollView.Dispose ();
				scrollView = null;
			}
		}
	}
}
