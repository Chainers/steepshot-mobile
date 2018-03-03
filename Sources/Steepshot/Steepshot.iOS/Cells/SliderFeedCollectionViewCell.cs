using System;
using UIKit;

namespace Steepshot.iOS.Cells
{
    public class SliderFeedCollectionViewCell : UICollectionViewCell
    {
        public FeedCellBuilder Cell;

        protected SliderFeedCollectionViewCell(IntPtr handle) : base(handle)
        {
            Cell = new FeedCellBuilder(ContentView);
        }
    }
}
