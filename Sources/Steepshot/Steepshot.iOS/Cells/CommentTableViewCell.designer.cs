// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
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

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton flagButton { get; set; }

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

            if (flagButton != null) {
                flagButton.Dispose ();
                flagButton = null;
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