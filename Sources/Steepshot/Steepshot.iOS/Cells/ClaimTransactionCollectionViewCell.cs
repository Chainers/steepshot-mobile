using System;
using CoreGraphics;
using Foundation;
using PureLayout.Net;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public class ClaimTransactionCollectionViewCell : UICollectionViewCell
    {
        private UILabel _action = new UILabel();
        private UILabel steemAmount = new UILabel();
        private UILabel sbdAmount = new UILabel();
        private UILabel spAmount = new UILabel();

        private UIView _topLine;
        private UIView _bottomLine;

        protected ClaimTransactionCollectionViewCell(IntPtr handle) : base(handle)
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

            _action.Font = Constants.Semibold14;
            background.AddSubview(_action);

            _action.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 18);
            _action.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);
            _action.AutoPinEdgeToSuperviewEdge(ALEdge.Right);

            var steemAmountView = new UIView();
            steemAmountView.BackgroundColor = Constants.R250G250B250;
            steemAmountView.Layer.CornerRadius = 8;
            background.AddSubview(steemAmountView);

            steemAmountView.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _action, 16);
            steemAmountView.AutoSetDimension(ALDimension.Height, 32);
            steemAmountView.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);
            steemAmountView.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 30);

            var steemAmountLabel = new UILabel();
            steemAmountLabel.Text = "Steem";
            steemAmountLabel.Font = Constants.Regular14;
            steemAmountView.AddSubview(steemAmountLabel);

            steemAmountLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            steemAmountLabel.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);

            steemAmount.Font = Constants.Semibold14;
            steemAmount.TextColor = Constants.R255G34B5;
            steemAmount.TextAlignment = UITextAlignment.Right;
            steemAmountView.AddSubview(steemAmount);
            steemAmount.SetContentHuggingPriority(1, UILayoutConstraintAxis.Horizontal);

            steemAmount.AutoPinEdge(ALEdge.Left, ALEdge.Right, steemAmountLabel, 5);
            steemAmount.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            steemAmount.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);


            var sbdAmountView = new UIView();
            sbdAmountView.BackgroundColor = Constants.R250G250B250;
            sbdAmountView.Layer.CornerRadius = 8;
            background.AddSubview(sbdAmountView);

            sbdAmountView.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, steemAmountView, 8);
            sbdAmountView.AutoSetDimension(ALDimension.Height, 32);
            sbdAmountView.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);
            sbdAmountView.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 30);

            var sbdAmountLabel = new UILabel();
            sbdAmountLabel.Text = "SBD";
            sbdAmountLabel.Font = Constants.Regular14;
            sbdAmountView.AddSubview(sbdAmountLabel);

            sbdAmountLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            sbdAmountLabel.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);

            sbdAmount.Font = Constants.Semibold14;
            sbdAmount.TextColor = Constants.R255G34B5;
            sbdAmount.TextAlignment = UITextAlignment.Right;
            sbdAmountView.AddSubview(sbdAmount);
            sbdAmount.SetContentHuggingPriority(1, UILayoutConstraintAxis.Horizontal);

            sbdAmount.AutoPinEdge(ALEdge.Left, ALEdge.Right, sbdAmountLabel, 5);
            sbdAmount.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            sbdAmount.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);


            var spAmountView = new UIView();
            spAmountView.BackgroundColor = Constants.R250G250B250;
            spAmountView.Layer.CornerRadius = 8;
            background.AddSubview(spAmountView);

            spAmountView.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, sbdAmountView, 8);
            spAmountView.AutoSetDimension(ALDimension.Height, 32);
            spAmountView.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);
            spAmountView.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 30);

            var spAmountLabel = new UILabel();
            spAmountLabel.Text = "Steem Power";
            spAmountLabel.Font = Constants.Regular14;
            spAmountView.AddSubview(spAmountLabel);

            spAmountLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            spAmountLabel.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);

            spAmount.Font = Constants.Semibold14;
            spAmount.TextColor = Constants.R255G34B5;
            spAmount.TextAlignment = UITextAlignment.Right;
            spAmountView.AddSubview(spAmount);
            spAmount.SetContentHuggingPriority(1, UILayoutConstraintAxis.Horizontal);

            spAmount.AutoPinEdge(ALEdge.Left, ALEdge.Right, spAmountLabel, 5);
            spAmount.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            spAmount.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);


            var circle = new UIView();
            circle.BackgroundColor = UIColor.FromRGB(230, 230, 230);
            circle.Layer.CornerRadius = 4;
            emptyBackground.AddSubview(circle);

            circle.AutoSetDimensionsToSize(new CGSize(8, 8));
            circle.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            circle.AutoAlignAxisToSuperviewAxis(ALAxis.Vertical);

            _topLine = new UIView();
            _topLine.BackgroundColor = UIColor.FromRGB(240, 240, 240);
            _topLine.Layer.CornerRadius = 1;
            emptyBackground.AddSubview(_topLine);

            _topLine.AutoSetDimension(ALDimension.Width, 2);
            _topLine.AutoAlignAxisToSuperviewAxis(ALAxis.Vertical);
            _topLine.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
            _topLine.AutoPinEdge(ALEdge.Bottom, ALEdge.Top, circle, -16);

            _bottomLine = new UIView();
            _bottomLine.BackgroundColor = UIColor.FromRGB(240, 240, 240);
            _bottomLine.Layer.CornerRadius = 1;
            emptyBackground.AddSubview(_bottomLine);

            _bottomLine.AutoSetDimension(ALDimension.Width, 2);
            _bottomLine.AutoAlignAxisToSuperviewAxis(ALAxis.Vertical);
            _bottomLine.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            _bottomLine.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, circle, 16);
        }

        public void UpdateCard(AccountHistoryResponse transaction, bool isFirst, bool isLast)
        {
            _topLine.Hidden = isFirst;
            _bottomLine.Hidden = isLast;
            _action.Text = transaction.Type.ToString();
            steemAmount.Text = transaction.RewardSteem;
            sbdAmount.Text = transaction.RewardSbd;
            spAmount.Text = transaction.RewardSp;
        }
    }
}
