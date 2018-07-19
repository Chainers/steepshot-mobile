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
	[Register ("SettingsViewController")]
	partial class SettingsViewController
	{
		[Outlet]
		UIKit.UITableView accountsTable { get; set; }

		[Outlet]
		UIKit.UIButton addAccountButton { get; set; }

		[Outlet]
		UIKit.UIButton guideButton { get; set; }

		[Outlet]
		UIKit.UILabel lowRatedLabel { get; set; }

		[Outlet]
		UIKit.UISwitch lowRatedSwitch { get; set; }

		[Outlet]
		UIKit.UIButton notificationSettings { get; set; }

		[Outlet]
		UIKit.UILabel nsfwLabel { get; set; }

		[Outlet]
		UIKit.UISwitch nsfwSwitch { get; set; }

		[Outlet]
		UIKit.UIButton reportButton { get; set; }

		[Outlet]
		UIKit.UIScrollView rootScrollView { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint tableHeight { get; set; }

		[Outlet]
		UIKit.UIButton termsButton { get; set; }

		[Outlet]
		UIKit.UILabel versionLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (accountsTable != null) {
				accountsTable.Dispose ();
				accountsTable = null;
			}

			if (addAccountButton != null) {
				addAccountButton.Dispose ();
				addAccountButton = null;
			}

			if (guideButton != null) {
				guideButton.Dispose ();
				guideButton = null;
			}

			if (lowRatedLabel != null) {
				lowRatedLabel.Dispose ();
				lowRatedLabel = null;
			}

			if (lowRatedSwitch != null) {
				lowRatedSwitch.Dispose ();
				lowRatedSwitch = null;
			}

			if (nsfwLabel != null) {
				nsfwLabel.Dispose ();
				nsfwLabel = null;
			}

			if (nsfwSwitch != null) {
				nsfwSwitch.Dispose ();
				nsfwSwitch = null;
			}

			if (reportButton != null) {
				reportButton.Dispose ();
				reportButton = null;
			}

			if (rootScrollView != null) {
				rootScrollView.Dispose ();
				rootScrollView = null;
			}

			if (tableHeight != null) {
				tableHeight.Dispose ();
				tableHeight = null;
			}

			if (termsButton != null) {
				termsButton.Dispose ();
				termsButton = null;
			}

			if (versionLabel != null) {
				versionLabel.Dispose ();
				versionLabel = null;
			}

			if (notificationSettings != null) {
				notificationSettings.Dispose ();
				notificationSettings = null;
			}
		}
	}
}
