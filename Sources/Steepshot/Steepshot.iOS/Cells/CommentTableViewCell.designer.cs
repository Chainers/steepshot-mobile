// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Steepshot.iOS.Cells
{
	[Register ("CommentTableViewCell")]
	partial class CommentTableViewCell
	{
		[Outlet]
		UIKit.UIImageView avatar { get; set; }

		[Outlet]
		UIKit.UILabel bodyLabel { get; set; }

		[Outlet]
		UIKit.UILabel costLabel { get; set; }

		[Outlet]
		UIKit.UIButton likeButton { get; set; }

		[Outlet]
		UIKit.UILabel likeLabel { get; set; }

		[Outlet]
		UIKit.UILabel loginLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (avatar != null) {
				avatar.Dispose ();
				avatar = null;
			}

			if (bodyLabel != null) {
				bodyLabel.Dispose ();
				bodyLabel = null;
			}

			if (costLabel != null) {
				costLabel.Dispose ();
				costLabel = null;
			}

			if (likeButton != null) {
				likeButton.Dispose ();
				likeButton = null;
			}

			if (likeLabel != null) {
				likeLabel.Dispose ();
				likeLabel = null;
			}

			if (loginLabel != null) {
				loginLabel.Dispose ();
				loginLabel = null;
			}
		}
	}
}
