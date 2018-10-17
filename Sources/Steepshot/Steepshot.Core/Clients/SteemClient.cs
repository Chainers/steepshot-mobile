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
using Steepshot.Core.Interfaces;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Clients
{
    public class SteemClient : BaseDitchClient
    {
        private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly OperationManager _operationManager;
        private readonly ConfigManager _configManager;

        private double? _vestsExchangeRatio;

        public override bool IsConnected => _operationManager.IsConnected;

        public override KnownChains Chain => KnownChains.Steem;

        public SteemClient(ExtendedHttpClient extendedHttpClient, ILogService logService, ConfigManager configManager)
            : base(extendedHttpClient, logService)
        {
            _configManager = configManager;
            var httpManager = new HttpManager(extendedHttpClient);
            _operationManager = new OperationManager(httpManager);
        }

        public override async Task<bool> TryReconnectChainAsync(CancellationToken token)
        {
            if (EnableWrite)
                return EnableWrite;

            try
            {
                await SemaphoreSlim.WaitAsync(token);

                if (EnableWrite || token.IsCancellationRequested)
                    return EnableWrite;

                await _configManager.UpdateAsync(ExtendedHttpClient, KnownChains.Steem, token)
                     .ConfigureAwait(false);

                var cUrls = _configManager.SteemNodeConfigs
                    .Where(n => n.IsEnabled)
                    .OrderBy(n => n.Order)
                    .Select(n => n.Url)
                    .ToArray();
                foreach (var url in cUrls)
                {
                    if (token.IsCancellationRequested)
                        break;

                    var isConnected = await _operationManager.ConnectToAsync(url, token)
                        .ConfigureAwait(false);
                    if (isConnected)
                    {
                        EnableWrite = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogService.WarningAsync(ex).Wait(token);
            }
            finally
            {
                SemaphoreSlim.Release();
            }

            return EnableWrite;
        }

        #region Post requests

        private async Task<OperationResult<VoidResponse>> BroadcastAsync(List<byte[]> keys, BaseOperation[] ops, CancellationToken ct)
        {
            var resp = await _operationManager.BroadcastOperationsSynchronousLikeSteemitAsync(keys, ops, ct).ConfigureAwait(false);

            var result = new OperationResult<VoidResponse>();
            if (resp.IsError)
                result.Exception = new RequestException(resp);
            else
                result.Result = new VoidResponse();
            return result;
        }

        public override async Task<OperationResult<VoidResponse>> VoteAsync(VoteModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<VoidResponse>(results);

            var isConnected = await TryReconnectChainAsync(ct).ConfigureAwait(false);
            if (!isConnected)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.PostingKey);
            if (keys == null)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.WrongPrivatePostingKey));

            short weigth = 0;
            if (model.Type == VoteType.Up)
                weigth = (short)(model.VotePower * 100);
            if (model.Type == VoteType.Flag)
                weigth = -10000;

            var op = new VoteOperation(model.Login, model.Author, model.Permlink, weigth);
            return await BroadcastAsync(keys, new BaseOperation[] { op }, ct).ConfigureAwait(false);
        }

        public override async Task<OperationResult<VoidResponse>> FollowAsync(FollowModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<VoidResponse>(results);

            var isConnected = await TryReconnectChainAsync(ct).ConfigureAwait(false);
            if (!isConnected)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.PostingKey);
            if (keys == null)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.WrongPrivatePostingKey));

            var op = model.Type == Models.Enums.FollowType.Follow
                ? new FollowOperation(model.Login, model.Username, Ditch.Steem.Models.FollowType.Blog, model.Login)
                : new UnfollowOperation(model.Login, model.Username, model.Login);

            return await BroadcastAsync(keys, new BaseOperation[] { op }, ct).ConfigureAwait(false);
        }

        public override async Task<OperationResult<VoidResponse>> CreateOrEditAsync(CommentModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<VoidResponse>(results);

            var isConnected = await TryReconnectChainAsync(ct).ConfigureAwait(false);
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

            return await BroadcastAsync(keys, ops, ct).ConfigureAwait(false);
        }

        public override async Task<OperationResult<VoidResponse>> DeleteAsync(DeleteModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<VoidResponse>(results);

            var isConnected = await TryReconnectChainAsync(ct).ConfigureAwait(false);
            if (!isConnected)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.PostingKey);
            if (keys == null)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.WrongPrivatePostingKey));

            var op = new DeleteCommentOperation(model.Author, model.Permlink);

            return await BroadcastAsync(keys, new BaseOperation[] { op }, ct).ConfigureAwait(false);
        }

        public override async Task<OperationResult<VoidResponse>> UpdateUserProfileAsync(UpdateUserProfileModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<VoidResponse>(results);

            var isConnected = await TryReconnectChainAsync(ct).ConfigureAwait(false);
            if (!isConnected)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.ActiveKey);
            if (keys == null)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.WrongPrivateActimeKey));

            var args = new FindAccountsArgs
            {
                Accounts = new[] { model.Login }
            };
            var resp = await _operationManager.FindAccountsAsync(args, ct).ConfigureAwait(false);
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

            return await BroadcastAsync(keys, new BaseOperation[] { op }, ct).ConfigureAwait(false);
        }

        public override async Task<OperationResult<VoidResponse>> TransferAsync(TransferModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<VoidResponse>(results);

            var isConnected = await TryReconnectChainAsync(ct).ConfigureAwait(false);
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

            return await BroadcastAsync(keys, new BaseOperation[] { op }, ct).ConfigureAwait(false);
        }

        public override async Task<OperationResult<VoidResponse>> PowerUpOrDownAsync(PowerUpDownModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<VoidResponse>(results);

            var isConnected = await TryReconnectChainAsync(ct).ConfigureAwait(false);
            if (!isConnected)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));

            var keys = ToKeyArr(model.ActiveKey);
            if (keys == null)
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.WrongPrivateActimeKey));

            var asset = new Asset();

            BaseOperation op;
            if (model.PowerAction == PowerAction.PowerUp)
            {
                asset.FromOldFormat($"{model.Value.ToString(CultureInfo.InvariantCulture)} {Config.Steem}");
                op = new TransferToVestingOperation(model.From, model.To, asset);
            }
            else
            {
                var vestsExchangeRatio = await GetVestsExchangeRatioAsync(ct).ConfigureAwait(false);
                if (!vestsExchangeRatio.IsSuccess)
                    return new OperationResult<VoidResponse>(vestsExchangeRatio.Exception);

                asset.FromOldFormat($"{(model.Value / vestsExchangeRatio.Result).ToString("F6", CultureInfo.InvariantCulture)} {Config.Vests}");
                op = new WithdrawVestingOperation(model.Login, asset);
            }

            return await BroadcastAsync(keys, new[] { op }, ct).ConfigureAwait(false);
        }

        public override async Task<OperationResult<VoidResponse>> ClaimRewardsAsync(ClaimRewardsModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<VoidResponse>(results);

            var isConnected = await TryReconnectChainAsync(ct).ConfigureAwait(false);
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

            return await BroadcastAsync(keys, new BaseOperation[] { op }, ct).ConfigureAwait(false);
        }

        #endregion Post requests

        #region Get
        public override async Task<OperationResult<string>> GetVerifyTransactionAsync(AuthorizedWifModel model, CancellationToken ct)
        {
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
            var tr = await _operationManager.CreateTransactionAsync(properties, keys, op, ct).ConfigureAwait(false);

            var conv = JsonConvert.SerializeObject(tr, _operationManager.CondenserJsonSerializerSettings);
            return new OperationResult<string> { Result = conv };
        }

        public override async Task<OperationResult<VoidResponse>> ValidatePrivateKeyAsync(ValidatePrivateKeyModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<VoidResponse>(results);

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

            var isConnected = await TryReconnectChainAsync(ct).ConfigureAwait(false);
            if (!isConnected)
            {
                return new OperationResult<VoidResponse>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));
            }

            var result = new OperationResult<VoidResponse>();


            var args = new FindAccountsArgs
            {
                Accounts = new[] { model.Login }
            };
            var resp = await _operationManager.FindAccountsAsync(args, ct).ConfigureAwait(false);
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

        public override async Task<OperationResult<AccountInfoResponse>> GetAccountInfoAsync(string userName, CancellationToken ct)
        {
            var isConnected = await TryReconnectChainAsync(ct).ConfigureAwait(false);
            if (!isConnected)
            {
                return new OperationResult<AccountInfoResponse>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));
            }

            var result = new OperationResult<AccountInfoResponse>();

            var args = new FindAccountsArgs
            {
                Accounts = new[] { userName }
            };
            var resp = await _operationManager.FindAccountsAsync(args, ct).ConfigureAwait(false);
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

            var vestsExchangeRatio = await GetVestsExchangeRatioAsync(ct).ConfigureAwait(false);
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
                        RewardSbd = acc.RewardSbdBalance.ToDouble(),
                        DelegatedToMe = acc.ReceivedVestingShares.ToDouble()  * vestsExchangeRatio.Result,
                        DelegatedByMe = acc.DelegatedVestingShares.ToDouble()  * vestsExchangeRatio.Result,
                        ToWithdraw = long.Parse(acc.ToWithdraw.ToString()) / 10e5 * vestsExchangeRatio.Result
                    },
                    new BalanceModel(acc.SbdBalance.ToDouble(), 3, CurrencyType.Sbd)
                    {
                        EffectiveSp = effectiveSp,
                        RewardSteem = acc.RewardSteemBalance.ToDouble(),
                        RewardSp = acc.RewardVestingBalance.ToDouble() * vestsExchangeRatio.Result,
                        RewardSbd = acc.RewardSbdBalance.ToDouble(),
                        DelegatedToMe = acc.ReceivedVestingShares.ToDouble() * vestsExchangeRatio.Result,
                        DelegatedByMe = acc.DelegatedVestingShares.ToDouble() * vestsExchangeRatio.Result,
                        ToWithdraw = long.Parse(acc.ToWithdraw.ToString()) / 10e5 * vestsExchangeRatio.Result
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

        public override async Task<OperationResult<AccountHistoryResponse[]>> GetAccountHistoryAsync(string userName, CancellationToken ct)
        {
            var isConnected = await TryReconnectChainAsync(ct).ConfigureAwait(false);
            if (!isConnected)
                return new OperationResult<AccountHistoryResponse[]>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));

            var result = new OperationResult<AccountHistoryResponse[]>();

            var args = new GetAccountHistoryArgs
            {
                Account = userName,
                Start = ulong.MaxValue,
                Limit = 1000
            };
            var resp = await _operationManager.CondenserGetAccountHistoryAsync(args, ct).ConfigureAwait(false);
            if (resp.IsError)
            {
                result.Exception = new RequestException(resp);
                return result;
            }

            var vestsExchangeRatio = await GetVestsExchangeRatioAsync(ct).ConfigureAwait(false);
            if (!vestsExchangeRatio.IsSuccess)
                return new OperationResult<AccountHistoryResponse[]>(vestsExchangeRatio.Exception);

            result.Result = resp.Result.History.Where(Filter).Select(pair => Transform(pair, vestsExchangeRatio.Result)).OrderByDescending(x => x.DateTime).ToArray();
            return result;
        }

        private async Task<OperationResult<double>> GetVestsExchangeRatioAsync(CancellationToken token)
        {
            if (_vestsExchangeRatio.HasValue)
                return new OperationResult<double>(_vestsExchangeRatio.Value);

            var properties = await _operationManager.GetDynamicGlobalPropertiesAsync(token).ConfigureAwait(false);
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
                            Amount = $"{(typed.VestingShares.ToDouble() * vestsExchangeRatio).ToBalanceValueString()} {CurrencyType.Steem.ToString().ToUpper()}"
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
                            RewardSp = (typed.RewardVests.ToDouble() * vestsExchangeRatio).ToBalanceValueString(),
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
