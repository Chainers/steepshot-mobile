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
	[Register ("AccountTableViewCell")]
	partial class AccountTableViewCell
	{
		[Outlet]
		UIKit.UIImageView closeButton { get; set; }

		[Outlet]
		UIKit.UILabel networkName { get; set; }

		[Outlet]
		UIKit.UIImageView networkStatus { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (networkName != null) {
				networkName.Dispose ();
				networkName = null;
			}

			if (networkStatus != null) {
				networkStatus.Dispose ();
				networkStatus = null;
			}

			if (closeButton != null) {
				closeButton.Dispose ();
				closeButton = null;
			}
		}
	}
}
