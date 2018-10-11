using System;
using System.Globalization;
using PureLayout.Net;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public class TransactionHeaderCollectionViewCell : UICollectionReusableView
    {
        private readonly UILabel _date = new UILabel();
        private readonly UIView _line;

        protected TransactionHeaderCollectionViewCell(IntPtr handle) : base(handle)
        {
            _line = new UIView
            {
                BackgroundColor = Constants.R240G240B240
            };
            AddSubview(_line);

            _line.AutoSetDimension(ALDimension.Width, 2);
            _line.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 29);
            _line.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
            _line.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);

            _date.Font = Constants.Regular12;
            _date.TextColor = Constants.R151G155B158;

            AddSubview(_date);

            _date.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 10);
            _date.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
        }

        public void Update(DateTime time, bool isFirst)
        {
            _line.Hidden = isFirst;
            _date.Text = time.ToString("dd MMM yyyy", CultureInfo.GetCultureInfo("en-US"));
        }
    }
}
