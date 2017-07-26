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
	[Register ("VotersViewController")]
	partial class VotersViewController
	{
		[Outlet]
		UIKit.UIActivityIndicatorView progressBar { get; set; }

		[Outlet]
		UIKit.UITableView votersTable { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (votersTable != null) {
				votersTable.Dispose ();
				votersTable = null;
			}

			if (progressBar != null) {
				progressBar.Dispose ();
				progressBar = null;
			}
		}
	}
}
