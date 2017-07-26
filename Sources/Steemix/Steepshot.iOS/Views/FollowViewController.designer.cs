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
	[Register ("FollowViewController")]
	partial class FollowViewController
	{
		[Outlet]
		UIKit.UITableView followTableView { get; set; }

		[Outlet]
		UIKit.UIActivityIndicatorView progressBar { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (followTableView != null) {
				followTableView.Dispose ();
				followTableView = null;
			}

			if (progressBar != null) {
				progressBar.Dispose ();
				progressBar = null;
			}
		}
	}
}
