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
	[Register ("FeedCollectionViewCell")]
	partial class FeedCollectionViewCell
	{
		[Outlet]
		UIKit.UIImageView avatarImage { get; set; }

		[Outlet]
		UIKit.UIImageView bodyImage { get; set; }

		[Outlet]
		UIKit.UILabel cellText { get; set; }

		[Outlet]
		UIKit.UILabel commentText { get; set; }

		[Outlet]
		UIKit.UIView commentView { get; set; }

		[Outlet]
		UIKit.UIView contentView { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint contentViewWidth { get; set; }

		[Outlet]
		UIKit.UIView flagButton { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint imageWidth { get; set; }

		[Outlet]
		UIKit.UIButton likeButton { get; set; }

		[Outlet]
		UIKit.UILabel netVotes { get; set; }

		[Outlet]
		UIKit.UILabel rewards { get; set; }

		[Outlet]
		UIKit.UIButton viewCommentButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (avatarImage != null) {
				avatarImage.Dispose ();
				avatarImage = null;
			}

			if (bodyImage != null) {
				bodyImage.Dispose ();
				bodyImage = null;
			}

			if (cellText != null) {
				cellText.Dispose ();
				cellText = null;
			}

			if (commentText != null) {
				commentText.Dispose ();
				commentText = null;
			}

			if (commentView != null) {
				commentView.Dispose ();
				commentView = null;
			}

			if (contentView != null) {
				contentView.Dispose ();
				contentView = null;
			}

			if (contentViewWidth != null) {
				contentViewWidth.Dispose ();
				contentViewWidth = null;
			}

			if (imageWidth != null) {
				imageWidth.Dispose ();
				imageWidth = null;
			}

			if (likeButton != null) {
				likeButton.Dispose ();
				likeButton = null;
			}

			if (netVotes != null) {
				netVotes.Dispose ();
				netVotes = null;
			}

			if (rewards != null) {
				rewards.Dispose ();
				rewards = null;
			}

			if (viewCommentButton != null) {
				viewCommentButton.Dispose ();
				viewCommentButton = null;
			}

			if (flagButton != null) {
				flagButton.Dispose ();
				flagButton = null;
			}
		}
	}
}
