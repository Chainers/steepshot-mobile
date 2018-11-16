using System;
using System.ComponentModel;
using Steepshot.Core.Authorization;
using Steepshot.Core.Presenters;
using System.Linq;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using System.Runtime.CompilerServices;

namespace Steepshot.Core.Facades
{
    public sealed class WalletFacade : INotifyPropertyChanged
    {
        private readonly UserManager _userManager;

        private WalletModel[] _wallets;
        public WalletModel[] Wallets
        {
            get
            {
                if (_wallets == null)
                {
                    _wallets = _userManager.Select()
                        .OrderByDescending(x => x.LoginTime)
                        .Select(u => new WalletModel(u))
                        .ToArray();
                }

                return _wallets;
            }
        }

        private BalanceModel[] _balances;
        public BalanceModel[] Balances
        {
            get
            {
                if (_balances == null)
                {
                    var bCount = Wallets.Sum(w => w.UserInfo.AccountInfo.Balances.Length);
                    _balances = new BalanceModel[bCount];

                    var i = 0;
                    foreach (var wallet in Wallets)
                        foreach (var balance in wallet.UserInfo.AccountInfo.Balances)
                            _balances[i++] = balance;
                }
                return _balances;
            }
        }

        private CurrencyRate[] _currencyRates;

        public int BalanceCount => Balances.Length;

        private int _selected = 0;
        public int Selected
        {
            get => _selected;
            set
            {
                if (_selected == value)
                    return;

                _selected = value;

                var i = _selected;

                foreach (var wallet in Wallets)
                {
                    if (wallet.UserInfo.AccountInfo.Balances.Length <= i)
                    {
                        i -= wallet.UserInfo.AccountInfo.Balances.Length;
                        continue;
                    }

                    SelectedWallet = wallet;
                    SelectedBalance = wallet.UserInfo.AccountInfo.Balances[i];
                    break;
                }

                NotifyPropertyChanged();
            }
        }

        private WalletModel _selectedWallet;
        public WalletModel SelectedWallet
        {
            get => _selectedWallet ?? (_selectedWallet = Wallets[0]);
            private set => _selectedWallet = value;
        }

        private BalanceModel _selectedBalance;
        public BalanceModel SelectedBalance
        {
            get => _selectedBalance ?? (_selectedBalance = SelectedWallet.UserInfo.AccountInfo.Balances[0]);
            private set => _selectedBalance = value;
        }

        public readonly WalletPresenter WalletPresenter;
        public readonly TransferPresenter TransferPresenter;

        public WalletFacade(WalletPresenter walletPresenter, TransferPresenter transferPresenter, UserManager userManager)
        {
            WalletPresenter = walletPresenter;
            TransferPresenter = transferPresenter;
            _userManager = userManager;
        }

        public async Task TryUpdateWallets()
        {
            foreach (var wallet in Wallets)
                await TryUpdateWallet(wallet.UserInfo)
                    .ConfigureAwait(true);
        }

        public async Task<OperationResult<AccountInfoResponse>> TryUpdateWallet(UserInfo userInfo)
        {
            var walletModel = Wallets.First(w => w.UserInfo.Id == userInfo.Id);

            await TryGetAccountHistoryAsync(walletModel);

            var result = await WalletPresenter.TryUpdateAccountInfoAsync(userInfo)
                .ConfigureAwait(true);

            return result;
        }

        public async Task TryGetAccountHistoryAsync(WalletModel model, bool isLoop = false)
        {
            var args = new AccountHistoryModel(model.UserInfo.Login);
            bool isChanged = false;

            while (isLoop && !isChanged && !model.IsLastReaded)
            {
                args.Start = model.HistoryStartId;

                var result = await WalletPresenter.TryGetAccountHistoryAsync(args, model.UserInfo.Chain)
                    .ConfigureAwait(true);

                if (result.IsSuccess)
                {
                    model.Update(result.Result);
                    isChanged = result.Result.Count > 0;
                }
            }
        }

        public async Task TryGetCurrencyRatesAsync()
        {
            if (_currencyRates == null)
            {
                var result = await WalletPresenter.TryGetCurrencyRatesAsync()
                    .ConfigureAwait(false);
                if (result.IsSuccess)
                    _currencyRates = result.Result;
            }
        }


        public CurrencyRate GetCurrencyRate(CurrencyType currency)
        {
            return _currencyRates?.FirstOrDefault(x => x.Symbol.Equals(currency.ToString(), StringComparison.OrdinalIgnoreCase)) ?? new CurrencyRate
            {
                Symbol = currency.ToString(),
                UsdRate = 1
            };
        }

        public void TasksCancel()
        {
            WalletPresenter.TasksCancel();
            TransferPresenter.TasksCancel();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
