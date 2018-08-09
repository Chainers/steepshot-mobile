using System;
using Foundation;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class CardsCollectionViewSource : UICollectionViewSource
    {
        private WalletPresenter _presenter;

        public CardsCollectionViewSource(WalletPresenter presenter)
        {
            _presenter = presenter;
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return _presenter.Balances.Count;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var cell = (CardCollectionViewCell)collectionView.DequeueReusableCell(nameof(CardCollectionViewCell), indexPath);
            var currencyRate = _presenter.GetCurrencyRate(_presenter.Balances[indexPath.Row].CurrencyType);
            cell.UpdateCard(_presenter.Balances[indexPath.Row], currencyRate, indexPath.Row);
            return cell;
        }
    }
}
