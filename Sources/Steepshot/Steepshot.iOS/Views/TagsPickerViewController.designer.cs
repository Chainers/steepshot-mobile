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
	[Register ("TagsPickerViewController")]
	partial class TagsPickerViewController
	{
		[Outlet]
		UIKit.NSLayoutConstraint collectionViewHeight { get; set; }

		[Outlet]
		UIKit.UICollectionView tagsCollectionView { get; set; }

		[Outlet]
		UIKit.UITableView tagsTableView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (collectionViewHeight != null) {
				collectionViewHeight.Dispose ();
				collectionViewHeight = null;
			}

			if (tagsCollectionView != null) {
				tagsCollectionView.Dispose ();
				tagsCollectionView = null;
			}

			if (tagsTableView != null) {
				tagsTableView.Dispose ();
				tagsTableView = null;
			}
		}
	}
}
