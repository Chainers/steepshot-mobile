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
	[Register ("ProfileHeaderViewController")]
	partial class ProfileHeaderViewController
	{
		[Outlet]
		UIKit.NSLayoutConstraint accountViewHeight { get; set; }

		[Outlet]
		UIKit.UILabel balanceLabel { get; set; }

		[Outlet]
		UIKit.UILabel balanceValue { get; set; }

		[Outlet]
		UIKit.UIView balanceView { get; set; }

		[Outlet]
		UIKit.UILabel descriptionLabel { get; set; }

		[Outlet]
		UIKit.UIView descriptionView { get; set; }

		[Outlet]
		UIKit.UIButton followButton { get; set; }

		[Outlet]
		UIKit.UIButton followersButton { get; set; }

		[Outlet]
		UIKit.UIButton followingButton { get; set; }

		[Outlet]
		UIKit.UILabel locationLabel { get; set; }

		[Outlet]
		UIKit.UIButton photosButton { get; set; }

		[Outlet]
		UIKit.UIActivityIndicatorView progressBar { get; set; }

		[Outlet]
		UIKit.UIStackView stackView { get; set; }

		[Outlet]
		UIKit.UILabel username { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint websiteHeight { get; set; }

		[Outlet]
		UIKit.UITextView websiteTextView { get; set; }

		[Outlet]
		UIKit.UIView websiteView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (accountViewHeight != null) {
				accountViewHeight.Dispose ();
				accountViewHeight = null;
			}

			if (balanceLabel != null) {
				balanceLabel.Dispose ();
				balanceLabel = null;
			}

			if (balanceValue != null) {
				balanceValue.Dispose ();
				balanceValue = null;
			}

			if (balanceView != null) {
				balanceView.Dispose ();
				balanceView = null;
			}

			if (descriptionLabel != null) {
				descriptionLabel.Dispose ();
				descriptionLabel = null;
			}

			if (descriptionView != null) {
				descriptionView.Dispose ();
				descriptionView = null;
			}

			if (followButton != null) {
				followButton.Dispose ();
				followButton = null;
			}

			if (followersButton != null) {
				followersButton.Dispose ();
				followersButton = null;
			}

			if (followingButton != null) {
				followingButton.Dispose ();
				followingButton = null;
			}

			if (locationLabel != null) {
				locationLabel.Dispose ();
				locationLabel = null;
			}

			if (photosButton != null) {
				photosButton.Dispose ();
				photosButton = null;
			}

			if (progressBar != null) {
				progressBar.Dispose ();
				progressBar = null;
			}

			if (username != null) {
				username.Dispose ();
				username = null;
			}

			if (websiteHeight != null) {
				websiteHeight.Dispose ();
				websiteHeight = null;
			}

			if (websiteTextView != null) {
				websiteTextView.Dispose ();
				websiteTextView = null;
			}

			if (websiteView != null) {
				websiteView.Dispose ();
				websiteView = null;
			}

			if (stackView != null) {
				stackView.Dispose ();
				stackView = null;
			}
		}
	}
}
