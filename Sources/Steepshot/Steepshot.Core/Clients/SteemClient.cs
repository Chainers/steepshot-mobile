using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Core;
using Ditch.Core.JsonRpc;
using Ditch.Steem;
using Ditch.Steem.Models;
using Ditch.Steem.Operations;
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
    internal class SteemClient : BaseDitchClient
    {
        private readonly OperationManager _operationManager;

        private double? _vestsExchangeRatio;

        public override bool IsConnected => _operationManager.IsConnected;

        public override KnownChains Chain => KnownChains.Steem;

        public SteemClient(ExtendedHttpClient extendedHttpClient) : base(extendedHttpClient)
        {
            var httpManager = new HttpManager(extendedHttpClient);
            _operationManager = new OperationManager(httpManager);
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
                    await AppSettings.ConfigManager.Update(ExtendedHttpClient, KnownChains.Steem, token);

                    var cUrls = AppSettings.ConfigManager.SteemNodeConfigs
                        .Where(n => n.IsEnabled)
                        .OrderBy(n => n.Order)
                        .Select(n => n.Url)
                        .ToArray();
                    foreach (var url in cUrls)
                    {
                        if (token.IsCancellationRequested)
                            break;

                        if (_operationManager.ConnectTo(url, token))
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

        private async Task<OperationResult<VoidResponse>> Broadcast(List<byte[]> keys, BaseOperation[] ops, CancellationToken ct)
        {
            var resp = await _operationManager.BroadcastOperationsSynchronousLikeSteemit(keys, ops, ct);

            var result = new OperationResult<VoidResponse>();
            if (resp.IsError)
                result.Exception = new RequestException(resp);
            else
                result.Result = new VoidResponse();
            return result;
        }

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
                weigth = (short)(AppSettings.User.VotePower * 100);
            if (model.Type == VoteType.Flag)
                weigth = -10000;

            var op = new VoteOperation(model.Login, model.Author, model.Permlink, weigth);
            return await Broadcast(keys, new BaseOperation[] { op }, ct);
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
                ? new FollowOperation(model.Login, model.Username, Ditch.Steem.Models.FollowType.Blog, model.Login)
                : new UnfollowOperation(model.Login, model.Username, model.Login);

            return await Broadcast(keys, new BaseOperation[] { op }, ct);
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
                    .Select(i => new Ditch.Steem.Operations.Beneficiary(i.Account, i.Weight))
                    .ToArray();
                ops = new BaseOperation[]
                {
                        op,
                        new BeneficiariesOperation(model.Login, model.Permlink,new Asset(1000000000, Config.SteemAssetNumSbd) ,beneficiaries)
                };
            }
            else
            {
                ops = new BaseOperation[] { op };
            }

            return await Broadcast(keys, ops, ct);
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

            return await Broadcast(keys, new BaseOperation[] { op }, ct);
        }

        public override async Task<OperationResult<VoidResponse>> UpdateUserProfile(UpdateUserProfileModel model, CancellationToken ct)
        {
            var isConnected = await TryReconnectChain(ct);
            if (!isConnected)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.ActiveKey);
            if (keys == null)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.WrongPrivateActimeKey));

            var args = new FindAccountsArgs
            {
                Accounts = new[] { model.Login }
            };
            var resp = await _operationManager.FindAccounts(args, CancellationToken.None);
            var result = new OperationResult<VoidResponse>();
            if (resp.IsError)
            {
                result.Exception = new RequestException(resp);
                return result;
            }

            var profile = resp.Result.Accounts.Length == 1 ? resp.Result.Accounts[0] : null;
            if (profile == null)
            {
                result.Exception = new ValidationException(LocalizationKeys.UnexpectedProfileData);
                return result;
            }

            var editedMeta = UpdateProfileJson(profile.JsonMetadata, model);

            var op = new AccountUpdateOperation(model.Login, profile.MemoKey, editedMeta);

            return await Broadcast(keys, new BaseOperation[] { op }, ct);
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

            var asset = new Asset();
            switch (model.CurrencyType)
            {
                case CurrencyType.Steem:
                    {
                        asset.FromOldFormat($"{model.Value} {Config.Steem}");
                        break;
                    }
                case CurrencyType.Sbd:
                    {
                        asset.FromOldFormat($"{model.Value} {Config.Sbd}");
                        break;
                    }
                default:
                    {
                        result.Exception = new ValidationException(LocalizationKeys.UnsupportedCurrency, model.CurrencyType.ToString());
                        return result;
                    }
            }

            var op = new TransferOperation(model.Login, model.Recipient, asset, model.Memo);

            return await Broadcast(keys, new BaseOperation[] { op }, ct);
        }

        public override async Task<OperationResult<VoidResponse>> PowerUpOrDown(PowerUpDownModel model, CancellationToken ct)
        {
            var isConnected = await TryReconnectChain(ct);
            if (!isConnected)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.ActiveKey);
            if (keys == null)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.WrongPrivateActimeKey));

            var asset = new Asset();

            BaseOperation op;
            if (model.PowerAction == PowerAction.PowerUp)
            {
                asset.FromOldFormat($"{model.Value} {Config.Steem}");
                op = new TransferToVestingOperation(model.From, model.To, asset);
            }
            else
            {
                var vestsExchangeRatio = await GetVestsExchangeRatio(ct);
                if (!vestsExchangeRatio.IsSuccess)
                    return new OperationResult<VoidResponse>(vestsExchangeRatio.Exception);

                asset.FromOldFormat($"{(model.Value / vestsExchangeRatio.Result):F6} {Config.Vests}");
                op = new WithdrawVestingOperation(model.Login, asset);
            }

            return await Broadcast(keys, new[] { op }, ct);
        }

        public override async Task<OperationResult<VoidResponse>> ClaimRewards(ClaimRewardsModel model, CancellationToken ct)
        {
            var isConnected = await TryReconnectChain(ct);
            if (!isConnected)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.PostingKey);
            if (keys == null)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.WrongPrivatePostingKey));

            var assetSteem = new Asset();
            assetSteem.FromOldFormat($"{model.RewardSteem} {Config.Steem}");

            var assetSp = new Asset();
            assetSp.FromOldFormat($"{(model.RewardSp / _vestsExchangeRatio.Value).ToString(CultureInfo.InvariantCulture) } {Config.Vests}");

            var assetSbd = new Asset();
            assetSbd.FromOldFormat($"{model.RewardSbd} {Config.Sbd}");

            var op = new ClaimRewardBalanceOperation(model.Login, assetSteem, assetSbd, assetSp);

            return await Broadcast(keys, new BaseOperation[] { op }, ct);
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

            var op = new FollowOperation(model.Login, "steepshot", Ditch.Steem.Models.FollowType.Blog, model.Login);
            var properties = new DynamicGlobalPropertyObject
            {
                HeadBlockId = "0000000000000000000000000000000000000000",
                Time = DateTime.Now,
                HeadBlockNumber = 0
            };
            var tr = await _operationManager.CreateTransaction(properties, keys, op, ct);

            var conv = JsonConvert.SerializeObject(tr, _operationManager.CondenserJsonSerializerSettings);
            return new OperationResult<string> { Result = conv };
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


            var args = new FindAccountsArgs
            {
                Accounts = new[] { model.Login }
            };
            var resp = await _operationManager.FindAccounts(args, CancellationToken.None);
            if (resp.IsError)
            {
                result.Exception = new RequestException(resp);
                return result;
            }

            if (resp.Result.Accounts.Length != 1 || resp.Result.Accounts[0] == null)
            {
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.UnexpectedProfileData));
            }

            Authority authority;

            switch (model.KeyRoleType)
            {
                case KeyRoleType.Active:
                    authority = resp.Result.Accounts[0].Active;
                    break;
                case KeyRoleType.Posting:
                    authority = resp.Result.Accounts[0].Posting;
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
            {
                return new OperationResult<AccountInfoResponse>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));
            }

            var result = new OperationResult<AccountInfoResponse>();

            var args = new FindAccountsArgs
            {
                Accounts = new[] { userName }
            };
            var resp = await _operationManager.FindAccounts(args, CancellationToken.None);
            if (resp.IsError)
            {
                result.Exception = new RequestException(resp);
                return result;
            }

            if (resp.Result.Accounts.Length != 1 || resp.Result.Accounts[0] == null)
            {
                return new OperationResult<AccountInfoResponse>(new ValidationException(LocalizationKeys.UnexpectedProfileData));
            }

            var acc = resp.Result.Accounts[0];

            var vestsExchangeRatio = await GetVestsExchangeRatio(ct);
            if (!vestsExchangeRatio.IsSuccess)
                return new OperationResult<AccountInfoResponse>(vestsExchangeRatio.Exception);

            var effectiveSp = (acc.VestingShares.ToDouble() + acc.ReceivedVestingShares.ToDouble() - acc.DelegatedVestingShares.ToDouble()) * vestsExchangeRatio.Result;

            result.Result = new AccountInfoResponse
            {
                Chains = KnownChains.Steem,
                PublicPostingKeys = acc.Posting.KeyAuths.Select(i => i.Key.Data).ToArray(),
                PublicActiveKeys = acc.Active.KeyAuths.Select(i => i.Key.Data).ToArray(),
                Metadata = JsonConvert.DeserializeObject<AccountMetadata>(acc.JsonMetadata),
                Balances = new List<BalanceModel>
                {
                    new BalanceModel(acc.Balance.ToDouble(), 3, CurrencyType.Steem)
                    {
                        EffectiveSp = effectiveSp,
                        RewardSteem = acc.RewardSteemBalance.ToDouble(),
                        RewardSp = acc.RewardVestingBalance.ToDouble() * vestsExchangeRatio.Result,
                        RewardSbd = acc.RewardSbdBalance.ToDouble()
                    },
                    new BalanceModel(acc.SbdBalance.ToDouble(), 3, CurrencyType.Sbd)
                    {
                        EffectiveSp = effectiveSp,
                        RewardSteem = acc.RewardSteemBalance.ToDouble(),
                        RewardSp = acc.RewardVestingBalance.ToDouble() * vestsExchangeRatio.Result,
                        RewardSbd = acc.RewardSbdBalance.ToDouble()
                    }
                }
            };

            return result;
        }

        private readonly string[] _accountHistoryFilter = {
            ClaimRewardBalanceOperation.OperationName,
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

            var args = new GetAccountHistoryArgs
            {
                Account = userName,
                Start = ulong.MaxValue,
                Limit = 1000
            };
            var resp = await _operationManager.CondenserGetAccountHistory(args, CancellationToken.None);
            if (resp.IsError)
            {
                result.Exception = new RequestException(resp);
                return result;
            }

            var vestsExchangeRatio = await GetVestsExchangeRatio(ct);
            if (!vestsExchangeRatio.IsSuccess)
                return new OperationResult<AccountHistoryResponse[]>(vestsExchangeRatio.Exception);

            result.Result = resp.Result.History.Where(Filter).Select(pair => Transform(pair, vestsExchangeRatio.Result)).OrderByDescending(x => x.DateTime).ToArray();
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
                            Amount = typed.Amount.ToOldFormatString(),
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
                            Amount = typed.Amount.ToOldFormatString()
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
                            Amount = $"{(typed.VestingShares.ToDouble() * vestsExchangeRatio).ToBalanceVaueString()} {CurrencyType.Steem.ToString().ToUpper()}"
                        };
                    }
                case ClaimRewardBalanceOperation.OperationName:
                    {
                        var typed = (ClaimRewardBalanceOperation)baseOperation;
                        return new AccountHistoryResponse
                        {
                            DateTime = arg.Value.Timestamp,
                            Type = AccountHistoryResponse.OperationType.ClaimReward,
                            From = typed.Account,
                            To = typed.Account,
                            RewardSteem = typed.RewardSteem.ToDoubleString(),
                            RewardSp = (typed.RewardVests.ToDouble() * vestsExchangeRatio).ToBalanceVaueString(),
                            RewardSbd = typed.RewardSbd.ToDoubleString()
                        };
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        #endregion
    }
}
