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
	[Register ("FeedTableViewCell")]
	partial class FeedTableViewCell
	{
		[Outlet]
		UIKit.UIImageView avatarImage { get; set; }

		[Outlet]
		UIKit.UIImageView bodyImage { get; set; }

		[Outlet]
		UIKit.UILabel cellText { get; set; }

		[Outlet]
		UIKit.UILabel commentAuthor { get; set; }

		[Outlet]
		UIKit.UILabel commentText { get; set; }

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

			if (netVotes != null) {
				netVotes.Dispose ();
				netVotes = null;
			}

			if (rewards != null) {
				rewards.Dispose ();
				rewards = null;
			}

			if (likeButton != null) {
				likeButton.Dispose ();
				likeButton = null;
			}

			if (commentAuthor != null) {
				commentAuthor.Dispose ();
				commentAuthor = null;
			}

			if (commentText != null) {
				commentText.Dispose ();
				commentText = null;
			}

			if (viewCommentButton != null) {
				viewCommentButton.Dispose ();
				viewCommentButton = null;
			}
		}
	}
}
