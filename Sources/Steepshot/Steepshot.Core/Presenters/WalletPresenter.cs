using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Authorization;
using Steepshot.Core.HttpClient;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public class WalletPresenter : PreSignInPresenter
    {
        public IEnumerator<UserInfo> ConnectedUsers { get; }
        public List<BalanceModel> Balances { get; }
        public bool HasNext { get; private set; }

        public WalletPresenter()
        {
            ConnectedUsers = AppSettings.DataProvider.Select().GetEnumerator();
            Balances = new List<BalanceModel>();
            HasNext = ConnectedUsers.MoveNext();
        }

        public async Task<Exception> TryLoadNextAccountInfo()
        {
            if (!HasNext || ConnectedUsers.Current == null)
                return null;

            var response = await TryRunTask<string, AccountInfoResponse>(GetAccountInfo, OnDisposeCts.Token, ConnectedUsers.Current.Login);
            var historyResp = await TryRunTask<string, AccountHistoryResponse[]>(GetAccountHistory, OnDisposeCts.Token, ConnectedUsers.Current.Login);
            if (response.IsSuccess)
            {
                ConnectedUsers.Current.AccountInfo = response.Result;
                lock (Balances)
                {
                    Balances.AddRange(response.Result.Balances);
                }
            }
            else
            {
                return response.Error;
            }

            if (historyResp.IsSuccess)
            {
                ConnectedUsers.Current.AccountHistory = historyResp.Result;
                HasNext = ConnectedUsers.MoveNext();
                return null;
            }

            return historyResp.Error;
        }

        private Task<OperationResult<AccountHistoryResponse[]>> GetAccountHistory(string login, CancellationToken ct)
        {
            return Api.GetAccountHistory(login, ct);
        }
    }
}
