using System;
using Foundation;
using PureLayout.Net;
using Steepshot.Core.Utils;
using Steepshot.iOS.ViewControllers;
using UIKit;
using Xamarin.TTTAttributedLabel;
using Constants = Steepshot.iOS.Helpers.Constants;
using Steepshot.Core.Localization;
using SafariServices;

namespace Steepshot.iOS.Views
{
    public partial class WelcomeViewController : BaseViewController, ISFSafariViewControllerDelegate
    {
        private readonly bool _showRegistration;
        private readonly TTTAttributedLabel _attributedLabel = new TTTAttributedLabel();
        private readonly UIBarButtonItem _leftBarButton = new UIBarButtonItem();
        private readonly UITapGestureRecognizer _devTap;

        public WelcomeViewController(bool showRegistration)
        {
            _showRegistration = showRegistration;
            _devTap = new UITapGestureRecognizer(ToggleDevSwitchVisibility)
            {
                NumberOfTapsRequired = 5
            };
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationController.SetNavigationBarHidden(false, false);
            steemLogin.Layer.CornerRadius = newAccount.Layer.CornerRadius = 25;
            steemLogin.TitleLabel.Font = newAccount.TitleLabel.Font = Constants.Semibold14;
            devSwitch.On = AppSettings.Settings.IsDev;
            Constants.CreateShadow(steemLogin, Constants.R231G72B0, 0.5f, 25, 10, 12);
            Constants.CreateShadow(newAccount, Constants.R204G204B204, 0.7f, 25, 10, 12);

            newAccount.Hidden = !_showRegistration;

            SetBackButton();
            SetAgreementDecoration();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            Constants.CreateGradient(steemLogin, 25);
        }

        public override void ViewWillAppear(bool animated)
        {
            if (IsMovingToParentViewController)
            {
                steemLogin.TouchDown += GoToPreLogin;
                newAccount.TouchDown += CreateAccount;
                devSwitch.ValueChanged += SwitchEnvironment;
                _leftBarButton.Clicked += GoBack;
                logo.AddGestureRecognizer(_devTap);
            }
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            if (IsMovingFromParentViewController)
            {
                steemLogin.TouchDown -= GoToPreLogin;
                newAccount.TouchDown -= CreateAccount;
                devSwitch.ValueChanged -= SwitchEnvironment;
                _leftBarButton.Clicked -= GoBack;
                logo.RemoveGestureRecognizer(_devTap);
            }
            base.ViewWillDisappear(animated);
        }

        private void SetBackButton()
        {
            _leftBarButton.Image = UIImage.FromBundle("ic_back_arrow");
            NavigationItem.LeftBarButtonItem = _leftBarButton;
            NavigationController.NavigationBar.TintColor = Constants.R15G24B30;
        }

        private void CreateAccount(object sender, EventArgs e)
        {
            var myViewController = new RegistrationViewController();
            NavigationController.PushViewController(myViewController, true);

            /*
            var sv = new SFSafariViewController(new Uri(Core.Constants.SteemitRegUrl));
            sv.Delegate = this;

            NavigationController.SetNavigationBarHidden(true, false);
            NavigationController.PushViewController(sv, false);*/
        }

        [Export("safariViewControllerDidFinish:")]
        public void DidFinish(SFSafariViewController controller)
        {
            NavigationController.SetNavigationBarHidden(false, false);
            NavigationController.PopViewController(false);
        }

        private void GoToPreLogin(object sender, EventArgs e)
        {
            if (!termsSwitcher.On)
            {
                ShowAlert(LocalizationKeys.AcceptToS);
                return;
            }
            var myViewController = new PreLoginViewController();
            NavigationController.PushViewController(myViewController, true);
        }

        private void SwitchEnvironment(object sender, EventArgs e)
        {
            var isDev = ((UISwitch)sender).On;
            AppSettings.SetDev(isDev);
        }

        private void ToggleDevSwitchVisibility()
        {
            devSwitch.Hidden = !devSwitch.Hidden;
        }

        private void SetAgreementDecoration()
        {
            var tsAttribute = new UIStringAttributes
            {
                Link = new NSUrl(Pp),
                Font = Constants.Regular12,
                ForegroundColor = Constants.R15G24B30,
            };

            var ppAttribute = new UIStringAttributes
            {
                Link = new NSUrl(Tos),
                Font = Constants.Regular12,
                ForegroundColor = Constants.R15G24B30,
            };

            var noLinkAttribute = new UIStringAttributes
            {
                Font = Constants.Regular12,
                ForegroundColor = Constants.R151G155B158,
            };

            _attributedLabel.EnabledTextCheckingTypes = NSTextCheckingType.Link;
            _attributedLabel.Lines = 2;

            var prop = new NSDictionary();
            _attributedLabel.LinkAttributes = prop;
            _attributedLabel.ActiveLinkAttributes = prop;

            _attributedLabel.Delegate = new TTTAttributedLabelCustomDelegate();
            agreementView.AddSubview(_attributedLabel);

            var at = new NSMutableAttributedString();
            at.Append(new NSAttributedString("I agree with ", noLinkAttribute));
            at.Append(new NSAttributedString("Terms of Service", tsAttribute));
            at.Append(new NSAttributedString(" & ", noLinkAttribute));
            at.Append(new NSAttributedString("Privacy Policy", ppAttribute));

            _attributedLabel.SetText(at);
            _attributedLabel.AutoAlignAxis(ALAxis.Horizontal, termsSwitcher);
            _attributedLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15f);
            termsSwitcher.AutoPinEdge(ALEdge.Left, ALEdge.Right, _attributedLabel, 5f);
            termsSwitcher.Layer.CornerRadius = 16;
        }
    }

    public class TTTAttributedLabelCustomDelegate : TTTAttributedLabelDelegate
    {
        public override void DidSelectLinkWithURL(TTTAttributedLabel label, NSUrl url)
        {
            UIApplication.SharedApplication.OpenUrl(url, new NSDictionary(), null);
        }
    }
}
