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
	[Register ("DescriptionViewController")]
	partial class DescriptionViewController
	{
		[Outlet]
		UIKit.UITextView descriptionTextField { get; set; }

		[Outlet]
		UIKit.UIImageView photoView { get; set; }

		[Outlet]
		UIKit.UIButton postPhotoButton { get; set; }

		[Outlet]
		UIKit.UITextField tagField { get; set; }

		[Outlet]
		UIKit.UICollectionView tagsCollectionView { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UITextView titleTextField { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (descriptionTextField != null) {
				descriptionTextField.Dispose ();
				descriptionTextField = null;
			}

			if (photoView != null) {
				photoView.Dispose ();
				photoView = null;
			}

			if (postPhotoButton != null) {
				postPhotoButton.Dispose ();
				postPhotoButton = null;
			}

			if (tagsCollectionView != null) {
				tagsCollectionView.Dispose ();
				tagsCollectionView = null;
			}

			if (titleTextField != null) {
				titleTextField.Dispose ();
				titleTextField = null;
			}

			if (tagField != null) {
				tagField.Dispose ();
				tagField = null;
			}
		}
	}
}
