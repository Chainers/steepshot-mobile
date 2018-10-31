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
        private readonly WalletPresenter _presenter;
        private readonly UINavigationController _controller;
        public List<IGrouping<DateTime, AccountHistoryItem>> GroupedHistory = new List<IGrouping<DateTime, AccountHistoryItem>>();
        public event Action<string> CellAction;
        public CardsContainerHeader Header;

        public TransferCollectionViewSource(WalletPresenter presenter, UINavigationController controller)
        {
            _controller = controller;
            _presenter = presenter;
        }

        public void GroupHistory()
        {
            GroupedHistory = _presenter.Current.AccountHistory.GroupBy(x => x.DateTime.Date).ToList();
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            if (GroupedHistory.Count == 0)
                return 4;
            if (section == 0)
                return 0;
            return GroupedHistory[(int)section - 1].Count();
        }

        [Export("numberOfSectionsInCollectionView:")]
        public override nint NumberOfSections(UICollectionView collectionView)
        {
            if (GroupedHistory.Count == 0)
                return 1;
            return GroupedHistory.Count() + 1;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            if(GroupedHistory.Count == 0)
            {
                var cell = (TransactionShimmerCollectionViewCell)collectionView.DequeueReusableCell(nameof(TransactionShimmerCollectionViewCell), indexPath);
                cell.UpdateCard(indexPath.Row == 0, indexPath.Row == 3);
                return cell;
            }

            var transaction = GroupedHistory[indexPath.Section - 1].ElementAt(indexPath.Row);

            var isFirst = indexPath.Section - 1 == 0 && indexPath.Row == 0;
            var isLast = indexPath.Section - 1 == GroupedHistory.Count() - 1 && indexPath.Row == GroupedHistory[indexPath.Section - 1].Count() - 1;
            if (transaction.Type == AccountHistoryItem.OperationType.ClaimReward)
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

        public void ReloadCardsHeader()
        {
            Header.ReloadCollection();
        }

        [Export("collectionView:viewForSupplementaryElementOfKind:atIndexPath:")]
        public override UICollectionReusableView GetViewForSupplementaryElement(UICollectionView collectionView, NSString elementKind, NSIndexPath indexPath)
        {
            if (indexPath.Section == 0)
            {
                Header = (CardsContainerHeader)collectionView.DequeueReusableSupplementaryView(UICollectionElementKindSection.Header, nameof(CardsContainerHeader), indexPath);
                Header.NavigationController = _controller;
                Header.Presenter = _presenter;
                return Header;
            }
            else
            {
                var header = (TransactionHeaderCollectionViewCell)collectionView.DequeueReusableSupplementaryView(UICollectionElementKindSection.Header, nameof(TransactionHeaderCollectionViewCell), indexPath);
                header.Update(GroupedHistory[indexPath.Section - 1].ElementAt(indexPath.Row).DateTime, indexPath.Section == 1 && indexPath.Row == 0);
                return header;
            }
        }
    }
}
