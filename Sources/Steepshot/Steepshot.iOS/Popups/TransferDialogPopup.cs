using System;
using Foundation;
using PureLayout.Net;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;
using Steepshot.iOS.CustomViews;
using Steepshot.iOS.Helpers;
using UIKit;
using Steepshot.Core.Extensions;

namespace Steepshot.iOS.Popups
{
    public static class TransferDialogPopup
    {
        private static readonly UIStringAttributes _noLinkAttribute = new UIStringAttributes
        {
            Font = Constants.Regular20,
            ForegroundColor = Constants.R15G24B30,
        };

        private static readonly UIStringAttributes _linkAttribute = new UIStringAttributes
        {
            Font = Constants.Regular20,
            ForegroundColor = Constants.R255G0B0,
        };

        public static void Create(UINavigationController controller,
                                  string amount,
                                  Action<bool> dialogAction,
                                  CurrencyType type = CurrencyType.Steem,
                                  string recipient = null,
                                  PowerAction powerAction = PowerAction.None)
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
            title.Lines = 3;
            popup.AddSubview(title);
            title.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 34);
            title.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            title.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            var at = new NSMutableAttributedString();
            if (powerAction == PowerAction.None)
            {
                at.Append(new NSAttributedString($"Are you sure you want to transfer {amount} {type.ToString()} to ", _noLinkAttribute));
                at.Append(new NSAttributedString($"@{recipient}", _linkAttribute));
                at.Append(new NSAttributedString("?", _noLinkAttribute));
            }
            else
            {
                at.Append(new NSAttributedString($"Are you sure you want to {powerAction.GetDescription()} {amount} {type.ToString()}?", _noLinkAttribute));
            }

            title.AttributedText = at;

            var separator = new UIView();
            separator.BackgroundColor = Constants.R245G245B245;
            popup.AddSubview(separator);

            separator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, title, 36);
            separator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            separator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            separator.AutoSetDimension(ALDimension.Height, 1);

            var yesButton = new UIButton();
            yesButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.Yes), UIControlState.Normal);
            yesButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            yesButton.Layer.CornerRadius = 25;
            yesButton.Font = Constants.Bold14;
            popup.AddSubview(yesButton);

            yesButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, separator, 20);
            yesButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            yesButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            yesButton.AutoSetDimension(ALDimension.Height, 50);

            var cancelButton = new UIButton();
            cancelButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.Cancel), UIControlState.Normal);
            cancelButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            cancelButton.Layer.CornerRadius = 25;
            cancelButton.Font = Constants.Semibold14;
            cancelButton.BackgroundColor = Constants.R255G255B255;
            cancelButton.Layer.BorderWidth = 1;
            cancelButton.Layer.BorderColor = Constants.R245G245B245.CGColor;
            popup.AddSubview(cancelButton);

            cancelButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, yesButton, 10);
            cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, commonMargin);
            cancelButton.AutoSetDimension(ALDimension.Height, 50);

            yesButton.TouchDown += (s, ev) =>
            {
                dialogAction?.Invoke(true);
                _alert.Close();
            };
            cancelButton.TouchDown += (s, ev) =>
            {
                dialogAction?.Invoke(false);
                _alert.Close();
            };

            yesButton.LayoutIfNeeded();
            Constants.CreateGradient(yesButton, 25);
            Constants.CreateShadowFromZeplin(yesButton, Constants.R231G72B0, 0.3f, 0, 10, 20, 0);
            popup.BringSubviewToFront(yesButton);

            _alert.Show();
        }
    }
}
