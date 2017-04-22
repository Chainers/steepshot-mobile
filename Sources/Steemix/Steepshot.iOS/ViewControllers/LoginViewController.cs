using System;
using System.Net;
using System.Threading.Tasks;
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

			password.ShouldReturn += (textField) =>
			{
				password.ResignFirstResponder();
				return true;
			};

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

					var myViewController = Storyboard.InstantiateViewController("MainTabBar") as UITabBarController; // MainTabBar to const
					this.NavigationController.ViewControllers = new UIViewController[2] { myViewController, this };
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

