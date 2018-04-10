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
	[Register ("PhotoPreviewViewController")]
	partial class PhotoPreviewViewController
	{
		[Outlet]
		UIKit.UIView cropBackgroundView { get; set; }

		[Outlet]
		UIKit.UIImageView multiSelect { get; set; }

		[Outlet]
		UIKit.UICollectionView photoCollection { get; set; }

		[Outlet]
		UIKit.UIImageView resize { get; set; }

		[Outlet]
		UIKit.UIImageView rotate { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (cropBackgroundView != null) {
				cropBackgroundView.Dispose ();
				cropBackgroundView = null;
			}

			if (multiSelect != null) {
				multiSelect.Dispose ();
				multiSelect = null;
			}

			if (photoCollection != null) {
				photoCollection.Dispose ();
				photoCollection = null;
			}

			if (rotate != null) {
				rotate.Dispose ();
				rotate = null;
			}

			if (resize != null) {
				resize.Dispose ();
				resize = null;
			}
		}
	}
}
