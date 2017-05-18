using System;
using System.Collections.Generic;
using System.Net;
using Foundation;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;
using UIKit;

namespace Steepshot.iOS
{
	public delegate void FollowEventHandler(FollowType followType, string authorName, Action<string, bool?> success);

	public partial class FollowViewCell : UITableViewCell
	{
		public static readonly NSString Key = new NSString("FollowViewCell");
		public static readonly UINib Nib;
		private List<WebClient> webClients = new List<WebClient>();
		private bool isButtonBinded = false;
		public event FollowEventHandler Follow;
		public event HeaderTappedHandler GoToProfile;
		private UserFriend _currentUser;

		public bool IsFollowSet
		{
			get
			{
				return Follow != null;
			}
		}

		public bool IsGoToProfileSet
		{
			get
			{
				return GoToProfile != null;
			}
		}

		static FollowViewCell()
		{
			Nib = UINib.FromName("FollowViewCell", NSBundle.MainBundle);
		}

		protected FollowViewCell(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public override void LayoutSubviews()
		{
			avatar.Layer.CornerRadius = avatar.Frame.Size.Width / 2;
			followButton.Layer.CornerRadius = 5;
			followButton.Layer.BorderWidth = 2;
			followButton.Layer.BorderColor = Constants.Blue.CGColor;
			followButton.ContentEdgeInsets = new UIEdgeInsets(10, 10, 10, 10);
			followButton.Hidden = UserContext.Instanse.Username == null;
			base.LayoutSubviews();
		}

		public void UpdateCell(UserFriend user)
		{
			_currentUser = user;
			userName.Text = user.Author;
			followButton.SetTitle(user.HasFollowed ? "UNFOLLOW" : "FOLLOW", UIControlState.Normal);

			followButton.Enabled = true;
			progressBar.StopAnimating();

			if (!isButtonBinded)
			{
				UITapGestureRecognizer tap = new UITapGestureRecognizer(() =>
				{
					GoToProfile(_currentUser.Author);
				});
				avatar.AddGestureRecognizer(tap);
			}

			if (!isButtonBinded)
			{
				followButton.TouchDown += (sender, e) =>
				{
					followButton.Enabled = false;
					progressBar.StartAnimating();
					Follow(_currentUser.HasFollowed ? FollowType.UnFollow : FollowType.Follow, _currentUser.Author, (author, success) =>
					{
						if (author == _currentUser.Author && success != null)
						{
							followButton.SetTitle((bool)success ? "UNFOLLOW" : "FOLLOW", UIControlState.Normal);
							followButton.Enabled = true;
							progressBar.StopAnimating();
						}
					});
				};
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
			LoadImage(user.Avatar, avatar, UIImage.FromBundle("ic_user_placeholder"));
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
