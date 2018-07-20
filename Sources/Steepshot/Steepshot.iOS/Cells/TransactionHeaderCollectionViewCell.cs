using System;
using PureLayout.Net;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public class TransactionHeaderCollectionViewCell : UICollectionReusableView
    {
        private UILabel _date = new UILabel();

        protected TransactionHeaderCollectionViewCell(IntPtr handle) : base(handle)
        {
            _date.Text = "15 Jul 2018";

            AddSubview(_date);

            _date.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 10);
            _date.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
        }
    }
}
