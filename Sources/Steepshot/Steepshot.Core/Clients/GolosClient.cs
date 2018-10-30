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
using Steepshot.Core.Interfaces;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Clients
{
    public sealed class GolosClient : BaseDitchClient
    {
        private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly OperationManager _operationManager;
        private readonly ConfigManager _configManager;
        private double? _vestsExchangeRatio;

        public override bool IsConnected => _operationManager.IsConnected;

        public override KnownChains Chain => KnownChains.Golos;


        public GolosClient(ExtendedHttpClient extendedHttpClient, ILogService logService, ConfigManager configManager)
            : base(extendedHttpClient, logService)
        {
            _configManager = configManager;
            var webSocketManager = new WebSocketManager();
            _operationManager = new OperationManager(webSocketManager);
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

                await _configManager.UpdateAsync(ExtendedHttpClient, KnownChains.Golos, token)
                    .ConfigureAwait(false);

                var cUrls = _configManager.GolosNodeConfigs
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
            var resp = await _operationManager.BroadcastOperationsSynchronousAsync(keys, ct, op).ConfigureAwait(false);

            var result = new OperationResult<VoidResponse>();
            if (resp.IsError)
                result.Exception = new RequestException(resp);
            else
                result.Result = new VoidResponse();

            return result;
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
                ? new FollowOperation(model.Login, model.Username, Ditch.Golos.Models.FollowType.Blog, model.Login)
                : new UnfollowOperation(model.Login, model.Username, model.Login);
            var resp = await _operationManager.BroadcastOperationsSynchronousAsync(keys, ct, op).ConfigureAwait(false);

            var result = new OperationResult<VoidResponse>();

            if (resp.IsError)
                result.Exception = new RequestException(resp);
            else
                result.Result = new VoidResponse();

            return result;
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

            var resp = await _operationManager.BroadcastOperationsSynchronousAsync(keys, ct, ops).ConfigureAwait(false);

            var result = new OperationResult<VoidResponse>();
            if (resp.IsError)
                result.Exception = new RequestException(resp);
            else
                result.Result = new VoidResponse();

            return result;
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
            var resp = await _operationManager.BroadcastOperationsSynchronousAsync(keys, ct, op).ConfigureAwait(false);

            var result = new OperationResult<VoidResponse>();
            if (resp.IsError)
                result.Exception = new RequestException(resp);
            else
                result.Result = new VoidResponse();
            return result;
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

            var resp = await _operationManager.LookupAccountNamesAsync(new[] { model.Login }, ct).ConfigureAwait(false);
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
            var resp2 = await _operationManager.BroadcastOperationsSynchronousAsync(keys, ct, op).ConfigureAwait(false);
            if (resp2.IsError)
                result.Exception = new RequestException(resp2);
            else
                result.Result = new VoidResponse();
            return result;
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
            var resp = await _operationManager.BroadcastOperationsSynchronousAsync(keys, ct, op).ConfigureAwait(false);
            if (resp.IsError)
                result.Exception = new RequestException(resp);
            else
                result.Result = new VoidResponse();
            return result;
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
                var vestsExchangeRatio = await GetVestsExchangeRatioAsync(ct).ConfigureAwait(false);
                if (!vestsExchangeRatio.IsSuccess)
                    return new OperationResult<VoidResponse>(vestsExchangeRatio.Exception);

                asset = new Asset($"{(model.Value / vestsExchangeRatio.Result).ToString("F6", CultureInfo.InvariantCulture)} GESTS");
                op = new WithdrawVestingOperation(model.From, asset);
            }

            var resp = await _operationManager.BroadcastOperationsSynchronousAsync(keys, ct, op).ConfigureAwait(false);
            if (resp.IsError)
                result.Exception = new RequestException(resp);
            else
                result.Result = new VoidResponse();
            return result;
        }

        public override Task<OperationResult<VoidResponse>> ClaimRewardsAsync(ClaimRewardsModel model, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        #endregion Post requests

        #region Get

        public override async Task<OperationResult<string>> GetVerifyTransactionAsync(AuthorizedWifModel model, CancellationToken ct)
        {
            var isConnected = await TryReconnectChainAsync(ct).ConfigureAwait(false);
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
            var tr = await _operationManager.CreateTransactionAsync(properties, keys, op, ct).ConfigureAwait(false);
            return new OperationResult<string> { Result = JsonConvert.SerializeObject(tr) };
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

            var resp = await _operationManager.LookupAccountNamesAsync(new[] { model.Login }, ct).ConfigureAwait(false);
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

        public override async Task<OperationResult<AccountInfoResponse>> GetAccountInfoAsync(string userName, CancellationToken ct)
        {
            var isConnected = await TryReconnectChainAsync(ct).ConfigureAwait(false);
            if (!isConnected)
                return new OperationResult<AccountInfoResponse>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));

            var result = new OperationResult<AccountInfoResponse>();

            var resp = await _operationManager.LookupAccountNamesAsync(new[] { userName }, ct).ConfigureAwait(false);
            if (resp.IsError)
            {
                result.Exception = new RequestException(resp);
                return result;
            }

            if (resp.Result.Length != 1 || resp.Result[0] == null)
                return new OperationResult<AccountInfoResponse>(new ValidationException(LocalizationKeys.UnexpectedProfileData));

            var acc = resp.Result[0];

            var vestsExchangeRatio = await GetVestsExchangeRatioAsync(ct).ConfigureAwait(false);
            if (!vestsExchangeRatio.IsSuccess)
                return new OperationResult<AccountInfoResponse>(vestsExchangeRatio.Exception);

            var effectiveSp = (acc.VestingShares.ToDouble() + acc.ReceivedVestingShares.ToDouble() - acc.DelegatedVestingShares.ToDouble()) * vestsExchangeRatio.Result;

            result.Result = new AccountInfoResponse
            {
                Chains = KnownChains.Golos,
                PublicPostingKeys = acc.Posting.KeyAuths.Select(i => i.Key.Data).ToArray(),
                PublicActiveKeys = acc.Active.KeyAuths.Select(i => i.Key.Data).ToArray(),
                Metadata = JsonConvert.DeserializeObject<AccountMetadata>(acc.JsonMetadata),
                Balances = new[]
                {
                    new BalanceModel(acc.Balance.ToDouble(), 3, CurrencyType.Golos)
                    {
                        EffectiveSp = effectiveSp,
                        ToWithdraw = long.Parse(acc.ToWithdraw.ToString()) / 10e5 * vestsExchangeRatio.Result
                    },
                    new BalanceModel(acc.SbdBalance.ToDouble(), 3,  CurrencyType.Gbg)
                    {
                        EffectiveSp =  effectiveSp,
                        ToWithdraw = long.Parse(acc.ToWithdraw.ToString()) / 10e5 * vestsExchangeRatio.Result
                    }
                }
            };

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


        private readonly string[] _accountHistoryFilter = {
            TransferOperation.OperationName,
            TransferToVestingOperation.OperationName,
            WithdrawVestingOperation.OperationName
        };

        public override async Task<OperationResult<AccountHistoryResponse[]>> GetAccountHistoryAsync(AccountHistoryModel model, CancellationToken ct)
        {
            var isConnected = await TryReconnectChainAsync(ct).ConfigureAwait(false);
            if (!isConnected)
                return new OperationResult<AccountHistoryResponse[]>(new ValidationException(LocalizationKeys.EnableConnectToBlockchain));

            var result = new OperationResult<AccountHistoryResponse[]>();

            var resp = await _operationManager.GetAccountHistoryAsync(model.Account, model.Start, model.Limit, ct).ConfigureAwait(false);
            if (resp.IsError)
            {
                result.Exception = new RequestException(resp);
                return result;
            }

            var vestsExchangeRatio = await GetVestsExchangeRatioAsync(ct).ConfigureAwait(false);
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
                            Id = arg.Key,
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
                            Id = arg.Key,
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
                            Id = arg.Key,
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