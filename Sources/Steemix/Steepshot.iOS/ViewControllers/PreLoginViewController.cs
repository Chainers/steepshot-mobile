/*using System;
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
			//networkSwitch.Layer.CornerRadius = 16;
			//networkSwitch.On = UserContext.Instanse.Network == Constants.Steem;
			SetText();
			//networkSwitch.ValueChanged += NetworkSwithed;
			loginButton.TouchDown += (sender, e) => GetUserInfo();
			loginLabel.Font = Constants.Bold175;
			signLabel.Font = Constants.Bold125;
			loginText.Font = Constants.Bold135;
			loginButton.Font = Constants.Heavy115;
			signUpButton.Font = Constants.Bold135;
			devSwitch.On = UserContext.Instanse.Dev;
			devSwitch.ValueChanged += (sender, e) =>
			{
				UserContext.Instanse.Dev = ((UISwitch)sender).On;
				SwitchApiAddress();
				UserContext.Save();
			};
			loginText.ShouldReturn += (textField) =>
			{
				loginText.ResignFirstResponder();
				return true;
			};

			if ((float)UIScreen.MainScreen.Bounds.Height < 500)
			{
				aboveConstant.Constant = 2;
				belowConstant.Constant = 2;
			}

#if DEBUG
			loginText.Text = "joseph.kalu";
#endif

			//picker.Hidden = true;
			//pickerHeight.Constant = 0;

			var tw = new UILabel(new CoreGraphics.CGRect(0, 0, 120, NavigationController.NavigationBar.Frame.Height));
			tw.TextColor = UIColor.White;
			tw.Text = "PROFILE"; // to constants
			tw.BackgroundColor = UIColor.Clear;
			tw.TextAlignment = UITextAlignment.Center;
			tw.Font = Constants.Heavy165;
			NavigationItem.TitleView = tw;

			picker.Model = new NetworkPickerViewModel(NetworkSwithed);
			picker.Select(Convert.ToInt32(UserContext.Instanse.Network != Constants.Steem), 0, true);
			        
			if (!string.IsNullOrEmpty(newAccountNetwork))
			{
				picker.Hidden = true;
				//steemImg.Hidden = true;
				//golosImg.Hidden = true;
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

			signUpButton.TouchDown += (sender, e) =>
			{
				var myViewController = Storyboard.InstantiateViewController(nameof(WebPageViewController)) as WebPageViewController;
				this.NavigationController.PushViewController(myViewController, true);
			};
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

		private void NetworkSwithed(string network)
		{
			UserContext.Instanse.Network = network;
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
                    ShowAlert(response.Errors[0]);
				}
			}
			catch (ArgumentNullException ex)
			{
				ShowAlert("Login cannot be empty");
			}
			catch (Exception ex)
			{
				Reporter.SendCrash(ex);
			}
			finally
			{
				loginButton.Enabled = true;
				activityIndicator.StopAnimating();
			}
		}
	}

	public class NetworkPickerViewModel : UIPickerViewModel
	{
		private Action<string> _switched;

		public NetworkPickerViewModel(Action<string> switched)
		{
			_switched = switched;
		}

		public override nint GetComponentCount(UIPickerView pickerView)
		{
			return 1;
		}

		public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
		{
			return 2;
		}

		public override string GetTitle(UIPickerView pickerView, nint row, nint component)
		{
			return GetTitle(row);
		}

		public override void Selected(UIPickerView pickerView, nint row, nint component)
		{
			_switched(GetTitle(row));
		}

		private string GetTitle(nint row)
		{
			switch (row)
			{
				case 0:
					return Constants.Steem;
				case 1:
					return Constants.Golos;
				default:
					return string.Empty;
			}
		}
	}
}

*/