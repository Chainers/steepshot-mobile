using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ditch;
using Ditch.Errors;
using Ditch.JsonRpc;
using Ditch.Operations.Get;
using Ditch.Operations.Post;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Serializing;

namespace Steepshot.Core.HttpClient
{
    public class DitchApi : BaseClient, ISteepshotApiClient
    {
        private readonly ChainInfo _chainInfo;
        private readonly JsonNetConverter _jsonConverter;
        private readonly Regex _errorMsg = new Regex(@"(?<=[\w\s\(\)&|\.<>=]+:\s+)[a-z\s0-9.]*", RegexOptions.IgnoreCase);
        private readonly Dictionary<string, OperationResult<UploadResponse>> _imgCash;

        private OperationManager _operationManager;
        private OperationManager OperationManager => _operationManager ?? (_operationManager = new OperationManager(_chainInfo.Url, _chainInfo.ChainId));

        public DitchApi(KnownChains chain, bool isDev) : base(ChainToUrl(chain, isDev))
        {
            _chainInfo = ChainManager.GetChainInfo(chain == KnownChains.Steem ? Ditch.KnownChains.Steem : Ditch.KnownChains.Golos);
            _jsonConverter = new JsonNetConverter();
            _imgCash = new Dictionary<string, OperationResult<UploadResponse>>();
        }

        private static string ChainToUrl(KnownChains chain, bool isDev)
        {
            if (chain == KnownChains.Steem)
                return isDev ? Constants.SteemUrlQa : Constants.SteemUrl;
            return isDev ? Constants.GolosUrlQa : Constants.GolosUrl;
        }

        #region Post requests

        public async Task<OperationResult<VoteResponse>> Vote(VoteRequest request, CancellationTokenSource cts)
        {
            var errors = CheckInternetConnection();
            if (errors != null)
                return new OperationResult<VoteResponse>() { Errors = errors.Errors };
            return await Task.Run(() =>
            {
                string author;
                string permlink;
                if (!TryCastUrlToAuthorAndPermlink(request.Identifier, out author, out permlink))
                {
                    return new OperationResult<VoteResponse>
                    {
                        Errors = new List<string> { Localization.Errors.IncorrectIdentifier }
                    };
                }

                var weigth = (short)(request.Type == VoteType.Up ? 10000 : 0);
                var op = new VoteOperation(request.Login, author, permlink, weigth);
                var resp = OperationManager.BroadcastOperations(ToKeyArr(request.PostingKey), op);

                var result = new OperationResult<VoteResponse>();
                if (!resp.IsError)
                {
                    var content = OperationManager.GetContent(author, permlink);
                    if (!content.IsError)
                    {
                        //Convert Money type to double
                        result.Result = new VoteResponse(true)
                        {
                            NewTotalPayoutReward = content.Result.TotalPayoutValue + content.Result.CuratorPayoutValue + content.Result.PendingPayoutValue
                        };
                    }
                }
                else
                {
                    OnError(resp, result);
                }

                Trace($"post/{request.Identifier}/{request.Type.GetDescription()}", request.Login, result.Errors, request.Identifier);
                return result;
            });
        }

        public async Task<OperationResult<FollowResponse>> Follow(FollowRequest request, CancellationTokenSource cts)
        {
            var errors = CheckInternetConnection();
            if (errors != null)
                return new OperationResult<FollowResponse> { Errors = errors.Errors };
            return await Task.Run(() =>
            {
                var op = request.Type == FollowType.Follow
                    ? new FollowOperation(request.Login, request.Username, Ditch.Operations.Enums.FollowType.blog, request.Login)
                    : new UnfollowOperation(request.Login, request.Username, request.Login);

                var resp = OperationManager.BroadcastOperations(ToKeyArr(request.PostingKey), op);

                var result = new OperationResult<FollowResponse>();

                if (!resp.IsError)
                    result.Result = new FollowResponse(true);
                else
                    OnError(resp, result);

                Trace($"user/{request.Username}/{request.Type.ToString().ToLowerInvariant()}", request.Login, result.Errors, request.Username);
                return result;
            });
        }

        public async Task<OperationResult<LoginResponse>> LoginWithPostingKey(AuthorizedRequest request, CancellationTokenSource cts)
        {
            var errors = CheckInternetConnection();
            if (errors != null)
                return new OperationResult<LoginResponse> { Errors = errors.Errors };
            return await Task.Run(() =>
            {
                var keys = ToKeyArr(request.PostingKey);
                var invalidKey = keys.Any(k => k.Length == 0);
                if (invalidKey)
                    return new OperationResult<LoginResponse> { Errors = new List<string> { Localization.Errors.WrongPrivateKey } };


                var op = new FollowOperation(request.Login, "steepshot", Ditch.Operations.Enums.FollowType.blog, request.Login);
                var resp = OperationManager.VerifyAuthority(keys, op);

                var result = new OperationResult<LoginResponse>();

                if (!resp.IsError)
                    result.Result = new LoginResponse(true);
                else
                    OnError(resp, result);

                Trace("login-with-posting", request.Login, result.Errors, string.Empty);
                return result;
            });
        }

        public async Task<OperationResult<CreateCommentResponse>> CreateComment(CreateCommentRequest request, CancellationTokenSource cts)
        {
            var errors = CheckInternetConnection();
            if (errors != null)
                return new OperationResult<CreateCommentResponse> { Errors = errors.Errors };
            return await Task.Run(() =>
            {
                string author;
                string permlink;
                if (!TryCastUrlToAuthorAndPermlink(request.Url, out author, out permlink))
                {
                    return new OperationResult<CreateCommentResponse>
                    {
                        Errors = new List<string> { Localization.Errors.IncorrectIdentifier }
                    };
                }

                var op = new ReplyOperation(author, permlink, request.Login, request.Body, $"{{\"app\": \"steepshot/{request.AppVersion}\"}}");

                var resp = OperationManager.BroadcastOperations(ToKeyArr(request.PostingKey), op);

                var result = new OperationResult<CreateCommentResponse>();
                if (!resp.IsError)
                    result.Result = new CreateCommentResponse(true);
                else
                    OnError(resp, result);
                Trace($"post/{request.Url}/comment", request.Login, result.Errors, request.Url);
                return result;
            });
        }

        public async Task<OperationResult<ImageUploadResponse>> Upload(UploadImageRequest request, CancellationTokenSource cts)
        {
            var errors = CheckInternetConnection();
            if (errors != null)
                return new OperationResult<ImageUploadResponse> { Errors = errors.Errors };

            return await Task.Run(async () =>
            {
                var hash = string.Empty;
                OperationResult<UploadResponse> uploadResponse = null;
                if (_imgCash.Any())
                {
                    hash = System.Text.Encoding.UTF8.GetString(Cryptography.ECDSA.Secp256k1Manager.GetMessageHash(request.Photo));
                    if (!string.IsNullOrEmpty(hash) && _imgCash.ContainsKey(hash))
                        uploadResponse = _imgCash[hash];
                }

                if (uploadResponse == null)
                {
                    var op = new FollowOperation(request.Login, "steepshot", Ditch.Operations.Enums.FollowType.blog, request.Login);
                    var tr = OperationManager.CreateTransaction(DynamicGlobalPropertyApiObj.Default, ToKeyArr(request.PostingKey), op);
                    var trx = _jsonConverter.Serialize(tr);

                    PostOperation.PrepareTags(request.Tags);
                    uploadResponse = await UploadWithPrepare(request, trx, cts);
                }

                var result = new OperationResult<ImageUploadResponse>();
                if (uploadResponse.Success)
                {
                    var upResp = uploadResponse.Result;
                    var meta = upResp.Meta.ToString();
                    if (!string.IsNullOrWhiteSpace(meta))
                    {
                        meta = meta.Replace(Environment.NewLine, string.Empty);
                    }
                    var post = new PostOperation("steepshot", request.Login, request.Title, upResp.Payload.Body, meta);
                    var resp = OperationManager.BroadcastOperations(ToKeyArr(request.PostingKey), post);
                    if (!resp.IsError)
                    {
                        result.Result = upResp.Payload;
                        if (_imgCash.ContainsKey(hash))
                            _imgCash.Remove(hash);
                    }
                    else
                    {
                        OnError(resp, result);
                        hash = System.Text.Encoding.UTF8.GetString(Cryptography.ECDSA.Secp256k1Manager.GetMessageHash(request.Photo));
                        if (!_imgCash.ContainsKey(hash))
                            _imgCash.Add(hash, uploadResponse);
                    }
                    Trace("post", request.Login, result.Errors, post.Permlink);
                }
                else
                {
                    result.Errors.AddRange(uploadResponse.Errors);
                }
                return result;
            });
        }

        public async Task<OperationResult<LogoutResponse>> Logout(AuthorizedRequest request, CancellationTokenSource cts)
        {
            var errors = CheckInternetConnection();
            if (errors != null)
                return new OperationResult<LogoutResponse>() { Errors = errors.Errors };
            return await Task.Run(() => new OperationResult<LogoutResponse>
            {
                Result = new LogoutResponse(true)
            });
        }

        #endregion Post requests

        private bool TryCastUrlToAuthorAndPermlink(string url, out string author, out string permlink)
        {
            var start = url.LastIndexOf('@');
            if (start == -1)
            {
                author = permlink = null;
                return false;
            }
            var authAndPermlink = url.Remove(0, start + 1);
            var authPostArr = authAndPermlink.Split('/');
            if (authPostArr.Length != 2)
            {
                author = permlink = null;
                return false;
            }
            author = authPostArr[0];
            permlink = authPostArr[1];
            return true;
        }

        private List<byte[]> ToKeyArr(string postingKey)
        {
            return new List<byte[]> { Ditch.Helpers.Base58.TryGetBytes(postingKey) };
        }

        private void OnError<T>(JsonRpcResponse response, OperationResult<T> operationResult)
        {
            if (response.IsError)
            {
                if (response.Error is SystemError)
                {
                    switch (response.Error.Code)
                    {
                        case (int)ErrorCodes.ConnectionTimeoutError:
                            {
                                operationResult.Errors.Add(Localization.Errors.EnableConnectToServer);
                                break;
                            }
                        case (int)ErrorCodes.ResponseTimeoutError:
                            {
                                operationResult.Errors.Add(Localization.Errors.ServeNotRespond);
                                break;
                            }
                        default:
                            {
                                operationResult.Errors.Add(Localization.Errors.ServeUnexpectedError);
                                break;
                            }
                    }
                }
                else if (response.Error is ResponseError)
                {
                    var typedError = (ResponseError)response.Error;
                    var t = typeof(T);

                    switch (typedError.Data.Code)
                    {
                        case 10: //Assert Exception
                            {
                                if (typedError.Data.Stack.Any())
                                {
                                    var match = _errorMsg.Match(typedError.Data.Stack[0].Format);
                                    if (match.Success && !string.IsNullOrWhiteSpace(match.Value))
                                    {
                                        operationResult.Errors.Add(match.Value);
                                        break;
                                    }
                                }
                                goto default;
                            }
                        //case 13: unknown key
                        //case 3000000: "transaction exception"
                        //case 3010000: "missing required active authority"
                        //case 3020000: "missing required owner authority"
                        //case 3030000: "missing required posting authority"
                        //case 3040000: "missing required other authority"
                        //case 3050000: "irrelevant signature included"
                        //case 3060000: "duplicate signature included"
                        case 3030000:
                            {
                                if (t.Name == "LoginResponse")
                                {
                                    operationResult.Errors.Add(Localization.Errors.WrongPrivateKey);
                                    break;
                                }
                                goto default;
                            }
                        default:
                            {
                                operationResult.Errors.Add(Localization.Errors.ServeRejectRequest(typedError.Data.Code, typedError.Data.Message));
                                break;
                            }
                    }
                }
                else
                {
                    operationResult.Errors.Add(response.GetErrorMessage());
                }
            }
        }
    }
}
