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
	[Register ("TagCollectionViewCell")]
	partial class TagCollectionViewCell
	{
		[Outlet]
		UIKit.UIButton addTagsButton { get; set; }

		[Outlet]
		UIKit.UIImageView closeImage { get; set; }

		[Outlet]
		UIKit.UILabel tagText { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (tagText != null) {
				tagText.Dispose ();
				tagText = null;
			}

			if (addTagsButton != null) {
				addTagsButton.Dispose ();
				addTagsButton = null;
			}

			if (closeImage != null) {
				closeImage.Dispose ();
				closeImage = null;
			}
		}
	}
}
