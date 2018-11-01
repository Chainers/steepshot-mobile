using System;
using Foundation;
using PureLayout.Net;
using Steepshot.Core.Localization;
using Steepshot.iOS.CustomViews;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.Views;
using UIKit;
using Steepshot.Core.Extensions;
using Steepshot.Core.Facades;

namespace Steepshot.iOS.Popups
{
    public class PowerManipulationUiView : UIView
    {
        public UIButton PowerUpButton;
        public UIButton PowerDownButton;
        public UIButton CancelPowerDownButton;
        public UIButton CancelButton;

        public PowerManipulationUiView(WalletFacade walletFacade)
        {
            var commonMargin = 20;

            base.ClipsToBounds = true;
            base.Layer.CornerRadius = 20;
            base.BackgroundColor = Constants.R250G250B250;

            var dialogWidth = UIScreen.MainScreen.Bounds.Width - 10 * 2;
            this.AutoSetDimension(ALDimension.Width, dialogWidth);

            var title = new UILabel
            {
                TextAlignment = UITextAlignment.Center,
                Font = Constants.Semibold14,
                Text = "SELECT ACTION"
            };
            base.AddSubview(title);
            title.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 24);
            title.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            title.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);

            var separator = new UIView
            {
                BackgroundColor = Constants.R245G245B245
            };
            base.AddSubview(separator);
            separator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, title, 26);
            separator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            separator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            separator.AutoSetDimension(ALDimension.Height, 1);

            PowerUpButton = new UIButton
            {
                BackgroundColor = Constants.R255G255B255,
                Font = Constants.Semibold14
            };
            PowerUpButton.Layer.CornerRadius = 25;
            PowerUpButton.Layer.BorderWidth = 1;
            PowerUpButton.Layer.BorderColor = Constants.R245G245B245.CGColor;
            PowerUpButton.SetTitle(AppDelegate.Localization.GetText(LocalizationKeys.PowerUp), UIControlState.Normal);
            PowerUpButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            base.AddSubview(PowerUpButton);
            PowerUpButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, separator, 20);
            PowerUpButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            PowerUpButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            PowerUpButton.AutoSetDimension(ALDimension.Height, 50);

            PowerDownButton = new UIButton
            {
                BackgroundColor = Constants.R255G255B255,
                Font = Constants.Semibold14
            };
            PowerDownButton.Layer.CornerRadius = 25;
            PowerDownButton.Layer.BorderWidth = 1;
            PowerDownButton.Layer.BorderColor = Constants.R245G245B245.CGColor;
            PowerDownButton.SetTitle(AppDelegate.Localization.GetText(LocalizationKeys.PowerDown), UIControlState.Normal);
            PowerDownButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            base.AddSubview(PowerDownButton);
            PowerDownButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, PowerUpButton, 10);
            PowerDownButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            PowerDownButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            PowerDownButton.AutoSetDimension(ALDimension.Height, 50);

            var showCancelPowerDown = walletFacade.SelectedBalance.ToWithdraw > 0;

            CancelPowerDownButton = new UIButton();

            if (showCancelPowerDown)
            {
                CancelPowerDownButton.SetTitle(AppDelegate.Localization.GetText(LocalizationKeys.CancelPowerDown), UIControlState.Normal);
                CancelPowerDownButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
                CancelPowerDownButton.BackgroundColor = Constants.R255G255B255;
                CancelPowerDownButton.Layer.CornerRadius = 25;
                CancelPowerDownButton.Font = Constants.Semibold14;
                CancelPowerDownButton.Layer.BorderWidth = 1;
                CancelPowerDownButton.Layer.BorderColor = Constants.R245G245B245.CGColor;
                base.AddSubview(CancelPowerDownButton);

                CancelPowerDownButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, PowerDownButton, 10);
                CancelPowerDownButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
                CancelPowerDownButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
                CancelPowerDownButton.AutoSetDimension(ALDimension.Height, 50);
            }

            var bottomSeparator = new UIView();
            bottomSeparator.BackgroundColor = Constants.R245G245B245;
            base.AddSubview(bottomSeparator);

            if (showCancelPowerDown)
                bottomSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, CancelPowerDownButton, 26);
            else
                bottomSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, PowerDownButton, 26);
            bottomSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            bottomSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            bottomSeparator.AutoSetDimension(ALDimension.Height, 1);

            CancelButton = new UIButton();
            CancelButton.SetTitle(AppDelegate.Localization.GetText(LocalizationKeys.Cancel), UIControlState.Normal);
            CancelButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            CancelButton.Layer.CornerRadius = 25;
            CancelButton.Font = Constants.Semibold14;
            CancelButton.BackgroundColor = Constants.R255G255B255;
            CancelButton.Layer.BorderWidth = 1;
            CancelButton.Layer.BorderColor = Constants.R245G245B245.CGColor;
            base.AddSubview(CancelButton);

            CancelButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, bottomSeparator, 20);
            CancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            CancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            CancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, commonMargin);
            CancelButton.AutoSetDimension(ALDimension.Height, 50);
        }
    }

    public class PowerManipulationPopup : CustomAlertView
    {
        private readonly WalletFacade _walletFacade;
        private readonly UINavigationController _controller;
        private readonly Action<bool> _continuePowerDownCancellation;

        public PowerManipulationPopup(UINavigationController controller, WalletFacade walletFacade, Action<bool> continuePowerDownCancellation)
            : this(controller, new PowerManipulationUiView(walletFacade), walletFacade, continuePowerDownCancellation)
        {

        }

        public PowerManipulationPopup(UINavigationController controller, PowerManipulationUiView view, WalletFacade walletFacade, Action<bool> continuePowerDownCancellation)
        : base(controller, view)
        {
            _walletFacade = walletFacade;
            _controller = controller;
            _continuePowerDownCancellation = continuePowerDownCancellation;

            view.PowerUpButton.TouchUpInside += OnPowerUp;
            view.PowerDownButton.TouchUpInside += OnPowerDown;
            view.CancelPowerDownButton.TouchUpInside += OnCancelPowerDown;
            view.CancelButton.TouchUpInside += OnCancel;
        }


        private void OnCancel(object s, EventArgs ev)
        {
            Close();
        }

        private void OnPowerDown(object s, EventArgs ev)
        {
            Close();
            _controller.PushViewController(new PowerManipulationViewController(_walletFacade, Core.Models.Enums.PowerAction.PowerDown), true);
        }

        private void OnPowerUp(object s, EventArgs ev)
        {
            Close();
            _controller.PushViewController(new PowerManipulationViewController(_walletFacade, Core.Models.Enums.PowerAction.PowerUp), true);
        }

        private void OnCancelPowerDown(object s, EventArgs ev)
        {
            Close();
            var at = new NSMutableAttributedString();
            at.Append(new NSAttributedString(string.Format(AppDelegate.Localization.GetText(LocalizationKeys.CancelPowerDownAlert), _walletFacade.SelectedBalance.ToWithdraw.ToBalanceValueString())));

            TransferDialogPopup.Create(_controller, at, _continuePowerDownCancellation);
        }
    }
}
