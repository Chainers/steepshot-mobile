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
	[Register ("LocalTagCollectionViewCell")]
	partial class LocalTagCollectionViewCell
	{
		[Outlet]
		UIKit.UIView rootView { get; set; }

		[Outlet]
		UIKit.UILabel tagText { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (rootView != null) {
				rootView.Dispose ();
				rootView = null;
			}

			if (tagText != null) {
				tagText.Dispose ();
				tagText = null;
			}
		}
	}
}
