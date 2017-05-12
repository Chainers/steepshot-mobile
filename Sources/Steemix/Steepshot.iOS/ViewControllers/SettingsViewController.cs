using System;
using System.Linq;
using System.Net;
using Foundation;
using UIKit;

namespace Steepshot.iOS
{
	public partial class SettingsViewController : BaseViewController
	{
		protected SettingsViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logi   
		}

		private Account steemAcc;
		private Account golosAcc;

		private string previousNetwork;

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
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
				var myViewController = Storyboard.InstantiateViewController(nameof(PreLoginViewController)) as PreLoginViewController;
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
		}

		private void SwitchNetwork(string network)
		{
			UserContext.Instanse.Network = network;
			//HighlightView();
			SwitchApiAddress();

			//SetAddButton();
			UserContext.Save();

			UserContext.Instanse.IsHomeFeedLoaded = false;
			var myViewController = Storyboard.InstantiateViewController("MainTabBar") as UITabBarController;
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
	            var myViewController = Storyboard.InstantiateViewController(nameof(FeedViewController)) as FeedViewController;
				NavigationController.ViewControllers = new UIViewController[2] { myViewController, this };
	            NavigationController.PopViewController(false);
			}
			else
			{
				UserContext.Instanse.Network = network == Constants.Steem? Constants.Golos : Constants.Steem;
				HighlightView();
				SwitchApiAddress();
				SetAddButton();
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
		}

		public void LoadImage(string uri, UIImageView avatar)
		{
			avatar.Image = UIImage.FromBundle("ic_user_placeholder");
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

		public override void ViewDidDisappear(bool animated)
		{
			base.ViewDidDisappear(animated);
			UserContext.Instanse.NetworkChanged = previousNetwork != UserContext.Instanse.Network;
			UserContext.Instanse.ShouldProfileUpdate = previousNetwork != UserContext.Instanse.Network;
		}
	}
}

