using System;
using Foundation;
using PureLayout.Net;
using Steepshot.Core.Localization;
using Steepshot.Core.Presenters;
using Steepshot.iOS.CustomViews;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.Views;
using UIKit;
using Steepshot.Core.Extensions;

namespace Steepshot.iOS.Popups
{
    public class PowerManipulationPopup
    {
        private readonly UINavigationController _controller;
        private readonly WalletPresenter _presenter;
        private readonly Action<bool> _continuePowerDownCancellation;
        private CustomAlertView _alert;
        private UIButton _powerUpButton;
        private UIButton _powerDownButton;
        private UIButton _cancelPowerDownButton;

        public PowerManipulationPopup(UINavigationController controller, WalletPresenter presenter, Action<bool> continuePowerDownCancellation)
        {
            _presenter = presenter;
            _controller = controller;
            _continuePowerDownCancellation = continuePowerDownCancellation;
        }

        public void Create()
        {
            var commonMargin = 20;

            var popup = new UIView();
            popup.ClipsToBounds = true;
            popup.Layer.CornerRadius = 20;
            popup.BackgroundColor = Constants.R250G250B250;

            _alert = new CustomAlertView(popup, _controller);

            var dialogWidth = UIScreen.MainScreen.Bounds.Width - 10 * 2;
            popup.AutoSetDimension(ALDimension.Width, dialogWidth);

            var title = new UILabel();
            title.TextAlignment = UITextAlignment.Center;
            title.Font = Constants.Semibold14;
            title.Text = AppDelegate.Localization.GetText(LocalizationKeys.SelectAction).ToUpper();
            popup.AddSubview(title);
            title.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 24);
            title.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            title.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);

            var separator = new UIView();
            separator.BackgroundColor = Constants.R245G245B245;
            popup.AddSubview(separator);

            separator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, title, 26);
            separator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            separator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            separator.AutoSetDimension(ALDimension.Height, 1);

            _powerUpButton = new UIButton();
            _powerUpButton.SetTitle(AppDelegate.Localization.GetText(LocalizationKeys.PowerUp), UIControlState.Normal);
            _powerUpButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            _powerUpButton.BackgroundColor = Constants.R255G255B255;
            _powerUpButton.Layer.CornerRadius = 25;
            _powerUpButton.Font = Constants.Semibold14;
            _powerUpButton.Layer.BorderWidth = 1;
            _powerUpButton.Layer.BorderColor = Constants.R245G245B245.CGColor;
            popup.AddSubview(_powerUpButton);

            _powerUpButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, separator, 20);
            _powerUpButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            _powerUpButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            _powerUpButton.AutoSetDimension(ALDimension.Height, 50);

            _powerDownButton = new UIButton();
            _powerDownButton.SetTitle(AppDelegate.Localization.GetText(LocalizationKeys.PowerDown), UIControlState.Normal);
            _powerDownButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            _powerDownButton.BackgroundColor = Constants.R255G255B255;
            _powerDownButton.Layer.CornerRadius = 25;
            _powerDownButton.Font = Constants.Semibold14;
            _powerDownButton.Layer.BorderWidth = 1;
            _powerDownButton.Layer.BorderColor = Constants.R245G245B245.CGColor;
            popup.AddSubview(_powerDownButton);

            _powerDownButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _powerUpButton, 10);
            _powerDownButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            _powerDownButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            _powerDownButton.AutoSetDimension(ALDimension.Height, 50);

            var showCancelPowerDown = _presenter.Balances[0].ToWithdraw > 0;

            _cancelPowerDownButton = new UIButton();

            if (showCancelPowerDown)
            {
                _cancelPowerDownButton.SetTitle(AppDelegate.Localization.GetText(LocalizationKeys.CancelPowerDown), UIControlState.Normal);
                _cancelPowerDownButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
                _cancelPowerDownButton.BackgroundColor = Constants.R255G255B255;
                _cancelPowerDownButton.Layer.CornerRadius = 25;
                _cancelPowerDownButton.Font = Constants.Semibold14;
                _cancelPowerDownButton.Layer.BorderWidth = 1;
                _cancelPowerDownButton.Layer.BorderColor = Constants.R245G245B245.CGColor;
                popup.AddSubview(_cancelPowerDownButton);

                _cancelPowerDownButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _powerDownButton, 10);
                _cancelPowerDownButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
                _cancelPowerDownButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
                _cancelPowerDownButton.AutoSetDimension(ALDimension.Height, 50);
            }

            var bottomSeparator = new UIView();
            bottomSeparator.BackgroundColor = Constants.R245G245B245;
            popup.AddSubview(bottomSeparator);

            if (showCancelPowerDown)
                bottomSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _cancelPowerDownButton, 26);
            else
                bottomSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _powerDownButton, 26);
            bottomSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            bottomSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            bottomSeparator.AutoSetDimension(ALDimension.Height, 1);

            var cancelButton = new UIButton();
            cancelButton.SetTitle(AppDelegate.Localization.GetText(LocalizationKeys.Cancel), UIControlState.Normal);
            cancelButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            cancelButton.Layer.CornerRadius = 25;
            cancelButton.Font = Constants.Semibold14;
            cancelButton.BackgroundColor = Constants.R255G255B255;
            cancelButton.Layer.BorderWidth = 1;
            cancelButton.Layer.BorderColor = Constants.R245G245B245.CGColor;
            popup.AddSubview(cancelButton);

            cancelButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, bottomSeparator, 20);
            cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, commonMargin);
            cancelButton.AutoSetDimension(ALDimension.Height, 50);

            _powerUpButton.TouchDown += OnPowerUpPressed;
            _powerDownButton.TouchDown += OnPowerDownPressed;
            _cancelPowerDownButton.TouchDown += OnCancelPowerDownPressed;
            cancelButton.TouchDown += (s, ev) => { _alert.Close(); };

            _alert.Show();
        }

        private void OnPowerUpPressed(object sender, EventArgs args)
        {
            _alert.Close();
            _controller.PushViewController(new PowerManipulationViewController(_presenter, Core.Models.Enums.PowerAction.PowerUp), true);
        }

        private void OnPowerDownPressed(object sender, EventArgs args)
        {
            _alert.Close();
            _controller.PushViewController(new PowerManipulationViewController(_presenter, Core.Models.Enums.PowerAction.PowerDown), true);
        }

        private void OnCancelPowerDownPressed(object sender, EventArgs args)
        {
            _alert.Close();
            var at = new NSMutableAttributedString();
            at.Append(new NSAttributedString(string.Format(AppDelegate.Localization.GetText(LocalizationKeys.CancelPowerDownAlert),
                                                           _presenter.Balances[0].ToWithdraw.ToBalanceValueString())));
            TransferDialogPopup.Create(_controller, at, _continuePowerDownCancellation);
        }

        public void CleanupPopup()
        {
            _powerUpButton.TouchDown -= OnPowerUpPressed;
            _powerDownButton.TouchDown -= OnPowerDownPressed;
            _cancelPowerDownButton.TouchDown -= OnCancelPowerDownPressed;
        }
    }
}
