// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using Foundation;

namespace Steepshot.iOS.Views
{
	[Register ("ProfileViewController")]
	partial class ProfileViewController
	{
		[Outlet]
		UIKit.UICollectionView collectionView { get; set; }

		[Outlet]
		UIKit.UILabel errorMessage { get; set; }

		[Outlet]
		UIKit.UIActivityIndicatorView loading { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint topViewHeight { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (collectionView != null) {
				collectionView.Dispose ();
				collectionView = null;
			}

			if (errorMessage != null) {
				errorMessage.Dispose ();
				errorMessage = null;
			}

			if (loading != null) {
				loading.Dispose ();
				loading = null;
			}

			if (topViewHeight != null) {
				topViewHeight.Dispose ();
				topViewHeight = null;
			}
		}
	}
}
