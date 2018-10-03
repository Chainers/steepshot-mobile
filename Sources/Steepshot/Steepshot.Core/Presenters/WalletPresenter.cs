using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ditch.Core.JsonRpc;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public class WalletPresenter : BasePresenter, IEnumerator<UserInfo>
    {
        public Dictionary<int, UserInfo> ConnectedUsers { get; }
        public bool HasNext { get; private set; }
        public List<BalanceModel> Balances { get; }
        public Action UpdateWallet;
        private CurrencyRate[] CurrencyRates { get; set; }
        private readonly int[] _logins;
        private int _current = -1;

        private SteepshotApiClient _steepshotApiClient;
        private BaseDitchClient _ditchClient;

        public WalletPresenter(IConnectionService connectionService, ILogService logService, BaseDitchClient ditchClient, SteepshotApiClient steepshotApiClient, UserManager dataProvider)
            : base(connectionService, logService)
        {
            _steepshotApiClient = steepshotApiClient;
            _ditchClient = ditchClient;

            ConnectedUsers = dataProvider.Select().OrderByDescending(x => x.LoginTime).ToDictionary(x => x.Id, x => x);
            _logins = ConnectedUsers.Keys.ToArray();
            Balances = new List<BalanceModel>();
            HasNext = MoveNext();
        }

        public async Task<Exception> TryLoadNextAccountInfoAsync()
        {
            if (!HasNext || Current == null)
                return new ValidationException(string.Empty);

            var exception = await TryUpdateAccountInfoAsync(Current).ConfigureAwait(false);
            if (exception == null)
                HasNext = MoveNext();

            return exception;
        }

        public async Task<Exception> TryUpdateAccountInfoAsync(UserInfo userInfo)
        {
            if (!ConnectedUsers.ContainsKey(userInfo.Id))
                return new ValidationException(string.Empty);

            var response = await TaskHelper
                .TryRunTaskAsync(_ditchClient.GetAccountInfoAsync, userInfo.Login, OnDisposeCts.Token)
                .ConfigureAwait(false);
            if (response.IsSuccess)
            {
                ConnectedUsers[userInfo.Id].AccountInfo = response.Result;
                var responseBalances = response.Result.Balances;
                responseBalances.ForEach(x => x.UserInfo = ConnectedUsers[userInfo.Id]);
                lock (Balances)
                {
                    responseBalances.ForEach(x =>
                        {
                            var balanceInd = Balances.FindIndex(y => y.UserInfo.Id == x.UserInfo.Id && y.CurrencyType == x.CurrencyType);
                            if (balanceInd >= 0)
                                Balances[balanceInd] = x;
                            else
                                Balances.Add(x);
                        });
                }
            }
            else
            {
                return response.Exception;
            }

            var historyResp = await TaskHelper.TryRunTaskAsync(_ditchClient.GetAccountHistoryAsync, userInfo.Login, OnDisposeCts.Token).ConfigureAwait(false);
            if (historyResp.IsSuccess)
            {
                ConnectedUsers[userInfo.Id].AccountHistory = historyResp.Result;
                return null;
            }

            return historyResp.Exception;
        }

        public async Task<OperationResult<VoidResponse>> TryClaimRewardsAsync(BalanceModel balance)
        {
            var claimRewardsModel = new ClaimRewardsModel(balance.UserInfo, balance.RewardSteem, balance.RewardSp, balance.RewardSbd);
            var result = await TaskHelper.TryRunTaskAsync(_ditchClient.ClaimRewardsAsync, claimRewardsModel, OnDisposeCts.Token).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                Balances.ForEach(x =>
                {
                    if (x.UserInfo.Chain == balance.UserInfo.Chain &&
                        x.UserInfo.Login.Equals(balance.UserInfo.Login, StringComparison.OrdinalIgnoreCase))
                    {
                        x.RewardSteem = x.RewardSbd = x.RewardSp = 0;
                    }
                });
                await TryUpdateAccountInfoAsync(balance.UserInfo).ConfigureAwait(false);
            }

            return result;
        }

        public async Task<OperationResult<CurrencyRate[]>> TryGetCurrencyRatesAsync()
        {
            var result = await TaskHelper.TryRunTaskAsync(_steepshotApiClient.GetCurrencyRatesAsync, OnDisposeCts.Token).ConfigureAwait(false);
            if (result.IsSuccess)
                CurrencyRates = result.Result;
            return result;
        }

        public CurrencyRate GetCurrencyRate(CurrencyType currency)
        {
            return CurrencyRates?.FirstOrDefault(x => x.Symbol.Equals(currency.ToString(), StringComparison.OrdinalIgnoreCase)) ?? new CurrencyRate
            {
                Symbol = currency.ToString(),
                UsdRate = 1
            };
        }

        public void SetClient(SteepshotApiClient steepshotApiClient, BaseDitchClient ditchClient)
        {
            TasksCancel();

            _steepshotApiClient = steepshotApiClient;
            _ditchClient = ditchClient;
        }

        
        public async Task<OperationResult<VoidResponse>> TryPowerUpOrDownAsync(BalanceModel balance, PowerAction powerAction)
        {
            var model = new PowerUpDownModel(balance, powerAction);
            return await TaskHelper.TryRunTaskAsync(_ditchClient.PowerUpOrDownAsync, model, OnDisposeCts.Token).ConfigureAwait(false);
        }

        #region IEnumerator<UserInfo>

        object IEnumerator.Current => Current;
        public UserInfo Current => ConnectedUsers[_logins[_current]];

        public void Reset()
        {
            _current = -1;
            HasNext = MoveNext();
        }

        public bool MoveNext()
        {
            var hasNext = _current + 1 < _logins.Length;
            if (hasNext)
                _current += 1;
            return hasNext;
        }

        public void Dispose()
        {
        }

        #endregion
    }
}
