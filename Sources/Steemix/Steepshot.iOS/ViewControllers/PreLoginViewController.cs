using System;
using System.Threading.Tasks;
using Foundation;
using Sweetshot.Library.Models.Requests;
using UIKit;

namespace Steepshot.iOS
{
	public partial class PreLoginViewController : BaseViewController
	{
		protected PreLoginViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logi  
		}
		public string newAccountNetwork;

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			networkSwitch.Layer.CornerRadius = 16;
			networkSwitch.On = UserContext.Instanse.Network == Constants.Steem;
			SetText();
			networkSwitch.ValueChanged += NetworkSwithed;
			loginButton.TouchDown += (sender, e) => GetUserInfo();
			loginLabel.Font = Constants.Bold175;
			signLabel.Font = Constants.Bold125;
			loginText.Font = Constants.Bold135;
			loginButton.Font = Constants.Heavy115;
			signUpButton.Font = Constants.Bold135;
			devSwitch.On = UserContext.Instanse.IsDev;
			devSwitch.ValueChanged += (sender, e) =>
			{
				UserContext.Instanse.IsDev = ((UISwitch)sender).On;
				SwitchApiAddress();
				UserContext.Save();
			};
			loginText.ShouldReturn += (textField) =>
			{
				loginText.ResignFirstResponder();
				return true;
			};

#if DEBUG
			loginText.Text = "joseph.kalu";
#endif

			var tw = new UILabel(new CoreGraphics.CGRect(0, 0, 120, NavigationController.NavigationBar.Frame.Height));
			tw.TextColor = UIColor.White;
			tw.Text = "PROFILE"; // to constants
			tw.BackgroundColor = UIColor.Clear;
			tw.TextAlignment = UITextAlignment.Center;
			tw.Font = Constants.Heavy165;
			NavigationItem.TitleView = tw;

			if (!string.IsNullOrEmpty(newAccountNetwork))
			{
				networkSwitch.Hidden = true;
				steemImg.Hidden = true;
				golosImg.Hidden = true;
				UserContext.Instanse.Network = newAccountNetwork;
                SwitchApiAddress();
			}

			UITapGestureRecognizer logoTap = new UITapGestureRecognizer(
				() =>
			{
				devSwitch.Hidden = !devSwitch.Hidden;
			}
			);
			logoTap.NumberOfTapsRequired = 5;
			logo.AddGestureRecognizer(logoTap);
		}

		public override void ViewDidDisappear(bool animated)
		{
			base.ViewDidDisappear(animated);
			if (IsMovingFromParentViewController && !string.IsNullOrEmpty(newAccountNetwork))
			{
				UserContext.Instanse.Network = newAccountNetwork == Constants.Steem ? Constants.Golos : Constants.Steem;
				SwitchApiAddress();
			}
			
		}

		private void NetworkSwithed(object sender, EventArgs e)
		{
			UserContext.Instanse.Network = ((UISwitch)sender).On ? Constants.Steem : Constants.Golos;
			SetText();
			UserContext.Save();
			SwitchApiAddress();
		}

		private void SetText()
		{
			loginLabel.Text = $"Log in with your {newAccountNetwork ?? UserContext.Instanse.Network} Account";
			signLabel.Text = $"Haven't {newAccountNetwork ?? UserContext.Instanse.Network} account yet?";
		}

		private async Task GetUserInfo()
		{
			activityIndicator.StartAnimating();
			loginButton.Enabled = false;
			try
			{
				var req = new UserProfileRequest(loginText.Text) { };
				var response = await Api.GetUserProfile(req);
				if (response.Success)
				{
					var myViewController = Storyboard.InstantiateViewController(nameof(LoginViewController)) as LoginViewController;
					myViewController.AvatarLink = response.Result.ProfileImage;
					myViewController.Username = response.Result.Username;
					this.NavigationController.PushViewController(myViewController, true);
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
					Message = "Login cannot be empty"
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
	}
}

