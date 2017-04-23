using System;
using System.Collections.Generic;
using System.Net;
using Foundation;
using Sweetshot.Library.Models.Responses;
using UIKit;

namespace Steepshot.iOS
{
	public delegate void HeaderTappedHandler(string username);
	public delegate void ImagePreviewHandler(UIImage image);
	public delegate void VoteEventHandler(bool vote, string postUri, Action<string, VoteResponse> success);

    public partial class FeedTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("FeedTableViewCell");
        public static readonly UINib Nib;

        private bool isButtonBinded = false;
        private List<WebClient> webClients = new List<WebClient>();
        public event VoteEventHandler Voted;
		public event HeaderTappedHandler GoToProfile;
		public event HeaderTappedHandler GoToComments;
		public event ImagePreviewHandler ImagePreview;
		private Post _currentPost;

        public bool IsVotedSet
        {
            get
			{
                return Voted != null;
            }
        }

		public bool IsGoToProfileSet
		{
			get
			{
				return GoToProfile != null;
			}
		}

		public bool IsGoToCommentsSet
		{
			get
			{
				return GoToComments != null;
			}
		}

		public bool IsImagePreviewSet
		{
			get
			{
				return ImagePreview != null;
			}
		}

        static FeedTableViewCell()
        {
            Nib = UINib.FromName("FeedTableViewCell", NSBundle.MainBundle);
        }

        protected FeedTableViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void LayoutSubviews()
        {
            avatarImage.Layer.CornerRadius = avatarImage.Frame.Size.Width / 2;
            base.LayoutSubviews();
        }

        public void UpdateCell(Post post)
        {
			_currentPost = post;
            cellText.Text = post.Author;
			rewards.Text = $"{Constants.Currency}{post.TotalPayoutReward.ToString()}";
            netVotes.Text = $"{post.NetVotes.ToString()} likes";
            likeButton.Selected = post.Vote;
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


			if (!isButtonBinded)
			{
				UITapGestureRecognizer tap = new UITapGestureRecognizer(() =>
				{
					ImagePreview(bodyImage.Image);
				});
				bodyImage.AddGestureRecognizer(tap);
			}

			if (!isButtonBinded)
			{
				UITapGestureRecognizer tap = new UITapGestureRecognizer(() =>
				{
					GoToProfile(post.Author);
				});
				avatarImage.AddGestureRecognizer(tap);
			}

			if (!isButtonBinded)
			{
				viewCommentButton.TouchDown += (sender, e) =>
				{
					GoToComments(post.Url);
				};
			}

            if (!isButtonBinded)
            {
                likeButton.TouchDown += LikeTap;
                isButtonBinded = true;
            }

            foreach (var webClient in webClients)
            {
                if (webClient != null)
                {
                    webClient.CancelAsync();
                    webClient.Dispose();
                }
            }
            LoadImage(post.Avatar, avatarImage, UIImage.FromBundle("ic_user_placeholder"));
            LoadImage(post.Body, bodyImage, UIImage.FromBundle("ic_photo_holder"));
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

					_currentPost.NetVotes++;
					netVotes.Text = $"{_currentPost.NetVotes.ToString()} likes";
                }
            });

        }

        public void LoadImage(string uri, UIImageView imageView, UIImage defaultPicture)
        {
            try
            {
                imageView.Image = defaultPicture;
                using (var webClient = new WebClient())
                {
                    webClients.Add(webClient);
                    webClient.DownloadDataCompleted += (sender, e) => {
                        try
                        {
                            using (var data = NSData.FromArray(e.Result))
                                imageView.Image = UIImage.LoadFromData(data);

                            /*string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                            string localFilename = "downloaded.png";
                            string localPath = Path.Combine(documentsPath, localFilename);
                            File.WriteAllBytes(localPath, bytes); // writes to local storage*/
                        }
                        catch (Exception ex)
                        {
                            //Logging
                        }
                    };
                    webClient.DownloadDataAsync(new Uri(uri));
                }
            }
            catch (Exception ex)
            {
                //Logging
            }
        }
    }
}
