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
	[Register ("FeedViewController")]
	partial class FeedViewController
	{
		[Outlet]
		UIKit.UIActivityIndicatorView activityIndicator { get; set; }

		[Outlet]
		UIKit.UITableView FeedTable { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (FeedTable != null) {
				FeedTable.Dispose ();
				FeedTable = null;
			}

			if (activityIndicator != null) {
				activityIndicator.Dispose ();
				activityIndicator = null;
			}
		}
	}
}
