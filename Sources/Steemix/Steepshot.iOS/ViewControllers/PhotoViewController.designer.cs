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
	[Register ("PhotoViewController")]
	partial class PhotoViewController
	{
		[Outlet]
		UIKit.UIView liveCameraStream { get; set; }

		[Outlet]
		UIKit.UIButton photoButton { get; set; }

		[Outlet]
		UIKit.UICollectionView photoCollection { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (liveCameraStream != null) {
				liveCameraStream.Dispose ();
				liveCameraStream = null;
			}

			if (photoButton != null) {
				photoButton.Dispose ();
				photoButton = null;
			}

			if (photoCollection != null) {
				photoCollection.Dispose ();
				photoCollection = null;
			}
		}
	}
}
