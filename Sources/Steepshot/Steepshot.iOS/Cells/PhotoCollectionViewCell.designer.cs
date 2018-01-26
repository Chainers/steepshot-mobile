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
	[Register ("PhotoCollectionViewCell")]
	partial class PhotoCollectionViewCell
	{
		[Outlet]
		UIKit.NSLayoutConstraint heightConstraint { get; set; }

		[Outlet]
		UIKit.UIImageView photoImg { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint widthConstraint { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (photoImg != null) {
				photoImg.Dispose ();
				photoImg = null;
			}

			if (heightConstraint != null) {
				heightConstraint.Dispose ();
				heightConstraint = null;
			}

			if (widthConstraint != null) {
				widthConstraint.Dispose ();
				widthConstraint = null;
			}
		}
	}
}
