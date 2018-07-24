using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Steepshot.iOS.Cells;
using Steepshot.iOS.Views;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class TransferCollectionViewSource : UICollectionViewSource
    {
        private List<Transfer> _transferHistory = new List<Transfer>()
        {
            new Transfer() { Time = new DateTime(2018, 5, 9, 23, 23, 0), To = "steepshit", Amount = "0.002"},
            new Transfer() { Time = new DateTime(2018, 5, 9, 14, 43, 12), To = "steepshat", Amount = "0.1"},
            new Transfer() { Time = new DateTime(2017, 8, 18, 16, 32, 0), To = "steepshot", Amount = "0.11"},
            new Transfer() { Time = new DateTime(2017, 8, 18, 23, 59, 59), To = "steepshet", Amount = "0.111111"},
            new Transfer() { Time = new DateTime(2017, 8, 18, 0, 0, 0), To = "steepshyt", Amount = "0.12"},
            new Transfer() { Time = new DateTime(2018, 5, 9, 4, 43, 12), To = "qw", Amount = "0.9"},
            new Transfer() { Time = new DateTime(2018, 5, 9, 7, 32, 12), To = "qwreqwrqwrqw", Amount = "1.002"},
            new Transfer() { Time = new DateTime(2017, 8, 18, 22, 59, 59), To = "okai", Amount = "11.002"},
            new Transfer() { Time = DateTime.Now, To = "sdfdsfdsfsdfsdf", Amount = "11110.002"},
            new Transfer() { Time = DateTime.Now, To = "sdfsd", Amount = "9"},
            new Transfer() { Time = DateTime.Now, To = "sdfdsfdsfdsfdsf", Amount = "999.002"},
            new Transfer() { Time = DateTime.Now, To = "sdfsdfs", Amount = "12321321321.002"},
            new Transfer() { Time = DateTime.Now, To = "sdf", Amount = "1.002"},
            new Transfer() { Time = new DateTime(2017, 8, 18, 17, 23, 45), To = "sdfdsfdsfdsfdsf", Amount = "0.2002"},
            new Transfer() { Time = new DateTime(2017, 7, 18, 17, 23, 45), To = "sdfsdfsd", Amount = "0.543002"},
            new Transfer() { Time = new DateTime(2017, 7, 19, 17, 23, 45), To = "sdfsdfdsfdsfdsfsdfsdfsdfds", Amount = "1230.002"},

        };

        List<IGrouping<DateTime,Transfer>> krot;

        public TransferCollectionViewSource()
        {
            krot = _transferHistory.GroupBy(x => x.Time.Date).ToList();

        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return krot[(int)section].Count();
        }


        [Export("numberOfSectionsInCollectionView:")]
        public override nint NumberOfSections(UICollectionView collectionView)
        {
            return krot.Count();
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var cell = (TransactionCollectionViewCell)collectionView.DequeueReusableCell(nameof(TransactionCollectionViewCell), indexPath);
            cell.UpdateCard(krot[indexPath.Section].ElementAt(indexPath.Row));
            return cell;
        }

        [Export("collectionView:viewForSupplementaryElementOfKind:atIndexPath:")]
        public override UICollectionReusableView GetViewForSupplementaryElement(UICollectionView collectionView, NSString elementKind, NSIndexPath indexPath)
        {
            var header = (TransactionHeaderCollectionViewCell)collectionView.DequeueReusableSupplementaryView(UICollectionElementKindSection.Header, nameof(TransactionHeaderCollectionViewCell), indexPath);
                //cell.UpdateCard(krot[indexPath.Section].ElementAt(indexPath.Row));
                return header;
            
        }
    }
}
