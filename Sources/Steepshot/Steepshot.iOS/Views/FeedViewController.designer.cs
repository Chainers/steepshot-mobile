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
	[Register ("FeedViewController")]
	partial class FeedViewController
	{
		[Outlet]
		UIKit.UIActivityIndicatorView activityIndicator { get; set; }

		[Outlet]
		UIKit.UICollectionView feedCollection { get; set; }

		[Outlet]
		UIKit.UICollectionViewFlowLayout flowLayout { get; set; }

		[Outlet]
		UIKit.UILabel noFeedLabel { get; set; }

		[Outlet]
		UIKit.UICollectionView sliderCollection { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (activityIndicator != null) {
				activityIndicator.Dispose ();
				activityIndicator = null;
			}

			if (feedCollection != null) {
				feedCollection.Dispose ();
				feedCollection = null;
			}

			if (flowLayout != null) {
				flowLayout.Dispose ();
				flowLayout = null;
			}

			if (noFeedLabel != null) {
				noFeedLabel.Dispose ();
				noFeedLabel = null;
			}

			if (sliderCollection != null) {
				sliderCollection.Dispose ();
				sliderCollection = null;
			}
		}
	}
}
