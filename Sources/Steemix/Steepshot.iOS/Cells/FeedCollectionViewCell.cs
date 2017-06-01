using System;
using System.Collections.Generic;
using System.Net;
using CoreGraphics;
using FFImageLoading;
using FFImageLoading.Work;
using Foundation;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Responses;
using UIKit;

namespace Steepshot.iOS
{
	public partial class FeedCollectionViewCell : BaseProfileCell
	{
		protected FeedCollectionViewCell(IntPtr handle) : base(handle) {}
		public static readonly NSString Key = new NSString("FeedCollectionViewCell");
		public static readonly UINib Nib;

		static FeedCollectionViewCell()
		{
			Nib = UINib.FromName("FeedCollectionViewCell", NSBundle.MainBundle);
		}

		private bool isButtonBinded = false;
		private List<WebClient> webClients = new List<WebClient>();
		public event VoteEventHandler<VoteResponse> Voted;
		public event VoteEventHandler<OperationResult<FlagResponse>> Flagged;
		public event HeaderTappedHandler GoToProfile;
		public event HeaderTappedHandler GoToComments;
		public event ImagePreviewHandler ImagePreview;
		private Post _currentPost;

		public bool IsVotedSet => Voted != null;
		public bool IsFlaggedSet => Flagged != null;
		public bool IsGoToProfileSet => GoToProfile != null;
		public bool IsGoToCommentsSet => GoToComments != null;
		public bool IsImagePreviewSet => ImagePreview != null;
		private IScheduledWork _scheduledWorkAvatar;
		private IScheduledWork _scheduledWorkBody;

		public override UICollectionViewLayoutAttributes PreferredLayoutAttributesFittingAttributes(UICollectionViewLayoutAttributes layoutAttributes)
		{
			contentViewWidth.Constant = UIScreen.MainScreen.Bounds.Width;
			var size = contentView.SystemLayoutSizeFittingSize(layoutAttributes.Size);
			var newFrame = layoutAttributes.Frame;
			newFrame.Size = new CGSize(newFrame.Size.Width, size.Height);
			layoutAttributes.Frame = newFrame;
			return layoutAttributes;
		}

		public override void LayoutSubviews()
		{
			avatarImage.Layer.CornerRadius = avatarImage.Frame.Size.Width / 2;
			base.LayoutSubviews();
		}

		public override void UpdateCell(Post post)
		{
			_currentPost = post;
			avatarImage.Image = null;
			_scheduledWorkAvatar?.Cancel();

			bodyImage.Image = null;
			_scheduledWorkBody?.Cancel();

			_scheduledWorkAvatar = ImageService.Instance.LoadUrl(_currentPost.Avatar, TimeSpan.FromDays(30))
													 .Retry(2, 200)
													 .FadeAnimation(false, false, 0)
													 .DownSample(width: (int)avatarImage.Frame.Width)
													 .Into(avatarImage);


			_scheduledWorkBody = ImageService.Instance.LoadUrl(_currentPost.Body, Constants.ImageCacheDuration)
													 .Retry(2, 200)
													 .FadeAnimation(false, false, 0)
													 .DownSample(width: (int)bodyImage.Frame.Width)
													 .Into(bodyImage);

			_currentPost = post;
			cellText.Text = post.Author;
			rewards.Text = $"{Constants.Currency}{post.TotalPayoutReward.ToString()}";
			netVotes.Text = $"{post.NetVotes.ToString()} likes";
			likeButton.Selected = post.Vote;
			flagButton.Selected = post.Flag;
			var nicknameAttribute = new UIStringAttributes
			{
				Font = UIFont.BoldSystemFontOfSize(commentText.Font.PointSize)
			};
			NSMutableAttributedString at = new NSMutableAttributedString();
			at.Append(new NSAttributedString(post.Author, nicknameAttribute));
			at.Append(new NSAttributedString(" "));
			at.Append(new NSAttributedString(post.Title));
			commentText.AttributedText = at;
			var buttonTitle = post.Children == 0 ? "Post first comment" : $"View {post.Children} comments";
			viewCommentButton.SetTitle(buttonTitle, UIControlState.Normal);
			likeButton.Enabled = true;
			flagButton.Enabled = true;

			if (!isButtonBinded)
			{
				UITapGestureRecognizer tap = new UITapGestureRecognizer(() =>
				{
					ImagePreview(bodyImage.Image, _currentPost.Body);
				});
				bodyImage.AddGestureRecognizer(tap);

				UITapGestureRecognizer imageTap = new UITapGestureRecognizer(() =>
				{
					GoToProfile(_currentPost.Author);
				});
				UITapGestureRecognizer textTap = new UITapGestureRecognizer(() =>
				{
					GoToProfile(_currentPost.Author);
				});
				avatarImage.AddGestureRecognizer(imageTap);
				cellText.AddGestureRecognizer(textTap);

				UITapGestureRecognizer commentTap = new UITapGestureRecognizer(() =>
				{
					GoToComments(_currentPost.Url);
				});
				commentView.AddGestureRecognizer(commentTap);

				flagButton.TouchDown += FlagButton_TouchDown;
				likeButton.TouchDown += LikeTap;
				isButtonBinded = true;
			}
		}

		private void LikeTap(object sender, EventArgs e)
		{
			likeButton.Enabled = false;
			Voted(!likeButton.Selected, _currentPost.Url, (url, post) =>
			{
				if (url == _currentPost.Url)
				{
					likeButton.Selected = post.IsVoted;
					likeButton.Enabled = true;
					rewards.Text = $"{Constants.Currency}{post.NewTotalPayoutReward.ToString()}";
					netVotes.Text = $"{_currentPost.NetVotes.ToString()} likes";
				}
			});
		}

		private void FlagButton_TouchDown(object sender, EventArgs e)
		{
			flagButton.Enabled = false;
            Flagged(!flagButton.Selected, _currentPost.Url, (url, post) =>
			{
				if (url == _currentPost.Url && post.Success)
				{
					flagButton.Selected = post.Result.IsFlagged;
				}
				flagButton.Enabled = true;
			});
		}
	}
}
