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
	[Register ("WelcomeViewController")]
	partial class WelcomeViewController
	{
		[Outlet]
		UIKit.UIView agreementView { get; set; }

		[Outlet]
		UIKit.UIImageView logo { get; set; }

		[Outlet]
		UIKit.UIButton newAccount { get; set; }

		[Outlet]
		UIKit.UIButton steemLogin { get; set; }

		[Outlet]
		UIKit.UISwitch termsSwitcher { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (agreementView != null) {
				agreementView.Dispose ();
				agreementView = null;
			}
            
			if (logo != null) {
				logo.Dispose ();
				logo = null;
			}

			if (newAccount != null) {
				newAccount.Dispose ();
				newAccount = null;
			}

			if (steemLogin != null) {
				steemLogin.Dispose ();
				steemLogin = null;
			}

			if (termsSwitcher != null) {
				termsSwitcher.Dispose ();
				termsSwitcher = null;
			}
		}
	}
}
