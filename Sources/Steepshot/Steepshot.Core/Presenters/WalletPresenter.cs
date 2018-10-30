using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ditch.Core.JsonRpc;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public class WalletPresenter : BasePresenter
    {
        private readonly UserManager _dataProvider;
        private readonly SteemClient _steemClient;
        private readonly GolosClient _golosClient;
        private readonly SteepshotApiClient _steepshotApiClient;

        public WalletPresenter(IConnectionService connectionService, ILogService logService, SteemClient steemClient, GolosClient golosClient, SteepshotApiClient steepshotApiClient, UserManager dataProvider)
            : base(connectionService, logService)
        {
            _steepshotApiClient = steepshotApiClient;

            _dataProvider = dataProvider;
            _steemClient = steemClient;
            _golosClient = golosClient;
        }

        public async Task<OperationResult<AccountInfoResponse>> TryUpdateAccountInfoAsync(UserInfo userInfo)
        {
            BaseDitchClient ditchClient;
            switch (userInfo.Chain)
            {
                case KnownChains.Steem:
                    ditchClient = _steemClient;
                    break;
                case KnownChains.Golos:
                    ditchClient = _golosClient;
                    break;

                default:
                    {
                        throw new NotSupportedException(userInfo.Chain.ToString());
                    }
            }

            var response = await TaskHelper
                .TryRunTaskAsync(ditchClient.GetAccountInfoAsync, userInfo.Login, OnDisposeCts.Token)
                .ConfigureAwait(true);

            if (response.IsSuccess)
            {
                foreach (var newBalance in response.Result.Balances)
                {
                    var oldBalance = userInfo.AccountInfo.Balances.First(b => b.CurrencyType == newBalance.CurrencyType);
                    oldBalance.Update(newBalance);
                }

                _dataProvider.Update(userInfo);
            }

            return response;
        }


        public async Task<OperationResult<AccountHistoryResponse[]>> TryGetAccountHistoryAsync(AccountHistoryModel model, KnownChains chain)
        {
            BaseDitchClient ditchClient;
            switch (chain)
            {
                case KnownChains.Steem:
                    ditchClient = _steemClient;
                    break;
                case KnownChains.Golos:
                    ditchClient = _golosClient;
                    break;

                default:
                    {
                        throw new NotSupportedException(chain.ToString());
                    }
            }

            return await TaskHelper.TryRunTaskAsync(ditchClient.GetAccountHistoryAsync, model, OnDisposeCts.Token)
                .ConfigureAwait(false);
        }


        public async Task<OperationResult<VoidResponse>> TryClaimRewardsAsync(UserInfo userInfo, BalanceModel balance)
        {
            BaseDitchClient ditchClient;
            switch (userInfo.Chain)
            {
                case KnownChains.Steem:
                    ditchClient = _steemClient;
                    break;
                case KnownChains.Golos:
                    ditchClient = _golosClient;
                    break;

                default:
                    {
                        throw new NotSupportedException(userInfo.Chain.ToString());
                    }
            }

            var claimRewardsModel = new ClaimRewardsModel(userInfo, balance.RewardSteem, balance.RewardSp, balance.RewardSbd);
            var result = await TaskHelper.TryRunTaskAsync(ditchClient.ClaimRewardsAsync, claimRewardsModel, OnDisposeCts.Token)
                .ConfigureAwait(false);

            if (result.IsSuccess)
            {
                await TryUpdateAccountInfoAsync(userInfo)
                    .ConfigureAwait(false);
            }

            return result;
        }

        public async Task<OperationResult<CurrencyRate[]>> TryGetCurrencyRatesAsync()
        {
            return await TaskHelper.TryRunTaskAsync(_steepshotApiClient.GetCurrencyRatesAsync, OnDisposeCts.Token)
                .ConfigureAwait(false);
        }
    }

    public class WalletModel : INotifyPropertyChanged
    {
        public AccountHistoryResponse[] AccountHistory = new AccountHistoryResponse[0];
        public readonly UserInfo UserInfo;
        public bool IsHistoryLoaded = false;


        public WalletModel(UserInfo userInfo)
        {
            UserInfo = userInfo;
        }

        public void Update(AccountHistoryResponse[] result)
        {
            IsHistoryLoaded = true;
            AccountHistory = AccountHistory.Union(result).ToArray();
            NotifyPropertyChanged(nameof(AccountHistory));
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
