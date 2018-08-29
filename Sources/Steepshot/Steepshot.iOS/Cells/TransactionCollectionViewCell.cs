using System;
using CoreGraphics;
using Foundation;
using PureLayout.Net;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;
using Steepshot.iOS.Delegates;
using Steepshot.iOS.Helpers;
using UIKit;
using Xamarin.TTTAttributedLabel;
using Constants = Steepshot.iOS.Helpers.Constants;

namespace Steepshot.iOS.Cells
{
    public class TransactionCollectionViewCell : UICollectionViewCell
    {
        private UILabel _action = new UILabel();
        private TTTAttributedLabel _to = new TTTAttributedLabel();
        private UILabel _amount = new UILabel();
        private UIView _topLine;
        private UIView _bottomLine;
        public event Action<string> CellAction
        {
            add
            {
                if(_to.Delegate == null)
                    _to.Delegate = new TTTAttributedLabelSimpleDelegate(value);
            }
            remove
            {
                throw new NotImplementedException();
            }
        }

        protected TransactionCollectionViewCell(IntPtr handle) : base(handle)
        {
            var sideMargin = DeviceHelper.IsSmallDevice ? 15 : 30;

            ContentView.UserInteractionEnabled = true;

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
            background.AddSubview(leftContainer);

            leftContainer.AutoPinEdgeToSuperviewEdge(ALEdge.Left, sideMargin);
            leftContainer.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);

            _action.Font = Constants.Semibold14;
            leftContainer.AddSubview(_action);

            _action.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
            _action.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _action.AutoPinEdgeToSuperviewEdge(ALEdge.Right);


            _to.EnabledTextCheckingTypes = NSTextCheckingType.Link;
            var prop = new NSDictionary();
            _to.LinkAttributes = prop;
            _to.ActiveLinkAttributes = prop;
            _to.UserInteractionEnabled = true;
            _to.Enabled = true;
            leftContainer.AddSubview(_to);

            _to.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _action);
            _to.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            _to.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _to.AutoPinEdgeToSuperviewEdge(ALEdge.Right);

            _amount.Font = Constants.Semibold16;
            _amount.TextAlignment = UITextAlignment.Right;
            background.AddSubview(_amount);

            _amount.AutoPinEdge(ALEdge.Left, ALEdge.Right, leftContainer);
            _amount.AutoAlignAxis(ALAxis.Horizontal, leftContainer);
            _amount.AutoPinEdgeToSuperviewEdge(ALEdge.Right, sideMargin);

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
            _amount.Text = transaction.Amount;

            var _noLinkAttribute = new UIStringAttributes
            {
                Font = Constants.Regular12,
                ForegroundColor = Constants.R151G155B158,
            };

            var at = new NSMutableAttributedString();
            at.Append(new NSAttributedString(transaction.From.Equals(AppSettings.User.Login) ? $"to " : $"from ", _noLinkAttribute));
            var login = transaction.From.Equals(AppSettings.User.Login) ? transaction.To : transaction.From;

            var linkAttribute = new UIStringAttributes
            {
                Font = Constants.Semibold12,
                ForegroundColor = Constants.R255G34B5,
                Link = new NSUrl(login),
            };

            at.Append(new NSAttributedString($"@{login}", linkAttribute));

            _to.SetText(at);;
        }
    }
}
