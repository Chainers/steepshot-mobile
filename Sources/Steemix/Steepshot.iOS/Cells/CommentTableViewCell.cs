using System;
using System.Collections.Generic;
using System.Net;
using Foundation;
using Sweetshot.Library.Models.Responses;
using UIKit;

namespace Steepshot.iOS
{
	public partial class CommentTableViewCell : UITableViewCell
	{
		public static readonly NSString Key = new NSString("CommentTableViewCell");
		public static readonly UINib Nib;
		private List<WebClient> webClients = new List<WebClient>();
		private bool isButtonBinded = false;
		public event VoteEventHandler Voted;
		public event HeaderTappedHandler GoToProfile;
		private string PostUrl;

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

		static CommentTableViewCell()
		{
			Nib = UINib.FromName("CommentTableViewCell", NSBundle.MainBundle);
		}

		protected CommentTableViewCell(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public override void LayoutSubviews()
		{
			avatar.Layer.CornerRadius = avatar.Frame.Size.Width / 2;
			base.LayoutSubviews();
		}

		public void UpdateCell(Post post)
		{
			PostUrl = post.Url;
			bodyLabel.Text = post.Body;
			loginLabel.Text = post.Author;
			likeLabel.Text = post.NetVotes.ToString();
			costLabel.Text = $"${post.TotalPayoutReward}";
			likeButton.Selected = post.Vote;
			likeButton.Enabled = true;

			if (!isButtonBinded)
			{
				UITapGestureRecognizer tap = new UITapGestureRecognizer(() =>
				{
					GoToProfile(post.Author);
				});
				avatar.AddGestureRecognizer(tap);
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

			LoadImage(post.Avatar, avatar, UIImage.FromBundle("ic_user_placeholder"));
		}

		private void LikeTap(object sender, EventArgs e)
		{
			likeButton.Enabled = false;
			Voted(!likeButton.Selected, PostUrl, (postUrl, post) =>
			{
				if (postUrl == PostUrl)
				{
					likeButton.Selected = post.IsVoted;
					likeButton.Enabled = true;
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
					webClient.DownloadDataCompleted += (sender, e) =>
					{
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
