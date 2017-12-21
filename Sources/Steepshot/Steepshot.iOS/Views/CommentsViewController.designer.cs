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
	[Register ("CommentsViewController")]
	partial class CommentsViewController
	{
		[Outlet]
		UIKit.UIView bottomView { get; set; }

		[Outlet]
		UIKit.UITableView commentsTable { get; set; }

		[Outlet]
		UIKit.UITextView commentTextView { get; set; }

		[Outlet]
		UIKit.UIActivityIndicatorView progressBar { get; set; }

		[Outlet]
		UIKit.UIButton sendButton { get; set; }

		[Outlet]
		UIKit.UIActivityIndicatorView sendProgressBar { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint tableBottomToCommentView { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint tableBottomToSuperview { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (bottomView != null) {
				bottomView.Dispose ();
				bottomView = null;
			}

			if (commentsTable != null) {
				commentsTable.Dispose ();
				commentsTable = null;
			}

			if (commentTextView != null) {
				commentTextView.Dispose ();
				commentTextView = null;
			}

			if (progressBar != null) {
				progressBar.Dispose ();
				progressBar = null;
			}

			if (sendButton != null) {
				sendButton.Dispose ();
				sendButton = null;
			}

			if (tableBottomToCommentView != null) {
				tableBottomToCommentView.Dispose ();
				tableBottomToCommentView = null;
			}

			if (tableBottomToSuperview != null) {
				tableBottomToSuperview.Dispose ();
				tableBottomToSuperview = null;
			}

			if (sendProgressBar != null) {
				sendProgressBar.Dispose ();
				sendProgressBar = null;
			}
		}
	}
}
