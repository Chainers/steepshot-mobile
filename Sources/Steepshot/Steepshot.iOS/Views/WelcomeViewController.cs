using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using PureLayout.Net;
using Steepshot.Core;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.ViewControllers;
using UIKit;
using Xamarin.TTTAttributedLabel;

namespace Steepshot.iOS.Views
{
    public partial class WelcomeViewController : BaseViewController
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var gradient = new CAGradientLayer();
            gradient.Frame = steemLogin.Bounds;
            gradient.StartPoint = Helpers.Constants.StartGradientPoint;
            gradient.EndPoint = Helpers.Constants.EndGradientPoint;
            gradient.Colors = Helpers.Constants.OrangeGradient;

            NavigationController.SetNavigationBarHidden(false, false);
            gradient.CornerRadius = steemLogin.Layer.CornerRadius = newAccount.Layer.CornerRadius = 25;
            steemLogin.TitleLabel.Font = newAccount.TitleLabel.Font = Helpers.Constants.Semibold14;
            devSwitch.On = AppSettings.IsDev;

            var devTap = new UITapGestureRecognizer(ToggleDevSwitchVisibility);
            devTap.NumberOfTapsRequired = 5;
            logo.AddGestureRecognizer(devTap);

            newAccount.Layer.MasksToBounds = steemLogin.Layer.MasksToBounds = false;
            newAccount.Layer.ShadowOffset = steemLogin.Layer.ShadowOffset = new CGSize(0f, 10.0f);
            newAccount.Layer.ShadowRadius = steemLogin.Layer.ShadowRadius = 12f;
            steemLogin.Layer.ShadowOpacity = 0.5f;
            steemLogin.Layer.ShadowColor = Helpers.Constants.R231G72B0.CGColor;
            steemLogin.Layer.InsertSublayer(gradient, 0);

            newAccount.Layer.ShadowOpacity = 0.7f;
            newAccount.Layer.ShadowColor = Helpers.Constants.R204G204B204.CGColor;

            steemLogin.TouchDown += GoToPreLogin;
            newAccount.TouchDown += CreateAccount;
            devSwitch.ValueChanged += SwitchEnvironment;

            SetBackButtton();
            SetAgreementDecoration();
        }

        private void SetBackButtton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.SetLeftBarButtonItem(leftBarButton, true);
            NavigationController.NavigationBar.TintColor = Helpers.Constants.R15G24B30;
        }

        private void CreateAccount(object sender, EventArgs e)
        {
            UIApplication.SharedApplication.OpenUrl(new Uri(Core.Constants.SteemitRegUrl));
        }

        private void GoToPreLogin(object sender, EventArgs e)
        {
            if (!termsSwitcher.On)
            {
                ShowAlert(Localization.Messages.AcceptToS);
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
                Font = Helpers.Constants.Regular12,
                ForegroundColor = Helpers.Constants.R15G24B30,
            };

            var ppAttribute = new UIStringAttributes
            {
                Link = new NSUrl(Tos),
                Font = Helpers.Constants.Regular12,
                ForegroundColor = Helpers.Constants.R15G24B30,
            };

            var noLinkAttribute = new UIStringAttributes
            {
                Font = Helpers.Constants.Regular12,
                ForegroundColor = Helpers.Constants.R151G155B158,
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
