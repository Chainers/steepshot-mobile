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
		public event VoteEventHandler<OperationResult<VoteResponse>> Voted;
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
			newFrame.Size = new CGSize(size);
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


			cellText.Text = _currentPost.Author;
			rewards.Text = $"{Constants.Currency}{_currentPost.TotalPayoutReward.ToString()}";
			netVotes.Text = $"{_currentPost.NetVotes.ToString()} likes";
			likeButton.Selected = _currentPost.Vote;
			flagButton.Selected = _currentPost.Flag;
			var nicknameAttribute = new UIStringAttributes
			{
				Font = UIFont.BoldSystemFontOfSize(commentText.Font.PointSize)
			};
			NSMutableAttributedString at = new NSMutableAttributedString();
			at.Append(new NSAttributedString(_currentPost.Author, nicknameAttribute));
			at.Append(new NSAttributedString(" "));
			at.Append(new NSAttributedString(_currentPost.Title));
			commentText.AttributedText = at;
			var buttonTitle = _currentPost.Children == 0 ? "Post first comment" : $"View {_currentPost.Children} comments";
			viewCommentButton.SetTitle(buttonTitle, UIControlState.Normal);
			likeButton.Enabled = true;
			flagButton.Enabled = true;
			var period = DateTime.UtcNow.Subtract(_currentPost.Created);

			if (period.Days / 365 != 0)
			{
				postTimeStamp.Text = $"{period.Days / 365} y";
			}
			else if(period.Days / 30 != 0)
			{
				postTimeStamp.Text = $"{period.Days / 30} M";
			}
			else if (period.Days != 0)
			{
				postTimeStamp.Text = $"{period.Days} d";
			}
			else if (period.Hours != 0)
			{
				postTimeStamp.Text = $"{period.Hours} h";
			}
			else if (period.Minutes != 0)
			{
				postTimeStamp.Text = $"{period.Minutes} m";
			}
			else if (period.Seconds != 0)
			{
				postTimeStamp.Text = $"{period.Seconds} s";
			}

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
				if (url == _currentPost.Url && post.Success)
				{
					likeButton.Selected = post.Result.IsVoted;
					flagButton.Selected = _currentPost.Flag;
					rewards.Text = $"{Constants.Currency}{post.Result.NewTotalPayoutReward.ToString()}";
					netVotes.Text = $"{_currentPost.NetVotes.ToString()} likes";
				}
				likeButton.Enabled = true;
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
					likeButton.Selected = _currentPost.Vote;
					netVotes.Text = $"{_currentPost.NetVotes.ToString()} likes";
					rewards.Text = $"{Constants.Currency}{post.Result.NewTotalPayoutReward.ToString()}";
				}
				flagButton.Enabled = true;
			});
		}
	}
}
