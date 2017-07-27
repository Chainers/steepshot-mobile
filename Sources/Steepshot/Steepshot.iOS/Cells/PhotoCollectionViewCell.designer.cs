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
	[Register ("PhotoCollectionViewCell")]
	partial class PhotoCollectionViewCell
	{
		[Outlet]
		UIKit.UIImageView photoImg { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (photoImg != null) {
				photoImg.Dispose ();
				photoImg = null;
			}
		}
	}
}
