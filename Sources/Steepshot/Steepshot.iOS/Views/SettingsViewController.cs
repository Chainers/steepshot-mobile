﻿using System;
using System.Linq;
using Autofac;
using Foundation;
using MessageUI;
using Steepshot.Core;
using Steepshot.Core.Authority;
using Steepshot.Core.Services;
using Steepshot.Core.Utils;
using Steepshot.iOS.ViewControllers;
using UIKit;

namespace Steepshot.iOS.Views
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

        private UserInfo _steemAcc;
        private UserInfo _golosAcc;
        private bool _isTabBarNeedResfresh;
        private KnownChains _previousNetwork;
        private MFMailComposeViewController _mailController;

        public override void ViewDidLoad()
        {
            NavigationController.NavigationBar.Translucent = false;
            base.ViewDidLoad();
            nsfwSwitch.On = User.IsNsfw;

            nsfwSwitch.On = User.IsNsfw;
            lowRatedSwitch.On = User.IsLowRated;
            NavigationController.SetNavigationBarHidden(false, false);
            _steemAcc = User.GetAllAccounts().FirstOrDefault(a => a.Chain == KnownChains.Steem);
            _golosAcc = User.GetAllAccounts().FirstOrDefault(a => a.Chain == KnownChains.Golos);
            _previousNetwork = Chain;
            versionLabel.Text = $"App version: {AppSettings.Container.Resolve<IAppInfo>().GetAppVersion()} Build number: {NSBundle.MainBundle.InfoDictionary["CFBundleVersion"]}";
            //steemAvatar.Layer.CornerRadius = steemAvatar.Frame.Width / 2;
            //golosAvatar.Layer.CornerRadius = golosAvatar.Frame.Width / 2;

            if (_steemAcc != null)
            {
                steemLabel.Text = _steemAcc.Login;
                //LoadImage(steemAcc.Avatar, steemAvatar);
            }
            else
                steemViewHeight.Constant = 0;


            if (_golosAcc != null)
            {
                golosLabel.Text = _golosAcc.Login;
                //LoadImage(golosAcc.Avatar, golosAvatar);
            }
            else
                golosViewHeight.Constant = 0;

            HighlightView(Chain);
            SetAddButton();

            addAccountButton.TouchDown += (sender, e) =>
            {
                var myViewController = new PreLoginViewController();
                myViewController.NewAccountNetwork = Chain == KnownChains.Steem ? KnownChains.Golos : KnownChains.Steem;
                NavigationController.PushViewController(myViewController, true);
            };

            steemButton.TouchDown += (sender, e) =>
            {
                User.Delete(_steemAcc);
                steemViewHeight.Constant = 0;
                RemoveNetwork(KnownChains.Steem);
            };

            golosButton.TouchDown += (sender, e) =>
            {
                User.Delete(_golosAcc);
                golosViewHeight.Constant = 0;
                RemoveNetwork(KnownChains.Golos);
            };

            UITapGestureRecognizer steemTap = new UITapGestureRecognizer(() => SwitchNetwork(_steemAcc));
            UITapGestureRecognizer golosTap = new UITapGestureRecognizer(() => SwitchNetwork(_golosAcc));

            steemView.AddGestureRecognizer(steemTap);
            golosView.AddGestureRecognizer(golosTap);

            reportButton.TouchDown += (sender, e) =>
            {
                if (MFMailComposeViewController.CanSendMail)
                {
                    _mailController = new MFMailComposeViewController();
                    _mailController.SetToRecipients(new string[] { "steepshot.org@gmail.com" });
                    _mailController.SetSubject("User report");
                    _mailController.Finished += (object s, MFComposeResultEventArgs args) =>
                    {
                        args.Controller.DismissViewController(true, null);
                    };
                    PresentViewController(_mailController, true, null);
                }
                else
                    ShowAlert("Setup your mail please");
            };

            termsButton.TouchDown += (sender, e) =>
            {
                UIApplication.SharedApplication.OpenUrl(new Uri(Tos));
            };
            lowRatedSwitch.ValueChanged += (sender, e) =>
            {
                User.IsLowRated = lowRatedSwitch.On;
            };
            nsfwSwitch.ValueChanged += (sender, e) =>
            {
                User.IsNsfw = nsfwSwitch.On;
            };
        }

        public override void ViewWillDisappear(bool animated)
        {
			NetworkChanged = _previousNetwork != Chain;
			ShouldProfileUpdate = _previousNetwork != Chain;

            if (IsMovingFromParentViewController && !_isTabBarNeedResfresh)
                NavigationController.SetNavigationBarHidden(true, true);
            base.ViewWillDisappear(animated);
        }

        private void SwitchNetwork(UserInfo user)
        {
            if (Chain == user.Chain)
                return;
            User.SwitchUser(user);
            HighlightView(user.Chain);
            SwitchChain(user.Chain);

            SetAddButton();

            IsHomeFeedLoaded = false;
            var myViewController = new MainTabBarController();
            NavigationController.ViewControllers = new UIViewController[] { myViewController, this };
            _isTabBarNeedResfresh = true;
            NavigationController.PopViewController(false);

            /*
			var alert = UIAlertController.Create(null, $"Do you want to change the network to the {network}?", UIAlertControllerStyle.Alert);

			alert.AddAction(UIAlertAction.Create("No", UIAlertActionStyle.Cancel, null));
			alert.AddAction(UIAlertAction.Create("Yes", UIAlertActionStyle.Default, action =>
			{
				if (Chain != network)
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

        private void RemoveNetwork(KnownChains network)
        {
            if (User.GetAllAccounts().Count == 0)
            {
                var myViewController = new FeedViewController();
                NavigationController.ViewControllers = new UIViewController[] { myViewController, this };
                _isTabBarNeedResfresh = true;
                NavigationController.PopViewController(false);
            }
            else
            {
                if (Chain != network)
                {
                    HighlightView(Chain);
                    SetAddButton();
                }
                else
                {
                    SwitchNetwork(Chain == KnownChains.Steem ? _golosAcc : _steemAcc);
                }
            }
            User.Save();
        }

        private void HighlightView(KnownChains network)
        {
            if (network == KnownChains.Golos)
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
            addAccountButton.Hidden = User.GetAllAccounts().Count == 2;
            //#if !DEBUG
            //addAccountButton.Hidden = true;
            //#endif
        }
    }
}