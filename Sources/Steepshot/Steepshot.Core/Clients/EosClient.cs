using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Core.JsonRpc;
using Ditch.EOS;
using Ditch.EOS.Models;
using Newtonsoft.Json.Linq;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Contracts.Eosio.Actions;
using Steepshot.Core.Models.Contracts.Eosio.Structs;
using Steepshot.Core.Models.Contracts.EosioToken.Actions;
using Steepshot.Core.Models.Contracts.EosioToken.Structs;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Clients
{
    internal class EosClient
    {
        protected readonly object SyncConnection;
        protected readonly ExtendedHttpClient ExtendedHttpClient;

        public volatile bool EnableWrite;

        private readonly OperationManager _operationManager;


        public EosClient(ExtendedHttpClient extendedHttpClient)
        {
            SyncConnection = new object();
            ExtendedHttpClient = extendedHttpClient;
            _operationManager = new OperationManager(extendedHttpClient);
        }

        public async Task<bool> TryReconnectChain(CancellationToken token)
        {
            if (EnableWrite)
                return EnableWrite;

            var lockWasTaken = false;
            try
            {
                Monitor.Enter(SyncConnection, ref lockWasTaken);
                if (!EnableWrite)
                {
                    await AppSettings.ConfigManager.Update(ExtendedHttpClient, KnownChains.Eos, token);

                    var cUrls = AppSettings.ConfigManager.EosNodeConfigs
                        .Where(n => n.IsEnabled)
                        .OrderBy(n => n.Order)
                        .Select(n => n.Url)
                        .ToArray();

                    foreach (var url in cUrls)
                    {
                        if (token.IsCancellationRequested)
                            break;

                        var rez = ExtendedHttpClient.GetAsync(url, token).Result;
                        if (rez.IsSuccessStatusCode)
                        {
                            _operationManager.ChainUrl = url;
                            EnableWrite = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await AppSettings.Logger.Warning(ex);
            }
            finally
            {
                if (lockWasTaken)
                    Monitor.Exit(SyncConnection);
            }
            return EnableWrite;
        }


        private async Task<Models.Common.OperationResult<VoidResponse>> Broadcast(List<byte[]> keys, BaseAction[] ops, CancellationToken ct)
        {
            var resp = await _operationManager.BroadcastActions(ops, keys, ct);

            var result = new Models.Common.OperationResult<VoidResponse>();
            if (resp.IsError)
                result.Exception = new RequestException(resp.Error, resp.RawRequest, resp.RawResponse);
            else
                result.Result = new VoidResponse();

            return result;
        }


        #region Post requests

        public async Task<Models.Common.OperationResult<VoidResponse>> Transfer(TransferModel model, CancellationToken ct)
        {
            var isConnected = await TryReconnectChain(ct);
            if (!isConnected)
                return new Models.Common.OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.ActiveKey);
            if (keys == null)
                return new Models.Common.OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.WrongPrivatePostingKey));

            BaseAction op = null;
            switch (model.CurrencyType)
            {
                case CurrencyType.Vim:
                    {
                        op = new Models.Contracts.Vimtoken.Actions.TransferAction
                        {
                            Account = TransferAction.ContractName,

                            Args = new Transfer
                            {
                                From = model.Login,
                                To = model.Recipient,
                                Quantity = new Asset(model.Value),
                                Memo = model.Memo,
                            },
                            Authorization = new[]
                            {
                            new Ditch.EOS.Models.PermissionLevel
                            {
                                Actor = model.Login,
                                Permission = "active"
                            }
                        }
                        };
                        break;
                    }
                case CurrencyType.Eos:
                    {
                        op = new Models.Contracts.EosioToken.Actions.TransferAction
                        {
                            Account = model.Login,

                            Args = new Transfer
                            {
                                From = model.Login,
                                To = model.Recipient,
                                Quantity = new Asset(model.Value),
                                Memo = model.Memo,
                            },
                            Authorization = new[]
                            {
                            new Ditch.EOS.Models.PermissionLevel
                            {
                                Actor = model.Login,
                                Permission = "active"
                            }
                        }
                        };
                        break;
                    }
                default:
                    throw new NotImplementedException();
            }

            return await Broadcast(keys, new[] { op }, ct);
        }

        public async Task<Models.Common.OperationResult<VoidResponse>> PowerUpOrDown(PowerUpDownModel model, CancellationToken ct)
        {
            var isConnected = await TryReconnectChain(ct);
            if (!isConnected)
                return new Models.Common.OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.ActiveKey);
            if (keys == null)
                return new Models.Common.OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.WrongPrivatePostingKey));

            BaseAction op = null;
            switch (model.CurrencyType)
            {
                case CurrencyType.Vim when model.PowerAction == PowerAction.PowerDown:
                    {
                        op = new Models.Contracts.Vimtoken.Actions.PowerdownAction()
                        {
                            Account = Models.Contracts.Vimtoken.Actions.PowerdownAction.ContractName,

                            //Args = new Models.Contracts.Vimtoken.Structs.Powerdown()
                            //{

                            //},
                            Authorization = new[]
                            {
                            new Ditch.EOS.Models.PermissionLevel
                            {
                                Actor = model.Login,
                                Permission = "active"
                            }
                        }
                        };
                        break;
                    }
                case CurrencyType.Vim when model.PowerAction == PowerAction.PowerUp:
                    {
                        op = new Models.Contracts.Vimtoken.Actions.PowerupAction()
                        {
                            Account = Models.Contracts.Vimtoken.Actions.PowerupAction.ContractName,

                            //Args = new Models.Contracts.Vimtoken.Structs.Powerdown()
                            //{

                            //},
                            Authorization = new[]
                            {
                            new Ditch.EOS.Models.PermissionLevel
                            {
                                Actor = model.Login,
                                Permission = "active"
                            }
                        }
                        };
                        break;
                    }
                default:
                    throw new NotImplementedException();
            }

            return await Broadcast(keys, new[] { op }, ct);
        }

        public async Task<Models.Common.OperationResult<VoidResponse>> ClaimRewards(ClaimRewardsModel model, CancellationToken ct)
        {
            var isConnected = await TryReconnectChain(ct);
            if (!isConnected)
                return new Models.Common.OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.PostingKey);
            if (keys == null)
                return new Models.Common.OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.WrongPrivatePostingKey));

            var op = new ClaimrewardsAction()
            {
                Account = model.Login,

                Args = new Claimrewards
                {
                    Owner = model.Login

                },
                Authorization = new[]
                {
                    new Ditch.EOS.Models.PermissionLevel
                    {
                        Actor = model.Login,
                        Permission = "active"
                    }
                }
            };

            return await Broadcast(keys, new BaseAction[] { op }, ct);
        }

        #endregion Post requests


        #region Get
        
        public async Task<Models.Common.OperationResult<VoidResponse>> ValidatePrivateKey(ValidatePrivateKeyModel model, CancellationToken ct)
        {
            var keys = ToKey(model.PrivateKey);
            if (keys == null)
            {
                switch (model.KeyRoleType)
                {
                    case KeyRoleType.Active:
                        return new Models.Common.OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.WrongPrivateActimeKey));
                    case KeyRoleType.Posting:
                        return new Models.Common.OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.WrongPrivatePostingKey));
                }
            }

            var isConnected = await TryReconnectChain(ct);
            if (!isConnected)
            {
                return new Models.Common.OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));
            }

            var result = new Models.Common.OperationResult<VoidResponse>();

            var args = new GetAccountParams
            {
                AccountName = model.Login
            };
            var resp = await _operationManager.GetAccount(args, CancellationToken.None);
            if (resp.IsError)
            {
                result.Exception = new RequestException(resp.Error, resp.RawRequest, resp.RawResponse);
                return result;
            }

            if (resp.Result == null)
            {
                return new Models.Common.OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.UnexpectedProfileData));
            }

            Ditch.EOS.Models.Authority authority;

            switch (model.KeyRoleType)
            {
                case KeyRoleType.Active:
                    authority = resp.Result.Permissions.FirstOrDefault(p => p.PermName.Equals("active"))?.RequiredAuth;
                    break;
                default:
                    throw new NotImplementedException();
            }

            var isSame = authority != null && KeyHelper.ValidatePrivateKey(keys, authority.Keys.Select(i => i.Key.Data).ToArray());

            if (isSame)
                return new Models.Common.OperationResult<VoidResponse>(new VoidResponse());

            switch (model.KeyRoleType)
            {
                case KeyRoleType.Active:
                    return new Models.Common.OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.WrongPrivateActimeKey));
                default:
                    throw new NotImplementedException();
            }
        }

        public async Task<Models.Common.OperationResult<AccountInfoResponse>> GetAccountInfo(string userName, CancellationToken ct)
        {
            var isConnected = await TryReconnectChain(ct);
            if (!isConnected)
            {
                return new Models.Common.OperationResult<AccountInfoResponse>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));
            }

            var result = new Models.Common.OperationResult<AccountInfoResponse>();

            var args = new GetAccountParams
            {
                AccountName = userName
            };
            var resp = await _operationManager.GetAccount(args, CancellationToken.None);
            if (resp.IsError)
            {
                result.Exception = new RequestException(resp.Error, resp.RawRequest, resp.RawResponse);
                return result;
            }

            var acc = resp.Result;
            result.Result = new AccountInfoResponse
            {
                Chains = KnownChains.Steem,
                PublicActiveKeys = acc.Permissions.FirstOrDefault(p => p.PermName.Equals("active"))?.RequiredAuth.Keys.Select(i => i.Key.Data).ToArray(),
                Balances = new List<BalanceModel>()
                {
                    ((JObject)acc.TotalResources).ToObject<EosBalanceModel>(),
                }
            };

            var vimAccArgs = new GetTableRowsParams
            {
                Scope = userName,
                Code = VimBalanceModel.Code,
                Table = VimBalanceModel.Table,
                Json = true,
            };

            var vimAcc = await _operationManager.GetTableRows(vimAccArgs, ct);
            if (vimAcc.IsError)
            {
                result.Exception = new RequestException(resp.Error, resp.RawRequest, resp.RawResponse);
                return result;
            }

            if (vimAcc.Result.Rows.Length == 1)
            {
                var blns = ((JObject)vimAcc.Result.Rows[0]).ToObject<VimBalanceModel>();
                result.Result.Balances.Add(blns);
            }

            return result;
        }

        public Task<Models.Common.OperationResult<AccountHistoryResponse[]>> GetAccountHistory(string userName, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        #endregion

        protected List<byte[]> ToKeyArr(string postingKey)
        {
            var key = ToKey(postingKey);
            if (key == null)
                return null;

            return new List<byte[]> { key };
        }

        protected byte[] ToKey(string postingKey)
        {
            try
            {
                var key = Ditch.Core.Base58.DecodePrivateWif(postingKey);
                if (key == null || key.Length != 32)
                    return null;
                return key;
            }
            catch (System.Exception ex)
            {
                AppSettings.Logger.Warning(ex);
            }
            return null;
        }
    }
}
