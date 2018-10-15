using System;
using CoreGraphics;
using PureLayout.Net;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public class ClaimTransactionCollectionViewCell : UICollectionViewCell
    {
        private UILabel _action = new UILabel();
        private UILabel _steemAmount = new UILabel();
        private UILabel _sbdAmount = new UILabel();
        private UILabel _spAmount = new UILabel();

        private UIView _topLine;
        private UIView _bottomLine;

        protected ClaimTransactionCollectionViewCell(IntPtr handle) : base(handle)
        {
            var background = new UIView();
            background.BackgroundColor = Constants.R255G255B255;
            background.Layer.CornerRadius = 16;
            ContentView.AddSubview(background);

            background.AutoPinEdgesToSuperviewEdges(new UIEdgeInsets(5, 60, 5, 10));

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
            steemAmountLabel.Text = CurrencyType.Steem.ToString();
            steemAmountLabel.Font = Constants.Regular14;
            steemAmountView.AddSubview(steemAmountLabel);

            steemAmountLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            steemAmountLabel.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);

            _steemAmount.Font = Constants.Semibold14;
            _steemAmount.TextAlignment = UITextAlignment.Right;
            steemAmountView.AddSubview(_steemAmount);
            _steemAmount.SetContentHuggingPriority(1, UILayoutConstraintAxis.Horizontal);

            _steemAmount.AutoPinEdge(ALEdge.Left, ALEdge.Right, steemAmountLabel, 5);
            _steemAmount.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            _steemAmount.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);


            var sbdAmountView = new UIView();
            sbdAmountView.BackgroundColor = Constants.R250G250B250;
            sbdAmountView.Layer.CornerRadius = 8;
            background.AddSubview(sbdAmountView);

            sbdAmountView.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, steemAmountView, 8);
            sbdAmountView.AutoSetDimension(ALDimension.Height, 32);
            sbdAmountView.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);
            sbdAmountView.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 30);

            var sbdAmountLabel = new UILabel();
            sbdAmountLabel.Text = CurrencyType.Sbd.ToString().ToUpper();
            sbdAmountLabel.Font = Constants.Regular14;
            sbdAmountView.AddSubview(sbdAmountLabel);

            sbdAmountLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            sbdAmountLabel.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);

            _sbdAmount.Font = Constants.Semibold14;
            _sbdAmount.TextAlignment = UITextAlignment.Right;
            sbdAmountView.AddSubview(_sbdAmount);
            _sbdAmount.SetContentHuggingPriority(1, UILayoutConstraintAxis.Horizontal);

            _sbdAmount.AutoPinEdge(ALEdge.Left, ALEdge.Right, sbdAmountLabel, 5);
            _sbdAmount.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            _sbdAmount.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);

           
            var spAmountView = new UIView();
            spAmountView.BackgroundColor = Constants.R250G250B250;
            spAmountView.Layer.CornerRadius = 8;
            background.AddSubview(spAmountView);

            spAmountView.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, sbdAmountView, 8);
            spAmountView.AutoSetDimension(ALDimension.Height, 32);
            spAmountView.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);
            spAmountView.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 30);

            var spAmountLabel = new UILabel();
            spAmountLabel.Text = CurrencyType.SteemPower.ToString();
            spAmountLabel.Font = Constants.Regular14;
            spAmountView.AddSubview(spAmountLabel);

            spAmountLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            spAmountLabel.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);

            _spAmount.Font = Constants.Semibold14;
            _spAmount.TextAlignment = UITextAlignment.Right;
            spAmountView.AddSubview(_spAmount);
            _spAmount.SetContentHuggingPriority(1, UILayoutConstraintAxis.Horizontal);

            _spAmount.AutoPinEdge(ALEdge.Left, ALEdge.Right, spAmountLabel, 5);
            _spAmount.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            _spAmount.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);


            var circle = new UIView();
            circle.BackgroundColor = Constants.R230G230B230;
            circle.Layer.CornerRadius = 4;
            emptyBackground.AddSubview(circle);

            circle.AutoSetDimensionsToSize(new CGSize(8, 8));
            circle.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            circle.AutoAlignAxisToSuperviewAxis(ALAxis.Vertical);

            _topLine = new UIView();
            _topLine.BackgroundColor = Constants.R240G240B240;
            _topLine.Layer.CornerRadius = 1;
            emptyBackground.AddSubview(_topLine);

            _topLine.AutoSetDimension(ALDimension.Width, 2);
            _topLine.AutoAlignAxisToSuperviewAxis(ALAxis.Vertical);
            _topLine.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
            _topLine.AutoPinEdge(ALEdge.Bottom, ALEdge.Top, circle, -16);

            _bottomLine = new UIView();
            _bottomLine.BackgroundColor = Constants.R240G240B240;
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
            _steemAmount.Text = transaction.RewardSteem;
            _sbdAmount.Text = transaction.RewardSbd;
            _spAmount.Text = transaction.RewardSp;
        }
    }
}
