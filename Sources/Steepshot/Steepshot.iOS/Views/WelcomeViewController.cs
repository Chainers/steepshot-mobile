using System;
using Foundation;
using PureLayout.Net;
using Steepshot.Core.Presenters;
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
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationController.SetNavigationBarHidden(false, false);
            steemLogin.Layer.CornerRadius = newAccount.Layer.CornerRadius = 25;
            steemLogin.TitleLabel.Font = newAccount.TitleLabel.Font = Constants.Semibold14;
            devSwitch.On = AppSettings.IsDev;
            Constants.CreateShadow(steemLogin, Constants.R231G72B0, 0.5f, 25, 10, 12);
            Constants.CreateShadow(newAccount, Constants.R204G204B204, 0.7f, 25, 10, 12);

            var devTap = new UITapGestureRecognizer(ToggleDevSwitchVisibility);
            devTap.NumberOfTapsRequired = 5;
            logo.AddGestureRecognizer(devTap);

            steemLogin.TouchDown += GoToPreLogin;
            newAccount.TouchDown += CreateAccount;
            devSwitch.ValueChanged += SwitchEnvironment;

            SetBackButton();
            SetAgreementDecoration();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            Constants.CreateGradient(steemLogin, 25);
        }

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.LeftBarButtonItem = leftBarButton;
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

        private void GoBack(object sender, EventArgs e)
        {
            NavigationController.PopViewController(true);
        }

        private void SwitchEnvironment(object sender, EventArgs e)
        {
            BasePresenter.SwitchChain(((UISwitch)sender).On);
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

            var attributedLabel = new TTTAttributedLabel();
            attributedLabel.EnabledTextCheckingTypes = NSTextCheckingType.Link;
            attributedLabel.Lines = 2;

            var prop = new NSDictionary();
            attributedLabel.LinkAttributes = prop;
            attributedLabel.ActiveLinkAttributes = prop;

            attributedLabel.Delegate = new TTTAttributedLabelCustomDelegate();
            agreementView.AddSubview(attributedLabel);

            var at = new NSMutableAttributedString();
            at.Append(new NSAttributedString("I agree with ", noLinkAttribute));
            at.Append(new NSAttributedString("Terms of Service", tsAttribute));
            at.Append(new NSAttributedString(" & ", noLinkAttribute));
            at.Append(new NSAttributedString("Privacy Policy", ppAttribute));

            attributedLabel.SetText(at);
            attributedLabel.AutoAlignAxis(axis: ALAxis.Horizontal, otherView: termsSwitcher);
            attributedLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15f);
            termsSwitcher.AutoPinEdge(ALEdge.Left, ALEdge.Right, attributedLabel, 5f);
            termsSwitcher.Layer.CornerRadius = 16;
        }
    }

    public class TTTAttributedLabelCustomDelegate : TTTAttributedLabelDelegate
    {
        public override void DidSelectLinkWithURL(TTTAttributedLabel label, NSUrl url)
        {
            UIApplication.SharedApplication.OpenUrl(url);
        }
    }
}
