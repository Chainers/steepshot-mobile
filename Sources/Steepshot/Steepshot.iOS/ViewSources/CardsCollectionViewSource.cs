using System;
using Foundation;
using Steepshot.Core.Facades;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class CardsCollectionViewSource : UICollectionViewSource
    {
        private readonly WalletFacade _walletFacade;

        public CardsCollectionViewSource(WalletFacade walletFacade)
        {
            _walletFacade = walletFacade;
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return _walletFacade.BalanceCount;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var cell = (CardCollectionViewCell)collectionView.DequeueReusableCell(nameof(CardCollectionViewCell), indexPath);

            var i = indexPath.Row;

            foreach (var wallet in _walletFacade.Wallets)
            {
                if (wallet.UserInfo.AccountInfo.Balances.Length <= i)
                {
                    i -= wallet.UserInfo.AccountInfo.Balances.Length;
                    continue;
                }

                var balance = wallet.UserInfo.AccountInfo.Balances[i];
                var cr = _walletFacade.GetCurrencyRate(balance.CurrencyType);
                cell.UpdateCard(wallet, balance, cr, indexPath.Row);
                break;
            }

            return cell;
        }
    }
}
