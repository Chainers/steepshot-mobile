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
	[Register ("SettingsViewController")]
	partial class SettingsViewController
	{
		[Outlet]
		UIKit.UIButton addAccountButton { get; set; }

		[Outlet]
		UIKit.UIButton golosButton { get; set; }

		[Outlet]
		UIKit.UILabel golosLabel { get; set; }

		[Outlet]
		UIKit.UIView golosView { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint golosViewHeight { get; set; }

		[Outlet]
		UIKit.UISwitch lowRatedSwitch { get; set; }

		[Outlet]
		UIKit.UISwitch nsfwSwitch { get; set; }

		[Outlet]
		UIKit.UIButton reportButton { get; set; }

		[Outlet]
		UIKit.UIButton steemButton { get; set; }

		[Outlet]
		UIKit.UILabel steemLabel { get; set; }

		[Outlet]
		UIKit.UIView steemView { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint steemViewHeight { get; set; }

		[Outlet]
		UIKit.UIButton termsButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (nsfwSwitch != null) {
				nsfwSwitch.Dispose ();
				nsfwSwitch = null;
			}

			if (steemLabel != null) {
				steemLabel.Dispose ();
				steemLabel = null;
			}

			if (steemViewHeight != null) {
				steemViewHeight.Dispose ();
				steemViewHeight = null;
			}

			if (golosLabel != null) {
				golosLabel.Dispose ();
				golosLabel = null;
			}

			if (golosViewHeight != null) {
				golosViewHeight.Dispose ();
				golosViewHeight = null;
			}

			if (addAccountButton != null) {
				addAccountButton.Dispose ();
				addAccountButton = null;
			}

			if (steemButton != null) {
				steemButton.Dispose ();
				steemButton = null;
			}

			if (golosButton != null) {
				golosButton.Dispose ();
				golosButton = null;
			}

			if (steemView != null) {
				steemView.Dispose ();
				steemView = null;
			}

			if (golosView != null) {
				golosView.Dispose ();
				golosView = null;
			}

			if (reportButton != null) {
				reportButton.Dispose ();
				reportButton = null;
			}

			if (termsButton != null) {
				termsButton.Dispose ();
				termsButton = null;
			}

			if (lowRatedSwitch != null) {
				lowRatedSwitch.Dispose ();
				lowRatedSwitch = null;
			}
		}
	}
}
