using System;
using PureLayout.Net;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public class CardCollectionViewCell : UICollectionViewCell
    {
        private UILabel _label;

        protected CardCollectionViewCell(IntPtr handle) : base(handle)
        {
            _label = new UILabel();
            _label.Text = "TRALALALLALALA";

            ContentView.AddSubview(_label);

            _label.AutoPinEdgesToSuperviewEdges();
        }
    }
}
