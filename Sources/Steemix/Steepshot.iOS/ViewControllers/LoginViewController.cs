using System;
using System.Net;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Sweetshot.Library.Models.Requests;
using UIKit;

namespace Steepshot.iOS
{
	public partial class LoginViewController : BaseViewController
	{
		protected LoginViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public string AvatarLink { get; set; }
		public string Username { get; set; }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			loginButton.TouchDown += (object sender, EventArgs e) => Login();
			avatar.Layer.CornerRadius = avatar.Frame.Height / 2;
			eyeButton.TouchDown += (sender, e) =>
			{
				password.SecureTextEntry = !password.SecureTextEntry;
			};
			LoadImage(AvatarLink);
			loginTitle.Text = $"Hello, {Username}";
			loginTitle.Font = Constants.Bold225;
			postingLabel.Font = Constants.Bold175;
			password.Font = Constants.Bold135;
			loginButton.Font = Constants.Heavy115;
			postingKeyButton.Font = Constants.Bold15;
#if DEBUG
			password.Text = "5JXCxj6YyyGUTJo9434ZrQ5gfxk59rE3yukN42WBA6t58yTPRTG";
#endif
			password.ShouldReturn += (textField) =>
			{
				password.ResignFirstResponder();
				return true;
			};
			password.RightView = new UIView(new CGRect(0, 0, eyeButton.Frame.Width + 10, 0));
			password.RightViewMode = UITextFieldViewMode.Always;
			var tw = new UILabel(new CoreGraphics.CGRect(0, 0, 120, NavigationController.NavigationBar.Frame.Height));
			tw.TextColor = UIColor.White;
			tw.Text = "PROFILE"; // to constants
			tw.BackgroundColor = UIColor.Clear;
			tw.TextAlignment = UITextAlignment.Center;
			tw.Font = Constants.Heavy165;
			NavigationItem.TitleView = tw;
		}

		private async Task Login()
		{
			activityIndicator.StartAnimating();
			loginButton.Enabled = false;
			try
			{
				var request = new LoginWithPostingKeyRequest(Username, password.Text);
				var response = await Api.LoginWithPostingKey(request);

				if (response.Success)
				{
					UserContext.Instanse.Accounts.Add(new Account()
					{
						Network = UserContext.Instanse.Network,
						Login = Username,
						Token = response.Result.SessionId,
						Avatar = AvatarLink
					});

					UserContext.Save();

					UserContext.Instanse.IsHomeFeedLoaded = false;
					var myViewController = Storyboard.InstantiateViewController("MainTabBar") as UITabBarController; // MainTabBar to const
					this.NavigationController.ViewControllers = new UIViewController[] { myViewController, this };
					this.NavigationController.PopViewController(true);
				}
				else
				{
					UIAlertView alert = new UIAlertView()
					{
						Message = response.Errors[0]
					};
					alert.AddButton("OK");
					alert.Show();
				}
			}
			catch (ArgumentNullException ex)
			{
				UIAlertView alert = new UIAlertView()
				{
					Message = "Password cannot be empty"
				};
				alert.AddButton("OK");
				alert.Show();
			}
			catch (Exception ex)
			{

			}
			finally
			{
				loginButton.Enabled = true;
				activityIndicator.StopAnimating();
			}
		}

		public void LoadImage(string uri)
		{
			try
			{
				using (var webClient = new WebClient())
				{
					webClient.DownloadDataCompleted += (sender, e) =>
					{
						try
						{
							using (var data = NSData.FromArray(e.Result))
								avatar.Image = UIImage.LoadFromData(data);

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

