using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Foundation;
using MessageUI;
using Sweetshot.Library;
using Sweetshot.Library.Models.Requests;
using UIKit;

namespace Steepshot.iOS
{
	public partial class SettingsViewController : BaseViewController
	{
		protected SettingsViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logi   
		}

		public SettingsViewController()
		{
		}

		private Account steemAcc;
		private Account golosAcc;

		private string previousNetwork;
		private MFMailComposeViewController mailController;

		public override void ViewDidLoad()
		{
			NavigationController.NavigationBar.Translucent = false;
			base.ViewDidLoad();
			nsfwSwitch.On = UserContext.Instanse.NSFW;
			CheckNsfw();
			CheckLowRated();
			NavigationController.SetNavigationBarHidden(false, false);
			steemAcc = UserContext.Instanse.Accounts.FirstOrDefault(a => a.Network == Constants.Steem);
			golosAcc = UserContext.Instanse.Accounts.FirstOrDefault(a => a.Network == Constants.Golos);
			previousNetwork = UserContext.Instanse.Network;

			//steemAvatar.Layer.CornerRadius = steemAvatar.Frame.Width / 2;
			//golosAvatar.Layer.CornerRadius = golosAvatar.Frame.Width / 2;

			if (steemAcc != null)
			{
				steemLabel.Text = steemAcc.Login;
				//LoadImage(steemAcc.Avatar, steemAvatar);
			}
			else
				steemViewHeight.Constant = 0;


			if (golosAcc != null)
			{
				golosLabel.Text = golosAcc.Login;
				//LoadImage(golosAcc.Avatar, golosAvatar);
			}
			else
				golosViewHeight.Constant = 0;

			HighlightView();
			SetAddButton();

			addAccountButton.TouchDown += (sender, e) =>
			{
				var myViewController = new PreLoginViewController();
				myViewController.newAccountNetwork = UserContext.Instanse.Network == Constants.Steem ? Constants.Golos : Constants.Steem;
				NavigationController.PushViewController(myViewController, true);
			};

			steemButton.TouchDown += (sender, e) =>
			{
				UserContext.Instanse.Accounts.Remove(steemAcc);
				steemViewHeight.Constant = 0;
				RemoveNetwork(Constants.Steem);
			};

			golosButton.TouchDown += (sender, e) =>
			{
				UserContext.Instanse.Accounts.Remove(golosAcc);
				golosViewHeight.Constant = 0;
				RemoveNetwork(Constants.Golos);
			};

			UITapGestureRecognizer steemTap = new UITapGestureRecognizer(() => SwitchNetwork(Constants.Steem));
			UITapGestureRecognizer golosTap = new UITapGestureRecognizer(() => SwitchNetwork(Constants.Golos));

			steemView.AddGestureRecognizer(steemTap);
			golosView.AddGestureRecognizer(golosTap);

			reportButton.TouchDown += (sender, e) =>
			{
				if (MFMailComposeViewController.CanSendMail)
				{
					mailController = new MFMailComposeViewController();
					mailController.SetToRecipients(new string[] { "steepshot.org@gmail.com" });
					mailController.SetSubject("User report");
					mailController.Finished += (object s, MFComposeResultEventArgs args) =>
					{
						args.Controller.DismissViewController(true, null);
					};
					this.PresentViewController(mailController, true, null);
				}
				else
					ShowAlert("Setup your mail please");
			};

			termsButton.TouchDown += (sender, e) =>
			{
				UIApplication.SharedApplication.OpenUrl(new Uri(Constants.Tos));
			};
			lowRatedSwitch.ValueChanged += (sender, e) =>
			{
				SwitchLowRated();
			};
			nsfwSwitch.ValueChanged += (sender, e) =>
			{
				SwitchNsfw();
			};
		}

		public override void ViewWillDisappear(bool animated)
		{
			NavigationController.SetNavigationBarHidden(true, true);
			base.ViewWillDisappear(animated);
		}

		private async Task SwitchNsfw()
		{
			try
			{
				nsfwSwitch.Enabled = false;
				var response = await Api.SetNsfw(new SetNsfwRequest(UserContext.Instanse.Token, !UserContext.Instanse.NSFW));
				if (response.Success)
				{
					UserContext.Instanse.NSFW = !UserContext.Instanse.NSFW;
					UserContext.Save();
				}
			}
			catch (Exception ex)
			{
				Reporter.SendCrash(ex);
			}
			finally
			{
				nsfwSwitch.On = UserContext.Instanse.NSFW;
				nsfwSwitch.Enabled = true;
			}
		}

		private async Task SwitchLowRated()
		{
			try
			{
				lowRatedSwitch.Enabled = false;
				var response = await Api.SetLowRated(new SetLowRatedRequest(UserContext.Instanse.Token, !UserContext.Instanse.LowRated));
				if (response.Success)
				{
					UserContext.Instanse.LowRated = !UserContext.Instanse.LowRated;
					UserContext.Save();
				}
			}
			catch (Exception ex)
			{
				Reporter.SendCrash(ex);
			}
			finally
			{
				lowRatedSwitch.On = UserContext.Instanse.LowRated;
				lowRatedSwitch.Enabled = true;
			}
		}

		private async Task CheckLowRated()
		{
			try
			{
				lowRatedSwitch.Enabled = false;
				var response = await Api.IsLowRated(new IsLowRatedRequest(UserContext.Instanse.Token));
				if (response.Success)
				{
					UserContext.Instanse.LowRated = response.Result.ShowLowRated;
					lowRatedSwitch.On = UserContext.Instanse.LowRated;
					UserContext.Save();
				}
			}
			catch (Exception ex)
			{
				Reporter.SendCrash(ex);
			}
			finally
			{
				lowRatedSwitch.Enabled = true;
			}
		}

		private async Task CheckNsfw()
		{
			try
			{
				nsfwSwitch.Enabled = false;
				var response = await Api.IsNsfw(new IsNsfwRequest(UserContext.Instanse.Token));
				if (response.Success)
				{
					UserContext.Instanse.NSFW = response.Result.ShowNsfw;
					nsfwSwitch.On = UserContext.Instanse.NSFW;
					UserContext.Save();
				}
			}
			catch (Exception ex)
			{
				Reporter.SendCrash(ex);
			}
			finally
			{
				nsfwSwitch.Enabled = true;
			}
		}

		private void SwitchNetwork(string network)
		{
			if (UserContext.Instanse.Network == network)
				return;

			UserContext.Instanse.Network = network;
			HighlightView();
			SwitchApiAddress();

			SetAddButton();
			UserContext.Save();

			UserContext.Instanse.IsHomeFeedLoaded = false;
			var myViewController = new MainTabBarController();
			NavigationController.ViewControllers = new UIViewController[] { myViewController, this };
			NavigationController.PopViewController(false);

			/*
			var alert = UIAlertController.Create(null, $"Do you want to change the network to the {network}?", UIAlertControllerStyle.Alert);

			alert.AddAction(UIAlertAction.Create("No", UIAlertActionStyle.Cancel, null));
			alert.AddAction(UIAlertAction.Create("Yes", UIAlertActionStyle.Default, action =>
			{
				if (UserContext.Instanse.Network != network)
				{
					try
					{

					}
					catch (Exception ex)
					{

					}
				}
			}));

			PresentViewController(alert, animated: true, completionHandler: null); */
		}

		private void RemoveNetwork(string network)
		{
			if (UserContext.Instanse.Accounts.Count == 0)
			{
				var myViewController = new FeedViewController();
				NavigationController.ViewControllers = new UIViewController[2] { myViewController, this };
				NavigationController.PopViewController(false);
			}
			else
			{
				if (UserContext.Instanse.Network != network)
				{
					HighlightView();
					SetAddButton();
				}
				else
				{
					SwitchNetwork(UserContext.Instanse.Network == Constants.Steem ? Constants.Golos : Constants.Steem);
				}
			}
			UserContext.Save();
		}

		private void HighlightView()
		{
			if (UserContext.Instanse.Network == Constants.Golos)
			{
				golosView.BackgroundColor = UIColor.Cyan;//Constants.Blue;
				steemView.BackgroundColor = UIColor.White;
			}
			else
			{
				steemView.BackgroundColor = UIColor.Cyan;//Constants.Blue;
				golosView.BackgroundColor = UIColor.White;
			}
		}

		private void SetAddButton()
		{
			addAccountButton.Hidden = UserContext.Instanse.Accounts.Count == 2;
			//#if !DEBUG
			//addAccountButton.Hidden = true;
			//#endif
		}

		public override void ViewDidDisappear(bool animated)
		{
			base.ViewDidDisappear(animated);
			UserContext.Instanse.NetworkChanged = previousNetwork != UserContext.Instanse.Network;
			UserContext.Instanse.ShouldProfileUpdate = previousNetwork != UserContext.Instanse.Network;
		}
	}
}

