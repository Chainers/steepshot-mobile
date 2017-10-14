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
        private readonly JsonNetConverter _jsonConverter;
        private readonly Regex _errorMsg = new Regex(@"(?<=[\w\s\(\)&|\.<>=]+:\s+)[a-z\s0-9.]*", RegexOptions.IgnoreCase);
        private readonly OperationManager _operationManager;
        private CancellationTokenSource CtsMain;

        public DitchApi()
        {
            _jsonConverter = new JsonNetConverter();
            _operationManager = new OperationManager();
            CtsMain = new CancellationTokenSource();
        }

        public bool Connect(KnownChains chain, bool isDev)
        {
            string sUrl;
            List<string> cUrls;
            if (chain == KnownChains.Steem)
            {
                sUrl = isDev ? Constants.SteemUrlQa : Constants.SteemUrl;
                cUrls = new List<string> { "wss://steemd.steemit.com" };
            }
            else
            {
                sUrl = isDev ? Constants.GolosUrlQa : Constants.GolosUrl;
                cUrls = new List<string> { "wss://ws.golos.io" };
            }

            if (Gateway.GatewayUrl == sUrl)
                return true;

            Locked = 1;
            CtsMain.Cancel();
            Gateway.GatewayUrl = sUrl;
            var conectedTo = _operationManager.TryConnectTo(cUrls);
            CtsMain = new CancellationTokenSource();
            Locked = 0;
            return !string.IsNullOrEmpty(conectedTo);
        }

        #region Post requests

        public async Task<OperationResult<VoteResponse>> Vote(VoteRequest request, CancellationToken ct)
        {
            if (Locked == 1)
                return null;

            var errors = CheckInternetConnection();
            if (errors != null)
                return new OperationResult<VoteResponse> { Errors = errors.Errors };

            var keys = ToKeyArr(request.PostingKey);
            if (keys == null)
                return new OperationResult<VoteResponse> { Errors = new List<string> { Localization.Errors.WrongPrivateKey } };
            var token = CancellationTokenSource.CreateLinkedTokenSource(ct, CtsMain.Token);
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

                short weigth = 0;
                if (request.Type == VoteType.Up)
                    weigth = 10000;
                if (request.Type == VoteType.Flag)
                    weigth = -10000;

                var op = new VoteOperation(request.Login, author, permlink, weigth);
                var resp = _operationManager.BroadcastOperations(keys, token.Token, op);

                var result = new OperationResult<VoteResponse>();
                if (!resp.IsError)
                {
                    var content = _operationManager.GetContent(author, permlink, token.Token);
                    if (!content.IsError)
                    {
                        //Convert Money type to double
                        result.Result = new VoteResponse(true)
                        {
                            NewTotalPayoutReward = content.Result.TotalPayoutValue + content.Result.CuratorPayoutValue + content.Result.PendingPayoutValue,
                            NetVotes = content.Result.NetVotes,
                        };
                    }
                }
                else
                {
                    OnError(resp, result);
                }

                Trace($"post/{request.Identifier}/{request.Type.GetDescription()}", request.Login, result.Errors, request.Identifier, token.Token);
                return result;
            }, token.Token);
        }

        public async Task<OperationResult<FollowResponse>> Follow(FollowRequest request, CancellationToken ct)
        {
            if (Locked == 1)
                return null;

            var errors = CheckInternetConnection();
            if (errors != null)
                return new OperationResult<FollowResponse> { Errors = errors.Errors };

            var keys = ToKeyArr(request.PostingKey);
            if (keys == null)
                return new OperationResult<FollowResponse> { Errors = new List<string> { Localization.Errors.WrongPrivateKey } };

            var token = CancellationTokenSource.CreateLinkedTokenSource(ct, CtsMain.Token);
            return await Task.Run(() =>
            {
                var op = request.Type == FollowType.Follow
                    ? new FollowOperation(request.Login, request.Username, Ditch.Operations.Enums.FollowType.blog, request.Login)
                    : new UnfollowOperation(request.Login, request.Username, request.Login);
                var resp = _operationManager.BroadcastOperations(keys, token.Token, op);

                var result = new OperationResult<FollowResponse>();

                if (!resp.IsError)
                    result.Result = new FollowResponse(true);
                else
                    OnError(resp, result);

                Trace($"user/{request.Username}/{request.Type.ToString().ToLowerInvariant()}", request.Login, result.Errors, request.Username, token.Token);
                return result;
            }, token.Token);
        }

        public async Task<OperationResult<LoginResponse>> LoginWithPostingKey(AuthorizedRequest request, CancellationToken ct)
        {
            if (Locked == 1)
                return null;

            var errors = CheckInternetConnection();
            if (errors != null)
                return new OperationResult<LoginResponse> { Errors = errors.Errors };

            var keys = ToKeyArr(request.PostingKey);
            if (keys == null)
                return new OperationResult<LoginResponse> { Errors = new List<string> { Localization.Errors.WrongPrivateKey } };

            var token = CancellationTokenSource.CreateLinkedTokenSource(ct, CtsMain.Token);
            return await Task.Run(() =>
            {
                var op = new FollowOperation(request.Login, "steepshot", Ditch.Operations.Enums.FollowType.blog, request.Login);
                var resp = _operationManager.VerifyAuthority(keys, token.Token, op);

                var result = new OperationResult<LoginResponse>();

                if (!resp.IsError)
                    result.Result = new LoginResponse(true);
                else
                    OnError(resp, result);

                Trace("login-with-posting", request.Login, result.Errors, string.Empty, token.Token);
                return result;
            }, token.Token);
        }

        public async Task<OperationResult<CreateCommentResponse>> CreateComment(CreateCommentRequest request, CancellationToken ct)
        {
            if (Locked == 1)
                return null;

            var errors = CheckInternetConnection();
            if (errors != null)
                return new OperationResult<CreateCommentResponse> { Errors = errors.Errors };

            var keys = ToKeyArr(request.PostingKey);
            if (keys == null)
                return new OperationResult<CreateCommentResponse> { Errors = new List<string> { Localization.Errors.WrongPrivateKey } };

            var token = CancellationTokenSource.CreateLinkedTokenSource(ct, CtsMain.Token);
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

                var resp = _operationManager.BroadcastOperations(keys, token.Token, op);

                var result = new OperationResult<CreateCommentResponse>();
                if (!resp.IsError)
                {
                    result.Result = new CreateCommentResponse(true);
                    result.Result.Permlink = op.Permlink;
                }
                else
                    OnError(resp, result);
                Trace($"post/{request.Url}/comment", request.Login, result.Errors, request.Url, token.Token);
                return result;
            }, token.Token);
        }

        public async Task<OperationResult<ImageUploadResponse>> Upload(UploadImageRequest request, CancellationToken ct)
        {
            if (Locked == 1)
                return null;

            var errors = CheckInternetConnection();
            if (errors != null)
                return new OperationResult<ImageUploadResponse> { Errors = errors.Errors };

            var keys = ToKeyArr(request.PostingKey);
            if (keys == null)
                return new OperationResult<ImageUploadResponse> { Errors = new List<string> { Localization.Errors.WrongPrivateKey } };

            var token = CancellationTokenSource.CreateLinkedTokenSource(ct, CtsMain.Token);
            return await Task.Run(async () =>
            {
                var op = new FollowOperation(request.Login, "steepshot", Ditch.Operations.Enums.FollowType.blog, request.Login);
                var tr = _operationManager.CreateTransaction(DynamicGlobalPropertyApiObj.Default, keys, token.Token, op);
                var trx = _jsonConverter.Serialize(tr);

                PostOperation.PrepareTags(request.Tags);
                var uploadResponse = await UploadWithPrepare(request, trx, token.Token);

                var result = new OperationResult<ImageUploadResponse>();
                if (uploadResponse.Success)
                {
                    var upResp = uploadResponse.Result;
                    var meta = upResp.Meta.ToString();
                    if (!string.IsNullOrWhiteSpace(meta))
                        meta = meta.Replace(Environment.NewLine, string.Empty);

                    var category = request.Tags.Length > 0 ? request.Tags[0] : "steepshot";
                    var post = new PostOperation(category, request.Login, request.Title, upResp.Payload.Body, meta);
                    var ops = upResp.Beneficiaries != null && upResp.Beneficiaries.Any()
                        ? new BaseOperation[] { post, new BeneficiariesOperation(request.Login, post.Permlink, _operationManager.SbdSymbol, upResp.Beneficiaries) }
                        : new BaseOperation[] { post };

                    var resp = _operationManager.BroadcastOperations(keys, token.Token, ops);


                    if (!resp.IsError)
                    {
                        upResp.Payload.Permlink = post.Permlink;
                        result.Result = upResp.Payload;
                    }
                    else
                        OnError(resp, result);

                    Trace("post", request.Login, result.Errors, post.Permlink, token.Token);
                }
                else
                {
                    result.Errors.AddRange(uploadResponse.Errors);
                }
                return result;
            }, token.Token);
        }

        #endregion Post requests

        #region Get

        public async Task<OperationResult<Discussion>> GetDiscussion(string author, string permlink, CancellationToken ct)
        {
            var errors = CheckInternetConnection();
            if (errors != null)
                return new OperationResult<Discussion> { Errors = errors.Errors };

            var token = CancellationTokenSource.CreateLinkedTokenSource(ct, CtsMain.Token);
            return await Task.Run(() =>
            {
                var resp = _operationManager.GetContent(author, permlink, token.Token);

                var result = new OperationResult<Discussion>();

                if (!resp.IsError)
                    result.Result = resp.Result;
                else
                    OnError(resp, result);

                return result;
            }, token.Token);
        }

        #endregion

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
            var key = Ditch.Helpers.Base58.TryGetBytes(postingKey);
            if (key == null || key.Length != 32)
                return null;
            return new List<byte[]> { key };
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
                        case 13: //unknown key
                            {
                                operationResult.Errors.Add(Localization.Errors.WrongPrivateKey);
                                break;
                            }
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
