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
	[Register ("PreSearchViewController")]
	partial class PreSearchViewController
	{
		[Outlet]
		UIKit.UIActivityIndicatorView activityIndicator { get; set; }

		[Outlet]
		UIKit.UICollectionView collectionView { get; set; }

		[Outlet]
		UIKit.UIButton hotButton { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint hotConstrain { get; set; }

		[Outlet]
		UIKit.UIButton loginButton { get; set; }

		[Outlet]
		UIKit.UIButton newButton { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint newConstraint { get; set; }

		[Outlet]
		UIKit.UILabel noFeedLabel { get; set; }

		[Outlet]
		UIKit.UIView searchButton { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint searchHeight { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint searchTopMargin { get; set; }

		[Outlet]
		UIKit.UICollectionView sliderCollection { get; set; }

		[Outlet]
		UIKit.UIButton switcher { get; set; }

		[Outlet]
		UIKit.UIButton topButton { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint topConstraint { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (activityIndicator != null) {
				activityIndicator.Dispose ();
				activityIndicator = null;
			}

			if (collectionView != null) {
				collectionView.Dispose ();
				collectionView = null;
			}

			if (hotButton != null) {
				hotButton.Dispose ();
				hotButton = null;
			}

			if (hotConstrain != null) {
				hotConstrain.Dispose ();
				hotConstrain = null;
			}

			if (loginButton != null) {
				loginButton.Dispose ();
				loginButton = null;
			}

			if (newButton != null) {
				newButton.Dispose ();
				newButton = null;
			}

			if (newConstraint != null) {
				newConstraint.Dispose ();
				newConstraint = null;
			}

			if (noFeedLabel != null) {
				noFeedLabel.Dispose ();
				noFeedLabel = null;
			}

			if (searchButton != null) {
				searchButton.Dispose ();
				searchButton = null;
			}

			if (searchHeight != null) {
				searchHeight.Dispose ();
				searchHeight = null;
			}

			if (searchTopMargin != null) {
				searchTopMargin.Dispose ();
				searchTopMargin = null;
			}

			if (switcher != null) {
				switcher.Dispose ();
				switcher = null;
			}

			if (topButton != null) {
				topButton.Dispose ();
				topButton = null;
			}

			if (topConstraint != null) {
				topConstraint.Dispose ();
				topConstraint = null;
			}

			if (sliderCollection != null) {
				sliderCollection.Dispose ();
				sliderCollection = null;
			}
		}
	}
}
