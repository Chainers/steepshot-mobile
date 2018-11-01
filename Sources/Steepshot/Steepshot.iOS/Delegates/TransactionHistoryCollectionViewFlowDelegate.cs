using System;
using System.Linq;
using CoreGraphics;
using Foundation;
using Steepshot.Core.Models.Responses;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.Delegates
{
    public class TransactionHistoryCollectionViewFlowDelegate : UICollectionViewDelegateFlowLayout
    {
        private TransferCollectionViewSource _source;

        public TransactionHistoryCollectionViewFlowDelegate(TransferCollectionViewSource source)
        {
            _source = source;
        }

        public override CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
        {
            if (_source.GroupedHistory == null)
                return new CGSize(UIScreen.MainScreen.Bounds.Width, 86);

            var transaction = _source.GroupedHistory[indexPath.Section - 1].ElementAt(indexPath.Row);
            if (transaction.Type == AccountHistoryItem.OperationType.ClaimReward)
                return new CGSize(UIScreen.MainScreen.Bounds.Width, 194);
            return new CGSize(UIScreen.MainScreen.Bounds.Width, 86);
        }

        public override CGSize GetReferenceSizeForHeader(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
        {
            if (section == 0)
                if (DeviceHelper.IsPlusDevice)
                    return new CGSize(UIScreen.MainScreen.Bounds.Width, 420);
                else if (DeviceHelper.IsSmallDevice)
                    return new CGSize(UIScreen.MainScreen.Bounds.Width, 370);
                else
                    return new CGSize(UIScreen.MainScreen.Bounds.Width, 400);
            return new CGSize(UIScreen.MainScreen.Bounds.Width, 53);
        }
    }
}
