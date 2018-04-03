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
	[Register ("DescriptionViewController")]
	partial class DescriptionViewController
	{
		[Outlet]
		UIKit.UIScrollView mainScroll { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (mainScroll != null) {
				mainScroll.Dispose ();
				mainScroll = null;
			}
		}
	}
}
