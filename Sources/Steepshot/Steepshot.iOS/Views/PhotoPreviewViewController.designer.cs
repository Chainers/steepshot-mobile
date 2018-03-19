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
		UIKit.UICollectionView photoCollection { get; set; }

		[Outlet]
		UIKit.UIImageView photoView { get; set; }

		[Outlet]
		UIKit.UIImageView rotate { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (photoView != null) {
				photoView.Dispose ();
				photoView = null;
			}

			if (rotate != null) {
				rotate.Dispose ();
				rotate = null;
			}

			if (photoCollection != null) {
				photoCollection.Dispose ();
				photoCollection = null;
			}
		}
	}
}
