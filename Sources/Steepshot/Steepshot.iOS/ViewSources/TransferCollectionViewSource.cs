using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class TransferCollectionViewSource : UICollectionViewSource
    {
        public List<IGrouping<DateTime, AccountHistoryResponse>> GroupedHistory = new List<IGrouping<DateTime, AccountHistoryResponse>>();
        private WalletPresenter _presenter;
        public event Action<string> CellAction;

        public TransferCollectionViewSource(WalletPresenter presenter)
        {
            _presenter = presenter;
        }

        public void GroupHistory()
        {
            GroupedHistory = _presenter.Current.AccountHistory.GroupBy(x => x.DateTime.Date).ToList();
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return GroupedHistory[(int)section].Count();
        }

        [Export("numberOfSectionsInCollectionView:")]
        public override nint NumberOfSections(UICollectionView collectionView)
        {
            return GroupedHistory.Count();
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var transaction = GroupedHistory[indexPath.Section].ElementAt(indexPath.Row);

            var isFirst = indexPath.Section == 0 && indexPath.Row == 0;
            var isLast = indexPath.Section == GroupedHistory.Count() - 1 && indexPath.Row == GroupedHistory[indexPath.Section].Count() - 1;
            if (transaction.Type == AccountHistoryResponse.OperationType.ClaimReward)
            {
                var cell = (ClaimTransactionCollectionViewCell)collectionView.DequeueReusableCell(nameof(ClaimTransactionCollectionViewCell), indexPath);
                cell.UpdateCard(transaction, isFirst, isLast);
                return cell;
            }
            else
            {
                var cell = (TransactionCollectionViewCell)collectionView.DequeueReusableCell(nameof(TransactionCollectionViewCell), indexPath);
                cell.CellAction += CellAction;
                cell.UpdateCard(transaction, isFirst, isLast);
                return cell;
            }
        }

        [Export("collectionView:viewForSupplementaryElementOfKind:atIndexPath:")]
        public override UICollectionReusableView GetViewForSupplementaryElement(UICollectionView collectionView, NSString elementKind, NSIndexPath indexPath)
        {
            var header = (TransactionHeaderCollectionViewCell)collectionView.DequeueReusableSupplementaryView(UICollectionElementKindSection.Header, nameof(TransactionHeaderCollectionViewCell), indexPath);
            header.Update(GroupedHistory[indexPath.Section].ElementAt(indexPath.Row).DateTime, indexPath.Section == 0 && indexPath.Row == 0);
            return header;
        }
    }
}
