using System;
using CoreGraphics;
using PureLayout.Net;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.CustomViews;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using UIKit;

namespace Steepshot.iOS.Views
{
    public class PowerManipulationViewController : BaseViewControllerWithPresenter<TransferPresenter>
    {
        private readonly BalanceModel _balance;
        private readonly PowerAction _powerAction;
        private double _powerAmount;

        public PowerManipulationViewController(BalanceModel balance, PowerAction action)
        {
            _balance = balance;
            _powerAction = action;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            SetBackButton();
            CreateView();
        }

        private void CreateView()
        {
            View.BackgroundColor = Constants.R250G250B250;

            var topBackground = new UIView();
            topBackground.BackgroundColor = UIColor.White;
            View.AddSubview(topBackground);

            topBackground.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 20);
            topBackground.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            topBackground.AutoPinEdgeToSuperviewEdge(ALEdge.Right);

            var steemView = new UIView();
            topBackground.AddSubview(steemView);

            var label = new UILabel();
            label.Text = "Steem";
            label.Font = Constants.Semibold14;
            steemView.AddSubview(label);

            label.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            label.AutoPinEdgeToSuperviewEdge(ALEdge.Left);

            var label3 = new UILabel();
            label3.Text = "4 > 5";
            steemView.AddSubview(label3);

            label3.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            label3.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            label3.SetContentHuggingPriority(1, UILayoutConstraintAxis.Horizontal);

            steemView.AutoSetDimension(ALDimension.Height, 70);
            steemView.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            steemView.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);
            steemView.AutoPinEdgeToSuperviewEdge(ALEdge.Top);

            var separator = new UIView();
            separator.BackgroundColor = Constants.R245G245B245;

            topBackground.AddSubview(separator);

            separator.AutoSetDimension(ALDimension.Height, 1);
            separator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            separator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);
            separator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, steemView);

            var spView = new UIView();
            topBackground.AddSubview(spView);

            var label2 = new UILabel();
            label2.Text = "SteemPower";
            label2.Font = Constants.Semibold14;
            spView.AddSubview(label2);

            label2.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            label2.AutoPinEdgeToSuperviewEdge(ALEdge.Left);

            var label4 = new UILabel();
            label4.Text = "4 > 8";
            spView.AddSubview(label4);

            label4.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            label4.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            label4.SetContentHuggingPriority(1, UILayoutConstraintAxis.Horizontal);

            spView.AutoSetDimension(ALDimension.Height, 70);
            spView.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, separator);
            spView.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            spView.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);
            spView.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);

            var amountBackground = new UIView();
            amountBackground.BackgroundColor = UIColor.White;
            View.AddSubview(amountBackground);

            amountBackground.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, topBackground, 10);
            amountBackground.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            amountBackground.AutoPinEdgeToSuperviewEdge(ALEdge.Right);

            var amountLabel = new UILabel();
            amountLabel.Text = "Amount";
            amountLabel.Font = Constants.Semibold14;
            amountBackground.AddSubview(amountLabel);

            amountLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 15);
            amountLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);

            var amount = new SearchTextField(() =>
            {
                //RemoveFocus();
            }, AppSettings.LocalizationManager.GetText(LocalizationKeys.TransferAmountHint), new AmountFieldDelegate());
            amount.KeyboardType = UIKeyboardType.DecimalPad;
            amount.Layer.CornerRadius = 25;
            amountBackground.AddSubview(amount);

            amount.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            amount.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, amountLabel, 16);
            amount.AutoSetDimension(ALDimension.Height, 50);
            amount.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, 20);

            var max = new UIButton();
            max.SetTitle("MAX", UIControlState.Normal);
            max.SetTitleColor(UIColor.Black, UIControlState.Normal);
            max.Font = Constants.Semibold14;
            max.Layer.BorderWidth = 1;
            max.Layer.BorderColor = Constants.R245G245B245.CGColor;
            max.Layer.CornerRadius = 25;

            amountBackground.AddSubview(max);

            max.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);
            max.AutoPinEdge(ALEdge.Left, ALEdge.Right, amount, 10);
            max.AutoSetDimensionsToSize(new CGSize(80, 50));
            max.AutoAlignAxis(ALAxis.Horizontal, amount);

            var actionButton = new UIButton();
            actionButton.SetTitle("POWER UP", UIControlState.Normal);
            actionButton.SetTitleColor(UIColor.White, UIControlState.Normal);

            View.AddSubview(actionButton);

            actionButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, amountBackground, 30);
            actionButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            actionButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);
            actionButton.AutoSetDimension(ALDimension.Height, 50);

            actionButton.LayoutIfNeeded();

            Constants.CreateGradient(actionButton, 25);
            Constants.CreateShadowFromZeplin(actionButton, Constants.R231G72B0, 0.3f, 0, 10, 20, 0);
        }

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.SetLeftBarButtonItem(leftBarButton, true);
            NavigationController.NavigationBar.TintColor = Constants.R15G24B30;

            NavigationItem.Title = AppSettings.LocalizationManager.GetText(_powerAction == PowerAction.PowerUp ? LocalizationKeys.PowerUp : LocalizationKeys.PowerDown);

            NavigationController.NavigationBar.Translucent = false;
        }
    }
}
