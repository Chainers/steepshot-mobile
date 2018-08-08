using System.Linq;
using CoreGraphics;
using Foundation;
using Steepshot.Core.Models.Responses;
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
            var transaction = _source.GroupedHistory[indexPath.Section].ElementAt(indexPath.Row);
            if (transaction.Type == AccountHistoryResponse.OperationType.ClaimReward)
                return new CGSize(UIScreen.MainScreen.Bounds.Width, 194);
            return new CGSize(UIScreen.MainScreen.Bounds.Width, 86);
        }
    }
}
