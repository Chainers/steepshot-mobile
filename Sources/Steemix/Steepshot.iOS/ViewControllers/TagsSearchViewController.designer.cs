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
	[Register ("TagsSearchViewController")]
	partial class TagsSearchViewController
	{
		[Outlet]
		UIKit.UIActivityIndicatorView activityIndicator { get; set; }

		[Outlet]
		UIKit.UILabel noTagsLabel { get; set; }

		[Outlet]
		UIKit.UITextField searchTextField { get; set; }

		[Outlet]
		UIKit.UITableView tagsTable { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (searchTextField != null) {
				searchTextField.Dispose ();
				searchTextField = null;
			}

			if (tagsTable != null) {
				tagsTable.Dispose ();
				tagsTable = null;
			}

			if (noTagsLabel != null) {
				noTagsLabel.Dispose ();
				noTagsLabel = null;
			}

			if (activityIndicator != null) {
				activityIndicator.Dispose ();
				activityIndicator = null;
			}
		}
	}
}
