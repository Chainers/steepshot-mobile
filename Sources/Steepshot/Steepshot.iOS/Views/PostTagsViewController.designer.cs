// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using Foundation;

namespace Steepshot.iOS.Views
{
	[Register ("PostTagsViewController")]
	partial class PostTagsViewController
	{
		[Outlet]
		UIKit.UIButton addTagButton { get; set; }

		[Outlet]
		UIKit.UIButton addTagsButton { get; set; }

		[Outlet]
		UIKit.UITextField searchText { get; set; }

		[Outlet]
		UIKit.UICollectionView tagsCollectionView { get; set; }

		[Outlet]
		UIKit.UITableView tagsTable { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (tagsCollectionView != null) {
				tagsCollectionView.Dispose ();
				tagsCollectionView = null;
			}

			if (tagsTable != null) {
				tagsTable.Dispose ();
				tagsTable = null;
			}

			if (addTagsButton != null) {
				addTagsButton.Dispose ();
				addTagsButton = null;
			}

			if (addTagButton != null) {
				addTagButton.Dispose ();
				addTagButton = null;
			}

			if (searchText != null) {
				searchText.Dispose ();
				searchText = null;
			}
		}
	}
}
