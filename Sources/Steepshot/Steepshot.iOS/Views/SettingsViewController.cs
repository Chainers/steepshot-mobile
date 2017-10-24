using System;
using System.Linq;
using Autofac;
using MessageUI;
using Steepshot.Core;
using Steepshot.Core.Authority;
using Steepshot.Core.Presenters;
using Steepshot.Core.Services;
using Steepshot.Core.Utils;
using Steepshot.iOS.ViewControllers;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class SettingsViewController : BaseViewController
    {
        private UserInfo _steemAcc;
        private UserInfo _golosAcc;
        private bool _isTabBarNeedResfresh;
        private KnownChains _previousNetwork;
        private MFMailComposeViewController _mailController;

        public override void ViewDidLoad()
        {
            NavigationController.NavigationBar.Translucent = false;
            base.ViewDidLoad();
            nsfwSwitch.On = BasePresenter.User.IsNsfw;
            lowRatedSwitch.On = BasePresenter.User.IsLowRated;
            NavigationController.SetNavigationBarHidden(false, false);
            _steemAcc = BasePresenter.User.GetAllAccounts().FirstOrDefault(a => a.Chain == KnownChains.Steem);
            _golosAcc = BasePresenter.User.GetAllAccounts().FirstOrDefault(a => a.Chain == KnownChains.Golos);
            _previousNetwork = BasePresenter.Chain;
            var appInfoService = AppSettings.Container.Resolve<IAppInfo>();
            versionLabel.Text = Localization.Messages.AppVersion(appInfoService.GetAppVersion(), appInfoService.GetBuildVersion());
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

            HighlightView(BasePresenter.Chain);
            SetAddButton();

            addAccountButton.TouchDown += (sender, e) =>
            {
                var myViewController = new PreLoginViewController();
                myViewController.NewAccountNetwork = BasePresenter.Chain == KnownChains.Steem ? KnownChains.Golos : KnownChains.Steem;
                NavigationController.PushViewController(myViewController, true);
            };

            steemButton.TouchDown += (sender, e) =>
            {
                BasePresenter.User.Delete(_steemAcc);
                steemViewHeight.Constant = 0;
                RemoveNetwork(KnownChains.Steem);
            };

            golosButton.TouchDown += (sender, e) =>
            {
                BasePresenter.User.Delete(_golosAcc);
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
                    _mailController.SetToRecipients(new[] { "steepshot.org@gmail.com" });
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
                BasePresenter.User.IsLowRated = lowRatedSwitch.On;
            };
            nsfwSwitch.ValueChanged += (sender, e) =>
            {
                BasePresenter.User.IsNsfw = nsfwSwitch.On;
            };
        }

        public override void ViewWillDisappear(bool animated)
        {
            ShouldProfileUpdate = _previousNetwork != BasePresenter.Chain;

            if (IsMovingFromParentViewController && !_isTabBarNeedResfresh)
                NavigationController.SetNavigationBarHidden(true, true);
            base.ViewWillDisappear(animated);
        }

        private void SwitchNetwork(UserInfo user)
        {
            if (BasePresenter.Chain == user.Chain)
                return;
            BasePresenter.User.SwitchUser(user);
            HighlightView(user.Chain);
            BasePresenter.SwitchChain(user.Chain);

            SetAddButton();

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
            if (BasePresenter.User.GetAllAccounts().Count == 0)
            {
                var myViewController = new FeedViewController();
                NavigationController.ViewControllers = new UIViewController[] { myViewController, this };
                _isTabBarNeedResfresh = true;
                NavigationController.PopViewController(false);
            }
            else
            {
                if (BasePresenter.Chain != network)
                {
                    HighlightView(BasePresenter.Chain);
                    SetAddButton();
                }
                else
                {
                    BasePresenter.SwitchChain(BasePresenter.Chain == KnownChains.Steem ? _golosAcc : _steemAcc);
                }
            }
            BasePresenter.User.Save();
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
            addAccountButton.Hidden = BasePresenter.User.GetAllAccounts().Count == 2;
            //#if !DEBUG
            //addAccountButton.Hidden = true;
            //#endif
        }
    }
}
