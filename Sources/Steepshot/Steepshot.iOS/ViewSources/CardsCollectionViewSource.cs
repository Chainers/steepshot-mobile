using System;
using Foundation;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class CardsCollectionViewSource : UICollectionViewSource
    {
        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return 8;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var cell = (CardCollectionViewCell)collectionView.DequeueReusableCell(nameof(CardCollectionViewCell), indexPath);
            cell.UpdateCard(indexPath.Row);
            return cell;
        }
    }
}
