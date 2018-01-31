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
		UIKit.UITextView commentText { get; set; }

		[Outlet]
		UIKit.UILabel costLabel { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint flagHiddenConstraint { get; set; }

		[Outlet]
		UIKit.UILabel flagLabel { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint flagVisibleConstraint { get; set; }

		[Outlet]
		UIKit.UIButton likeButton { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint likeHiddenConstraint { get; set; }

		[Outlet]
		UIKit.UILabel likeLabel { get; set; }

		[Outlet]
		UIKit.UILabel loginLabel { get; set; }

		[Outlet]
		UIKit.UIButton otherActionButton { get; set; }

		[Outlet]
		UIKit.UILabel replyButton { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint replyHiddenConstraint { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint replyVisibleConstraint { get; set; }

		[Outlet]
		UIKit.UILabel timestamp { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (avatar != null) {
				avatar.Dispose ();
				avatar = null;
			}

			if (commentText != null) {
				commentText.Dispose ();
				commentText = null;
			}

			if (costLabel != null) {
				costLabel.Dispose ();
				costLabel = null;
			}

			if (flagHiddenConstraint != null) {
				flagHiddenConstraint.Dispose ();
				flagHiddenConstraint = null;
			}

			if (flagLabel != null) {
				flagLabel.Dispose ();
				flagLabel = null;
			}

			if (flagVisibleConstraint != null) {
				flagVisibleConstraint.Dispose ();
				flagVisibleConstraint = null;
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

			if (otherActionButton != null) {
				otherActionButton.Dispose ();
				otherActionButton = null;
			}

			if (replyButton != null) {
				replyButton.Dispose ();
				replyButton = null;
			}

			if (replyHiddenConstraint != null) {
				replyHiddenConstraint.Dispose ();
				replyHiddenConstraint = null;
			}

			if (replyVisibleConstraint != null) {
				replyVisibleConstraint.Dispose ();
				replyVisibleConstraint = null;
			}

			if (timestamp != null) {
				timestamp.Dispose ();
				timestamp = null;
			}

			if (likeHiddenConstraint != null) {
				likeHiddenConstraint.Dispose ();
				likeHiddenConstraint = null;
			}
		}
	}
}
