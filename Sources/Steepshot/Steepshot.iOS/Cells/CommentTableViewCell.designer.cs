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
	[Register ("CommentTableViewCell")]
	partial class CommentTableViewCell
	{
		[Outlet]
		UIKit.UIImageView avatar { get; set; }

		[Outlet]
		UIKit.UILabel bodyLabel { get; set; }

		[Outlet]
		UIKit.UITextView commentText { get; set; }

		[Outlet]
		UIKit.UITextView commentTextView { get; set; }

		[Outlet]
		UIKit.UILabel costLabel { get; set; }

		[Outlet]
		UIKit.UIButton likeButton { get; set; }

		[Outlet]
		UIKit.UILabel likeLabel { get; set; }

		[Outlet]
		UIKit.UILabel loginLabel { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint textViewHeight { get; set; }
		
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

			if (commentTextView != null) {
				commentTextView.Dispose ();
				commentTextView = null;
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

			if (commentText != null) {
				commentText.Dispose ();
				commentText = null;
			}

			if (textViewHeight != null) {
				textViewHeight.Dispose ();
				textViewHeight = null;
			}
		}
	}
}
