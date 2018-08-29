using System;
using PureLayout.Net;
using Steepshot.Core.Localization;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.CustomViews;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.Views;
using UIKit;

namespace Steepshot.iOS.Popups
{
    public class PowerManipulationPopup
    {
        public static void Create(UINavigationController controller, WalletPresenter _presenter)
        {
            var commonMargin = 20;

            var popup = new UIView();
            popup.ClipsToBounds = true;
            popup.Layer.CornerRadius = 20;
            popup.BackgroundColor = Constants.R250G250B250;

            var _alert = new CustomAlertView(popup, controller);

            var dialogWidth = UIScreen.MainScreen.Bounds.Width - 10 * 2;
            popup.AutoSetDimension(ALDimension.Width, dialogWidth);

            var title = new UILabel();
            title.TextAlignment = UITextAlignment.Center;
            title.Font = Constants.Semibold14;
            title.Text = "SELECT ACTION";
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

            var powerUpButton = new UIButton();
            powerUpButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.PowerUp), UIControlState.Normal);
            powerUpButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            powerUpButton.BackgroundColor = Constants.R255G255B255;
            powerUpButton.Layer.CornerRadius = 25;
            powerUpButton.Font = Constants.Semibold14;
            powerUpButton.Layer.BorderWidth = 1;
            powerUpButton.Layer.BorderColor = Constants.R245G245B245.CGColor;
            popup.AddSubview(powerUpButton);

            powerUpButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, separator, 20);
            powerUpButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            powerUpButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            powerUpButton.AutoSetDimension(ALDimension.Height, 50);

            var powerDownButton = new UIButton();
            powerDownButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.PowerDown), UIControlState.Normal);
            powerDownButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            powerDownButton.BackgroundColor = Constants.R255G255B255;
            powerDownButton.Layer.CornerRadius = 25;
            powerDownButton.Font = Constants.Semibold14;
            powerDownButton.Layer.BorderWidth = 1;
            powerDownButton.Layer.BorderColor = Constants.R245G245B245.CGColor;
            popup.AddSubview(powerDownButton);

            powerDownButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, powerUpButton, 10);
            powerDownButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            powerDownButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            powerDownButton.AutoSetDimension(ALDimension.Height, 50);

            var bottomSeparator = new UIView();
            bottomSeparator.BackgroundColor = Constants.R245G245B245;
            popup.AddSubview(bottomSeparator);

            bottomSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, powerDownButton, 26);
            bottomSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            bottomSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            bottomSeparator.AutoSetDimension(ALDimension.Height, 1);

            var cancelButton = new UIButton();
            cancelButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.Cancel), UIControlState.Normal);
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

            powerUpButton.TouchDown += (s, ev) =>
            {
                _alert.Close();
                controller.PushViewController(new PowerManipulationViewController(_presenter.Balances[0], Core.Models.Enums.PowerAction.PowerUp), true);
            };
            powerDownButton.TouchDown += (s, ev) =>
            {
                _alert.Close();
                controller.PushViewController(new PowerManipulationViewController(_presenter.Balances[0], Core.Models.Enums.PowerAction.PowerDown), true);

            };
            cancelButton.TouchDown += (s, ev) => { _alert.Close(); };

            _alert.Show();
        }
    }
}
