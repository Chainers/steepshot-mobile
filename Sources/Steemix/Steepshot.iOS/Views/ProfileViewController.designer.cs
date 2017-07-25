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
	[Register ("ProfileViewController")]
	partial class ProfileViewController
	{
		[Outlet]
		UIKit.UICollectionView collectionView { get; set; }

		[Outlet]
		UIKit.UILabel errorMessage { get; set; }

		[Outlet]
		UIKit.UIActivityIndicatorView loading { get; set; }
		
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
		}
	}
}
