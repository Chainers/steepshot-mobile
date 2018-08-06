using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Core.JsonRpc;
using Steepshot.Core.Authorization;
using Steepshot.Core.Errors;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public class WalletPresenter : PreSignInPresenter, IEnumerator<UserInfo>
    {
        public Dictionary<int, UserInfo> ConnectedUsers { get; }
        public bool HasNext { get; private set; }
        public List<BalanceModel> Balances { get; }
        public CurrencyRate[] CurrencyRates { get; private set; }
        private readonly int[] _logins;
        private int _current = -1;

        public WalletPresenter()
        {
            ConnectedUsers = AppSettings.DataProvider.Select().ToDictionary(x => x.Id, x => x);
            _logins = ConnectedUsers.Keys.ToArray();
            Balances = new List<BalanceModel>();
            HasNext = MoveNext();
        }

        public async Task<Exception> TryLoadNextAccountInfo()
        {
            if (!HasNext || Current == null)
                return new ValidationException(string.Empty);

            var error = await TryUpdateAccountInfo(Current);
            if (error == null)
            {
                HasNext = MoveNext();
            }

            return error;
        }

        public async Task<Exception> TryUpdateAccountInfo(UserInfo userInfo)
        {
            if (!ConnectedUsers.ContainsKey(userInfo.Id))
                return new ValidationException(string.Empty);

            var response = await TryRunTask<string, AccountInfoResponse>(GetAccountInfo, OnDisposeCts.Token, userInfo.Login);
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
                return response.Error;
            }

            var historyResp = await TryRunTask<string, AccountHistoryResponse[]>(GetAccountHistory, OnDisposeCts.Token, userInfo.Login);
            if (historyResp.IsSuccess)
            {
                ConnectedUsers[userInfo.Id].AccountHistory = historyResp.Result;
                return null;
            }

            return historyResp.Error;
        }

        private Task<OperationResult<AccountHistoryResponse[]>> GetAccountHistory(string login, CancellationToken ct)
        {
            return Api.GetAccountHistory(login, ct);
        }

        public async Task<Exception> TryClaimRewards(BalanceModel balance)
        {
            var claimRewardsModel = new ClaimRewardsModel(balance.UserInfo, balance.RewardSteem.ToString(CultureInfo.InvariantCulture), balance.RewardSp.ToString(CultureInfo.InvariantCulture), balance.RewardSbd.ToString(CultureInfo.InvariantCulture));
            var response = await TryRunTask<ClaimRewardsModel, VoidResponse>(ClaimRewards, CancellationToken.None, claimRewardsModel);
            return response.Error;
        }

        private Task<OperationResult<VoidResponse>> ClaimRewards(ClaimRewardsModel claimRewardsModel, CancellationToken ct)
        {
            return Api.ClaimRewards(claimRewardsModel, ct);
        }

        public async Task<Exception> TryGetCurrencyRates()
        {
            var response = await TryRunTask<CurrencyRate[]>(GetCurrencyRates, OnDisposeCts.Token);
            if (response.IsSuccess)
            {
                CurrencyRates = response.Result;
            }
            return response.Error;
        }

        private Task<OperationResult<CurrencyRate[]>> GetCurrencyRates(CancellationToken ct)
        {
            return Api.GetCurrencyRates(ct);
        }

        public CurrencyRate GetCurrencyRate(CurrencyType currency)
        {
            return CurrencyRates?.First(x =>
                x.Symbol.Equals(currency.ToString(), StringComparison.OrdinalIgnoreCase)) ?? new CurrencyRate
                {
                    Symbol = currency.ToString(),
                    UsdRate = 1
                };
        }

        public bool MoveNext()
        {
            _current += 1;
            return _current < _logins.Length;
        }

        public void Reset()
        {
            _current = -1;
        }

        public UserInfo Current => ConnectedUsers[_logins[_current]];

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }
}
