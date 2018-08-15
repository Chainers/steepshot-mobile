using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Core;
using Ditch.Core.JsonRpc;
using Ditch.Golos;
using Ditch.Golos.Models;
using Ditch.Golos.Operations;
using Newtonsoft.Json;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Extensions;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Clients
{
    internal class GolosClient : BaseDitchClient
    {
        private readonly OperationManager _operationManager;
        private double? _vestsExchangeRatio;

        public override bool IsConnected => _operationManager.IsConnected;

        public override KnownChains Chain => KnownChains.Golos;


        public GolosClient(ExtendedHttpClient extendedHttpClient) : base(extendedHttpClient)
        {
            var webSocketManager = new WebSocketManager();
            _operationManager = new OperationManager(webSocketManager);
        }

        public override async Task<bool> TryReconnectChain(CancellationToken token)
        {
            if (EnableWrite)
                return EnableWrite;

            var lockWasTaken = false;
            try
            {
                Monitor.Enter(SyncConnection, ref lockWasTaken);
                if (!EnableWrite)
                {
                    await AppSettings.ConfigManager.Update(ExtendedHttpClient, KnownChains.Golos, token);

                    var cUrls = AppSettings.ConfigManager.GolosNodeConfigs
                        .Where(n => n.IsEnabled)
                        .OrderBy(n => n.Order)
                        .Select(n => n.Url)
                        .ToArray();
                    foreach (var url in cUrls)
                    {
                        if (token.IsCancellationRequested)
                            break;

                        var isConnected = await _operationManager.ConnectTo(url, token);
                        if (isConnected)
                        {
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

        #region Post requests

        public override async Task<OperationResult<VoidResponse>> Vote(VoteModel model, CancellationToken ct)
        {
            var isConnected = await TryReconnectChain(ct);
            if (!isConnected)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.PostingKey);
            if (keys == null)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.WrongPrivatePostingKey));

            short weigth = 0;
            if (model.Type == VoteType.Up)
                weigth = 10000;
            if (model.Type == VoteType.Flag)
                weigth = -10000;

            var op = new VoteOperation(model.Login, model.Author, model.Permlink, weigth);
            var resp = await _operationManager.BroadcastOperationsSynchronous(keys, ct, op);

            var result = new OperationResult<VoidResponse>();
            if (resp.IsError)
                result.Exception = new RequestException(resp);
            else
                result.Result = new VoidResponse();

            return result;
        }

        public override async Task<OperationResult<VoidResponse>> Follow(FollowModel model, CancellationToken ct)
        {
            var isConnected = await TryReconnectChain(ct);
            if (!isConnected)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.PostingKey);
            if (keys == null)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.WrongPrivatePostingKey));

            var op = model.Type == Models.Enums.FollowType.Follow
                ? new FollowOperation(model.Login, model.Username, Ditch.Golos.Models.FollowType.Blog, model.Login)
                : new UnfollowOperation(model.Login, model.Username, model.Login);
            var resp = await _operationManager.BroadcastOperationsSynchronous(keys, ct, op);

            var result = new OperationResult<VoidResponse>();

            if (resp.IsError)
                result.Exception = new RequestException(resp);
            else
                result.Result = new VoidResponse();

            return result;
        }

        public override async Task<OperationResult<VoidResponse>> CreateOrEdit(CommentModel model, CancellationToken ct)
        {
            var isConnected = await TryReconnectChain(ct);
            if (!isConnected)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.PostingKey);
            if (keys == null)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.WrongPrivatePostingKey));

            var op = new CommentOperation(model.ParentAuthor, model.ParentPermlink, model.Author, model.Permlink, model.Title, model.Body, model.JsonMetadata);

            BaseOperation[] ops;
            if (model.Beneficiaries != null && model.Beneficiaries.Any())
            {
                var beneficiaries = model.Beneficiaries
                    .Select(i => new Ditch.Golos.Operations.Beneficiary(i.Account, i.Weight))
                    .ToArray();
                ops = new BaseOperation[]
                {
                        op,
                        new BeneficiariesOperation(model.Login, model.Permlink, "GBG", beneficiaries)
                };
            }
            else
            {
                ops = new BaseOperation[] { op };
            }

            var resp = await _operationManager.BroadcastOperationsSynchronous(keys, ct, ops);

            var result = new OperationResult<VoidResponse>();
            if (resp.IsError)
                result.Exception = new RequestException(resp);
            else
                result.Result = new VoidResponse();

            return result;
        }

        public override async Task<OperationResult<VoidResponse>> Delete(DeleteModel model, CancellationToken ct)
        {
            var isConnected = await TryReconnectChain(ct);
            if (!isConnected)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.PostingKey);
            if (keys == null)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.WrongPrivatePostingKey));

            var op = new DeleteCommentOperation(model.Author, model.Permlink);
            var resp = await _operationManager.BroadcastOperationsSynchronous(keys, ct, op);

            var result = new OperationResult<VoidResponse>();
            if (resp.IsError)
                result.Exception = new RequestException(resp);
            else
                result.Result = new VoidResponse();
            return result;
        }

        public override async Task<OperationResult<VoidResponse>> UpdateUserProfile(UpdateUserProfileModel model, CancellationToken ct)
        {
            var isConnected = await TryReconnectChain(ct);
            if (!isConnected)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.ActiveKey);
            if (keys == null)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.WrongPrivateActimeKey));

            var resp = await _operationManager.LookupAccountNames(new[] { model.Login }, CancellationToken.None);
            var result = new OperationResult<VoidResponse>();
            if (resp.IsError)
            {
                result.Exception = new RequestException(resp);
                return result;
            }

            var profile = resp.Result.Length == 1 ? resp.Result[0] : null;
            if (profile == null)
            {
                result.Exception = new ValidationException(LocalizationKeys.UnexpectedProfileData);
                return result;
            }

            var editedMeta = UpdateProfileJson(profile.JsonMetadata, model);

            var op = new AccountUpdateOperation(model.Login, profile.MemoKey, editedMeta);
            var resp2 = await _operationManager.BroadcastOperationsSynchronous(keys, ct, op);
            if (resp2.IsError)
                result.Exception = new RequestException(resp2);
            else
                result.Result = new VoidResponse();
            return result;
        }

        public override async Task<OperationResult<VoidResponse>> Transfer(TransferModel model, CancellationToken ct)
        {
            var isConnected = await TryReconnectChain(ct);
            if (!isConnected)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.ActiveKey);
            if (keys == null)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.WrongPrivateActimeKey));

            var result = new OperationResult<VoidResponse>();

            Asset asset;
            switch (model.CurrencyType)
            {
                case CurrencyType.Golos:
                    {
                        asset = new Asset($"{model.Value} GOLOS");
                        break;
                    }
                case CurrencyType.Gbg:
                    {
                        asset = new Asset($"{model.Value} GBG");
                        break;
                    }
                default:
                    {
                        result.Exception = new ValidationException(LocalizationKeys.UnsupportedCurrency, model.CurrencyType.ToString());
                        return result;
                    }
            }

            var op = new TransferOperation(model.Login, model.Recipient, asset, model.Memo);
            var resp = await _operationManager.BroadcastOperationsSynchronous(keys, ct, op);
            if (resp.IsError)
                result.Exception = new RequestException(resp);
            else
                result.Result = new VoidResponse();
            return result;
        }

        public override async Task<OperationResult<VoidResponse>> PowerUpOrDown(PowerUpDownModel model, CancellationToken ct)
        {
            var isConnected = await TryReconnectChain(ct);
            if (!isConnected)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.ActiveKey);
            if (keys == null)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.WrongPrivateActimeKey));

            var result = new OperationResult<VoidResponse>();

            BaseOperation op;

            Asset asset;
            if (model.PowerAction == PowerAction.PowerUp)
            {
                asset = new Asset($"{model.Value.ToString(CultureInfo.InvariantCulture)} GOLOS");
                op = new TransferToVestingOperation(model.From, model.To, asset);
            }
            else
            {
                var vestsExchangeRatio = await GetVestsExchangeRatio(ct);
                if (!vestsExchangeRatio.IsSuccess)
                    return new OperationResult<VoidResponse>(vestsExchangeRatio.Exception);

                asset = new Asset($"{(model.Value / vestsExchangeRatio.Result).ToString("F6", CultureInfo.InvariantCulture)} GESTS");
                op = new WithdrawVestingOperation(model.From, asset);
            }

            var resp = await _operationManager.BroadcastOperationsSynchronous(keys, ct, op);
            if (resp.IsError)
                result.Exception = new RequestException(resp);
            else
                result.Result = new VoidResponse();
            return result;
        }

        public override Task<OperationResult<VoidResponse>> ClaimRewards(ClaimRewardsModel model, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        #endregion Post requests

        #region Get

        public override async Task<OperationResult<string>> GetVerifyTransaction(AuthorizedPostingModel model, CancellationToken ct)
        {
            var isConnected = await TryReconnectChain(ct);
            if (!isConnected)
                return new OperationResult<string>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.PostingKey);
            if (keys == null)
                return new OperationResult<string>(new ValidationException(LocalizationKeys.WrongPrivatePostingKey));

            var op = new FollowOperation(model.Login, "steepshot", Ditch.Golos.Models.FollowType.Blog, model.Login);
            var properties = new DynamicGlobalPropertyObject
            {
                HeadBlockId = "0000000000000000000000000000000000000000",
                Time = DateTime.Now,
                HeadBlockNumber = 0
            };
            var tr = await _operationManager.CreateTransaction(properties, keys, op, ct);
            return new OperationResult<string> { Result = JsonConvert.SerializeObject(tr) };
        }

        public override async Task<OperationResult<VoidResponse>> ValidatePrivateKey(ValidatePrivateKeyModel model, CancellationToken ct)
        {
            var keys = ToKey(model.PrivateKey);
            if (keys == null)
            {
                switch (model.KeyRoleType)
                {
                    case KeyRoleType.Active:
                        return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.WrongPrivateActimeKey));
                    case KeyRoleType.Posting:
                        return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.WrongPrivatePostingKey));
                }
            }

            var isConnected = await TryReconnectChain(ct);
            if (!isConnected)
            {
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));
            }

            var result = new OperationResult<VoidResponse>();

            var resp = await _operationManager.LookupAccountNames(new[] { model.Login }, CancellationToken.None);
            if (resp.IsError)
            {
                result.Exception = new RequestException(resp);
                return result;
            }

            if (resp.Result.Length != 1 || resp.Result[0] == null)
            {
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.UnexpectedProfileData));
            }

            Authority authority;

            switch (model.KeyRoleType)
            {
                case KeyRoleType.Active:
                    authority = resp.Result[0].Active;
                    break;
                case KeyRoleType.Posting:
                    authority = resp.Result[0].Posting;
                    break;
                default:
                    throw new NotImplementedException();
            }

            var isSame = KeyHelper.ValidatePrivateKey(keys, authority.KeyAuths.Select(i => i.Key.Data).ToArray());

            if (isSame)
                return new OperationResult<VoidResponse>(new VoidResponse());

            switch (model.KeyRoleType)
            {
                case KeyRoleType.Active:
                    return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.WrongPrivateActimeKey));
                default:
                    return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.WrongPrivatePostingKey));
            }
        }

        public override async Task<OperationResult<AccountInfoResponse>> GetAccountInfo(string userName, CancellationToken ct)
        {
            var isConnected = await TryReconnectChain(ct);
            if (!isConnected)
                return new OperationResult<AccountInfoResponse>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));

            var result = new OperationResult<AccountInfoResponse>();

            var resp = await _operationManager.LookupAccountNames(new[] { userName }, CancellationToken.None);
            if (resp.IsError)
            {
                result.Exception = new RequestException(resp);
                return result;
            }

            if (resp.Result.Length != 1 || resp.Result[0] == null)
                return new OperationResult<AccountInfoResponse>(new ValidationException(LocalizationKeys.UnexpectedProfileData));

            var acc = resp.Result[0];

            var vestsExchangeRatio = await GetVestsExchangeRatio(ct);
            if (!vestsExchangeRatio.IsSuccess)
                return new OperationResult<AccountInfoResponse>(vestsExchangeRatio.Exception);

            var effectiveSp = (acc.VestingShares.ToDouble() + acc.ReceivedVestingShares.ToDouble() - acc.DelegatedVestingShares.ToDouble()) * vestsExchangeRatio.Result;

            result.Result = new AccountInfoResponse
            {
                Chains = KnownChains.Golos,
                PublicPostingKeys = acc.Posting.KeyAuths.Select(i => i.Key.Data).ToArray(),
                PublicActiveKeys = acc.Active.KeyAuths.Select(i => i.Key.Data).ToArray(),
                Metadata = JsonConvert.DeserializeObject<AccountMetadata>(acc.JsonMetadata),
                Balances = new List<BalanceModel>
                {
                    new BalanceModel(acc.Balance.ToDouble(), 3, CurrencyType.Golos)
                    {
                        EffectiveSp = effectiveSp
                    },
                    new BalanceModel(acc.SbdBalance.ToDouble(), 3,  CurrencyType.Gbg)
                    {
                        EffectiveSp =  effectiveSp
                    }
                }
            };

            return result;
        }

        private async Task<OperationResult<double>> GetVestsExchangeRatio(CancellationToken token)
        {
            if (_vestsExchangeRatio.HasValue)
                return new OperationResult<double>(_vestsExchangeRatio.Value);

            var properties = await _operationManager.GetDynamicGlobalProperties(token);
            if (properties.IsError)
                return new OperationResult<double>(properties.Exception);

            var totalVestingShares = properties.Result.TotalVestingShares.ToDouble();
            var totalVestingFund = properties.Result.TotalVestingFundSteem.ToDouble();
            _vestsExchangeRatio = totalVestingFund / totalVestingShares;
            return new OperationResult<double>(_vestsExchangeRatio.Value);
        }


        private readonly string[] _accountHistoryFilter = {
            TransferOperation.OperationName,
            TransferToVestingOperation.OperationName,
            WithdrawVestingOperation.OperationName
        };

        public override async Task<OperationResult<AccountHistoryResponse[]>> GetAccountHistory(string userName, CancellationToken ct)
        {
            var isConnected = await TryReconnectChain(ct);
            if (!isConnected)
                return new OperationResult<AccountHistoryResponse[]>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));

            var result = new OperationResult<AccountHistoryResponse[]>();

            var resp = await _operationManager.GetAccountHistory(userName, ulong.MaxValue, 1000, CancellationToken.None);
            if (resp.IsError)
            {
                result.Exception = new RequestException(resp);
                return result;
            }

            var vestsExchangeRatio = await GetVestsExchangeRatio(ct);
            if (!vestsExchangeRatio.IsSuccess)
                return new OperationResult<AccountHistoryResponse[]>(vestsExchangeRatio.Exception);

            result.Result = resp.Result.Where(Filter).Select(pair => Transform(pair, vestsExchangeRatio.Result)).OrderByDescending(x => x.DateTime).ToArray();
            return result;
        }

        private bool Filter(KeyValuePair<uint, AppliedOperation> arg)
        {
            BaseOperation baseOperation = arg.Value.Op;
            return _accountHistoryFilter.Contains(baseOperation.TypeName);
        }

        private AccountHistoryResponse Transform(KeyValuePair<uint, AppliedOperation> arg, double vestsExchangeRatio)
        {
            BaseOperation baseOperation = arg.Value.Op;
            switch (baseOperation.TypeName)
            {
                case TransferOperation.OperationName:
                    {
                        var typed = (TransferOperation)baseOperation;
                        return new AccountHistoryResponse
                        {
                            DateTime = arg.Value.Timestamp,
                            Type = AccountHistoryResponse.OperationType.Transfer,
                            From = typed.From,
                            To = typed.To,
                            Amount = $"{typed.Amount.ToDoubleString()} {typed.Amount.Currency}",
                            Memo = typed.Memo
                        };
                    }
                case TransferToVestingOperation.OperationName:
                    {
                        var typed = (TransferToVestingOperation)baseOperation;
                        return new AccountHistoryResponse
                        {
                            DateTime = arg.Value.Timestamp,
                            Type = AccountHistoryResponse.OperationType.PowerUp,
                            From = typed.From,
                            To = typed.To,
                            Amount = $"{typed.Amount.ToDoubleString()} {typed.Amount.Currency}"
                        };
                    }
                case WithdrawVestingOperation.OperationName:
                    {
                        var typed = (WithdrawVestingOperation)baseOperation;
                        return new AccountHistoryResponse
                        {
                            DateTime = arg.Value.Timestamp,
                            Type = AccountHistoryResponse.OperationType.PowerDown,
                            From = typed.Account,
                            To = typed.Account,
                            Amount = $"{(typed.VestingShares.ToDouble() * vestsExchangeRatio).ToBalanceValueString()} {CurrencyType.Golos.ToString().ToUpper()}"
                        };
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        #endregion
    }
}