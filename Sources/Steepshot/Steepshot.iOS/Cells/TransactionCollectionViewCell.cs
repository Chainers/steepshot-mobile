using System;
using CoreGraphics;
using PureLayout.Net;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.Views;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public class TransactionCollectionViewCell : UICollectionViewCell
    {
        private UILabel _action = new UILabel();
        private UILabel _to = new UILabel();
        private UILabel _amount = new UILabel();

        protected TransactionCollectionViewCell(IntPtr handle) : base(handle)
        {
            var background = new UIView();
            background.BackgroundColor = Constants.R255G255B255;
            background.Layer.CornerRadius = 16;
            ContentView.AddSubview(background);

            background.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 60);
            background.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 5);
            background.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 10);
            background.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, 5);

            var emptyBackground = new UIView();
            ContentView.AddSubview(emptyBackground);

            emptyBackground.AutoPinEdge(ALEdge.Right, ALEdge.Left, background);
            emptyBackground.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            emptyBackground.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
            emptyBackground.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);

            var leftContainer = new UIView();
            leftContainer.BackgroundColor = UIColor.Green;
            background.AddSubview(leftContainer);

            leftContainer.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);
            leftContainer.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);

            _action.Text = "Send token";
            leftContainer.AddSubview(_action);

            _action.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
            _action.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _action.AutoPinEdgeToSuperviewEdge(ALEdge.Right);

            _to.Text = "steepshot2324234234234234324";
            leftContainer.AddSubview(_to);

            _to.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _action);
            _to.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            _to.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _to.AutoPinEdgeToSuperviewEdge(ALEdge.Right);

            _amount.Text = "10000.001";
            background.AddSubview(_amount);

            _amount.AutoPinEdge(ALEdge.Left, ALEdge.Right, leftContainer);
            _amount.AutoAlignAxis(ALAxis.Horizontal, leftContainer);
            _amount.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 30);
            _amount.AutoSetDimension(ALDimension.Width, 86);

            var circle = new UIView();
            circle.BackgroundColor = UIColor.FromRGB(230, 230, 230);
            circle.Layer.CornerRadius = 4;
            emptyBackground.AddSubview(circle);

            circle.AutoSetDimensionsToSize(new CGSize(8, 8));
            circle.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            circle.AutoAlignAxisToSuperviewAxis(ALAxis.Vertical);

            var topLine = new UIView();
            topLine.BackgroundColor = UIColor.FromRGB(240, 240, 240);
            topLine.Layer.CornerRadius = 1;
            emptyBackground.AddSubview(topLine);

            topLine.AutoSetDimension(ALDimension.Width, 2);
            topLine.AutoAlignAxisToSuperviewAxis(ALAxis.Vertical);
            topLine.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
            topLine.AutoPinEdge(ALEdge.Bottom, ALEdge.Top, circle, -16);

            var bottomLine = new UIView();
            bottomLine.BackgroundColor = UIColor.FromRGB(240, 240, 240);
            bottomLine.Layer.CornerRadius = 1;
            emptyBackground.AddSubview(bottomLine);

            bottomLine.AutoSetDimension(ALDimension.Width, 2);
            bottomLine.AutoAlignAxisToSuperviewAxis(ALAxis.Vertical);
            bottomLine.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            bottomLine.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, circle, 16);

            /*
            _amount.BackgroundColor = UIColor.White;
            ContentView.AddSubview(_amount);
            _amount.AutoPinEdge(ALEdge.Left, ALEdge.Left, _to, 20);
            _amount.AutoPinEdge(ALEdge.Right, ALEdge.Right, _to, -20);
            _amount.AutoPinEdge(ALEdge.Bottom, ALEdge.Bottom, _to);
            _amount.AutoPinEdge(ALEdge.Top, ALEdge.Top, _to);
            ContentView.BringSubviewToFront(_to);

            var loginBackground = new UIView();
            loginBackground.Layer.CornerRadius = 15;
            loginBackground.BackgroundColor = Constants.R255G255B255.ColorWithAlpha(0.1f);

            _to.AddSubview(loginBackground);

            loginBackground.AutoSetDimensionsToSize(new CGSize(140, 32));
            loginBackground.AutoPinEdgeToSuperviewEdge(ALEdge.Right, -20);
            loginBackground.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 21);

            var login = new UILabel();
            login.Text = "@steepshot";
            login.Font = Constants.Semibold14;
            login.TextColor = Constants.R255G255B255;
            loginBackground.AddSubview(login);

            login.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            login.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);
            login.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 21);

            var balanceLabel = new UILabel();
            balanceLabel.Text = "Account balance";
            balanceLabel.Font = Constants.Semibold14;
            balanceLabel.TextColor = Constants.R255G255B255.ColorWithAlpha(0.5f);
            _to.AddSubview(balanceLabel);

            balanceLabel.AutoAlignAxis(ALAxis.Horizontal, loginBackground);
            balanceLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);

            var equivalentBalance = new UILabel();
            equivalentBalance.Text = "$ 1 999.999";
            equivalentBalance.Font = Constants.Bold34;
            equivalentBalance.TextColor = Constants.R255G255B255;
            _to.AddSubview(equivalentBalance);

            equivalentBalance.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);
            equivalentBalance.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, loginBackground);
            equivalentBalance.AutoPinEdgeToSuperviewEdge(ALEdge.Right);

            var bottomContainer = new UIView();
            _to.AddSubview(bottomContainer);

            bottomContainer.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);
            bottomContainer.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 30);
            bottomContainer.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            bottomContainer.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, equivalentBalance);

            var steemContainer = new UIView();
            bottomContainer.AddSubview(steemContainer);

            steemContainer.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            steemContainer.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            steemContainer.AutoMatchDimensionWithMultiplier(ALDimension.Width, ALDimension.Width, bottomContainer, 0.5f);

            var steemLabel = new UILabel();
            steemLabel.Text = "Steem";
            steemLabel.Font = Constants.Semibold14;
            steemLabel.TextColor = Constants.R255G255B255.ColorWithAlpha(0.5f);
            steemContainer.AddSubview(steemLabel);

            steemLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            steemLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            steemLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Top);

            var steemValue = new UILabel();
            steemValue.Text = "999.999";
            steemValue.Font = Constants.Semibold20;
            steemValue.TextColor = Constants.R255G255B255;
            steemContainer.AddSubview(steemValue);

            steemValue.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            steemValue.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            steemValue.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            steemValue.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, steemLabel);

            var steemPowerContainer = new UIView();
            bottomContainer.AddSubview(steemPowerContainer);

            steemPowerContainer.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            steemPowerContainer.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            steemPowerContainer.AutoMatchDimensionWithMultiplier(ALDimension.Width, ALDimension.Width, bottomContainer, 0.5f);

            var steemPowerLabel = new UILabel();
            steemPowerLabel.Text = "Steem Power";
            steemPowerLabel.Font = Constants.Semibold14;
            steemPowerLabel.TextColor = Constants.R255G255B255.ColorWithAlpha(0.5f);
            steemPowerContainer.AddSubview(steemPowerLabel);

            steemPowerLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            steemPowerLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            steemPowerLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Top);

            var steemPowerValue = new UILabel();
            steemPowerValue.Text = "999.999";
            steemPowerValue.Font = Constants.Semibold20;
            steemPowerValue.TextColor = Constants.R255G255B255;
            steemPowerContainer.AddSubview(steemPowerValue);

            steemPowerValue.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            steemPowerValue.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            steemPowerValue.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            steemPowerValue.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, steemPowerLabel);
            */
        }

        public void UpdateCard(Transfer i)
        {
            /*
            _action.Text = "Send token";
            background.AddSubview(_to);

            _to.Text = "steepshot";
            background.AddSubview(_to);

            _amount.Text = "0.001";
            background.AddSubview(_amount);
            */
        }
    }
}
