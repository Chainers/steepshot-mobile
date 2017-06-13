using System;
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
		protected FeedCollectionViewCell(IntPtr handle) : base(handle)
		{
			
		}
		public static readonly NSString Key = new NSString("FeedCollectionViewCell");
		public static readonly UINib Nib;

		static FeedCollectionViewCell()
		{
			Nib = UINib.FromName("FeedCollectionViewCell", NSBundle.MainBundle);
		}

		private bool isButtonBinded = false;
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

		public override void UpdateCell(Post post, NSMutableAttributedString comment)
		{
			_currentPost = post;
			avatarImage.Image = null;
			_scheduledWorkAvatar?.Cancel();

			bodyImage.Image = null;
			_scheduledWorkBody?.Cancel();

			_scheduledWorkAvatar = ImageService.Instance.LoadUrl(_currentPost.Avatar, TimeSpan.FromDays(30))
													 .WithCache(FFImageLoading.Cache.CacheType.All)
													 .Retry(2, 200)
													 .DownSample(width: 20)
													 .Into(avatarImage);


			_scheduledWorkBody = ImageService.Instance.LoadUrl(_currentPost.Body, Constants.ImageCacheDuration)
													 .WithCache(FFImageLoading.Cache.CacheType.All)
													 .Retry(2, 200)
													 .DownSample(width: 200)
													 .Into(bodyImage);

			cellText.Text = _currentPost.Author;
			rewards.Text = $"{Constants.Currency}{_currentPost.TotalPayoutReward.ToString()}";
			netVotes.Text = $"{_currentPost.NetVotes.ToString()} likes";
			likeButton.Selected = _currentPost.Vote;
			flagButton.Selected = _currentPost.Flag;
			commentText.AttributedText = comment;

			var buttonTitle = _currentPost.Children == 0 ? "Post first comment" : $"View {_currentPost.Children} comments";
			viewCommentButton.SetTitle(buttonTitle, UIControlState.Normal);
			likeButton.Enabled = true;
			flagButton.Enabled = true;
			postTimeStamp.Text = _currentPost.Created.ToPostTime();

			if (!isButtonBinded)
			{
				avatarImage.Layer.CornerRadius = avatarImage.Frame.Size.Width / 2;
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
				flagButton.Selected = _currentPost.Flag;
				flagButton.Enabled = true;
			});
		}
	}
}
