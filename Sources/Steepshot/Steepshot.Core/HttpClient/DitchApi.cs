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

namespace Steepshot.Core.HttpClient
{
    public class DitchApi : BaseClient, ISteepshotApiClient
    {
        private readonly Regex _errorMsg = new Regex(@"(?<=[\w\s\(\)&|\.<>=]+:\s+)[a-z\s0-9.]*", RegexOptions.IgnoreCase);
        private readonly OperationManager _operationManager;
        private volatile bool _enableWrite;

        public DitchApi()
        {
            _operationManager = new OperationManager();
        }

        public async Task<bool> Connect(KnownChains chain, bool isDev, bool connectToBlockcain, CancellationToken token)
        {
            var sUrl = string.Empty;
            switch (chain)
            {
                case KnownChains.Steem when isDev:
                    sUrl = Constants.SteemUrlQa;
                    break;
                case KnownChains.Steem when !isDev:
                    sUrl = Constants.SteemUrl;
                    break;
                case KnownChains.GolosTestNet when isDev:
                case KnownChains.Golos when isDev:
                    sUrl = Constants.GolosUrlQa;
                    break;
                case KnownChains.GolosTestNet when !isDev:
                case KnownChains.Golos when !isDev:
                    sUrl = Constants.GolosUrl;
                    break;
            }

            EnableRead = false;
            _enableWrite = false;
            if (Gateway != null)
            {
                CtsMain.Cancel();
                CtsMain = new CancellationTokenSource();
            }
            Gateway = new ApiGateway(sUrl);
            EnableRead = true;
            if (connectToBlockcain)
                return await Task.Run(() => TryReconnectChain(chain, token), token);
            return false;
        }

        public bool TryReconnectChain(KnownChains chain, CancellationToken token)
        {
            try
            {
                if (!_enableWrite)
                {
                    var cUrls = new List<string>();
                    switch (chain)
                    {
                        case KnownChains.Steem:
                            cUrls = new List<string> { "wss://steemd.steemit.com" };
                            break;
                        case KnownChains.Golos:
                            cUrls = new List<string> { "wss://ws.golos.io" };
                            break;
                        case KnownChains.GolosTestNet:
                            cUrls = new List<string> { "wss://ws.testnet.golos.io" };
                            break;
                    }

                    var cts = CancellationTokenSource.CreateLinkedTokenSource(token, CtsMain.Token);
                    var conectedTo = _operationManager.TryConnectTo(cUrls, cts.Token);
                    if (!string.IsNullOrEmpty(conectedTo))
                        _enableWrite = true;
                }
            }
            catch (Exception)
            {
                //todo nothing
            }
            return _enableWrite;
        }

        #region Post requests

        public async Task<OperationResult<VoteResponse>> Vote(VoteRequest request, CancellationToken ct)
        {
            if (!_enableWrite)
                return null;

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
                    var dt = DateTime.Now;
                    var content = _operationManager.GetContent(author, permlink, token.Token);
                    if (!content.IsError)
                    {
                        //Convert Money type to double
                        result.Result = new VoteResponse(true)
                        {
                            NewTotalPayoutReward = content.Result.TotalPayoutValue + content.Result.CuratorPayoutValue + content.Result.PendingPayoutValue,
                            NetVotes = content.Result.NetVotes,
                            VoteTime = dt
                        };
                    }
                }
                else
                {
                    OnError(resp, result);
                }

                Trace($"post/{request.Identifier}/{request.Type.GetDescription()}", request.Login, result.Errors, request.Identifier, token.Token).Wait(5000);
                return result;
            }, token.Token);
        }

        public async Task<OperationResult<FollowResponse>> Follow(FollowRequest request, CancellationToken ct)
        {
            if (!_enableWrite)
                return null;

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

                Trace($"user/{request.Username}/{request.Type.ToString().ToLowerInvariant()}", request.Login, result.Errors, request.Username, token.Token).Wait(5000);
                return result;
            }, token.Token);
        }

        public async Task<OperationResult<LoginResponse>> LoginWithPostingKey(AuthorizedRequest request, CancellationToken ct)
        {
            if (!_enableWrite)
                return null;

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

                Trace("login-with-posting", request.Login, result.Errors, string.Empty, token.Token).Wait(5000);
                return result;
            }, token.Token);
        }

        public async Task<OperationResult<CommentResponse>> CreateComment(CommentRequest request, CancellationToken ct)
        {
            if (!_enableWrite)
                return null;

            var keys = ToKeyArr(request.PostingKey);
            if (keys == null)
                return new OperationResult<CommentResponse> { Errors = new List<string> { Localization.Errors.WrongPrivateKey } };

            var token = CancellationTokenSource.CreateLinkedTokenSource(ct, CtsMain.Token);
            return await Task.Run(() =>
            {
                string author;
                string permlink;
                if (!TryCastUrlToAuthorAndPermlink(request.Url, out author, out permlink))
                {
                    return new OperationResult<CommentResponse>
                    {
                        Errors = new List<string> { Localization.Errors.IncorrectIdentifier }
                    };
                }

                var op = new ReplyOperation(author, permlink, request.Login, request.Body, $"{{\"app\": \"steepshot/{request.AppVersion}\"}}");

                var resp = _operationManager.BroadcastOperations(keys, token.Token, op);

                var result = new OperationResult<CommentResponse>();
                if (!resp.IsError)
                {
                    result.Result = new CommentResponse(true);
                    result.Result.Permlink = op.Permlink;
                }
                else
                    OnError(resp, result);
                Trace($"post/{request.Url}/comment", request.Login, result.Errors, request.Url, token.Token).Wait(5000);
                return result;
            }, token.Token);
        }

        public async Task<OperationResult<CommentResponse>> EditComment(CommentRequest request, CancellationToken ct)
        {
            if (!_enableWrite)
                return null;

            var keys = ToKeyArr(request.PostingKey);
            if (keys == null)
                return new OperationResult<CommentResponse> { Errors = new List<string> { Localization.Errors.WrongPrivateKey } };

            var token = CancellationTokenSource.CreateLinkedTokenSource(ct, CtsMain.Token);
            return await Task.Run(() =>
            {
                string author;
                string commentPermlink;
                string parentAuthor;
                string parentPermlink;
                if (!TryCastUrlToAuthorPermlinkAndParentPermlink(request.Url, out author, out commentPermlink, out parentAuthor, out parentPermlink) || !string.Equals(author, request.Login))
                {
                    return new OperationResult<CommentResponse>
                    {
                        Errors = new List<string> { Localization.Errors.IncorrectIdentifier }
                    };
                }

                var op = new CommentOperation(parentAuthor, parentPermlink, author, commentPermlink, string.Empty, request.Body, $"{{\"app\": \"steepshot/{request.AppVersion}\"}}");
                // var op = new ReplyOperation(author, permlink, request.Login, request.Body, $"{{\"app\": \"steepshot/{request.AppVersion}\"}}");

                var resp = _operationManager.BroadcastOperations(keys, token.Token, op);

                var result = new OperationResult<CommentResponse>();
                if (!resp.IsError)
                {
                    result.Result = new CommentResponse(true);
                    result.Result.Permlink = op.Permlink;
                }
                else
                {
                    OnError(resp, result);
                }
                Trace($"post/{request.Url}/comment", request.Login, result.Errors, request.Url, token.Token).Wait(5000);
                return result;
            }, token.Token);
        }

        public async Task<OperationResult<UploadResponse>> UploadWithPrepare(UploadImageRequest request, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var keys = ToKeyArr(request.PostingKey);
            if (keys == null)
                return new OperationResult<UploadResponse> { Errors = new List<string> { Localization.Errors.WrongPrivateKey } };

            var token = CancellationTokenSource.CreateLinkedTokenSource(ct, CtsMain.Token);

            return await Task.Run(async () =>
            {
                var op = new FollowOperation(request.Login, "steepshot", Ditch.Operations.Enums.FollowType.blog, request.Login);
                var tr = _operationManager.CreateTransaction(DynamicGlobalPropertyApiObj.Default, keys, token.Token, op);
                var trx = JsonConverter.Serialize(tr);

                PostOperation.PrepareTags(request.Tags);

                var response = await Gateway.Upload(GatewayVersion.V1, "post/prepare", request, trx, token.Token);
                var errorResult = CheckErrors(response);
                return CreateResult<UploadResponse>(response?.Content, errorResult);

            }, token.Token);
        }

        public async Task<OperationResult<ImageUploadResponse>> Upload(UploadImageRequest request, UploadResponse uploadResponse, CancellationToken ct)
        {
            if (!_enableWrite)
                return null;

            var keys = ToKeyArr(request.PostingKey);
            if (keys == null)
                return new OperationResult<ImageUploadResponse> { Errors = new List<string> { Localization.Errors.WrongPrivateKey } };

            var token = CancellationTokenSource.CreateLinkedTokenSource(ct, CtsMain.Token);
            return await Task.Run(() =>
            {
                PostOperation.PrepareTags(request.Tags);

                var meta = uploadResponse.Meta.ToString();
                if (!string.IsNullOrWhiteSpace(meta))
                    meta = meta.Replace(Environment.NewLine, string.Empty);

                var category = request.Tags.Length > 0 ? request.Tags[0] : "steepshot";
                var post = new PostOperation(category, request.Login, request.Title, uploadResponse.Payload.Body, meta);
                var ops = uploadResponse.Beneficiaries != null && uploadResponse.Beneficiaries.Any()
                    ? new BaseOperation[] { post, new BeneficiariesOperation(request.Login, post.Permlink, _operationManager.SbdSymbol, uploadResponse.Beneficiaries) }
                    : new BaseOperation[] { post };

                var resp = _operationManager.BroadcastOperations(keys, token.Token, ops);

                var result = new OperationResult<ImageUploadResponse>();
                if (!resp.IsError)
                {
                    uploadResponse.Payload.Permlink = post.Permlink;
                    result.Result = uploadResponse.Payload;
                }
                else
                    OnError(resp, result);

                Trace("post", request.Login, result.Errors, post.Permlink, token.Token).Wait(5000);

                return result;
            }, token.Token);
        }

        #endregion Post requests

        #region Get

        public async Task<OperationResult<Discussion>> GetDiscussion(string author, string permlink, CancellationToken ct)
        {
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

        private bool TryCastUrlToAuthorPermlinkAndParentPermlink(string url, out string author, out string commentPermlink, out string parentAuthor, out string parentPermlink)
        {
            var start = url.LastIndexOf('#');

            author = parentPermlink = parentAuthor = commentPermlink = null;

            if (start == -1)
                return false;

            if (!TryCastUrlToAuthorAndPermlink(url.Remove(0, start + 1), out author, out commentPermlink))
                return false;


            if (!TryCastUrlToAuthorAndPermlink(url.Substring(0, start), out parentAuthor, out parentPermlink))
                return false;

            return true;
        }

        private List<byte[]> ToKeyArr(string postingKey)
        {
            try
            {
                var key = Ditch.Helpers.Base58.TryGetBytes(postingKey);
                if (key == null || key.Length != 32)
                    return null;
                return new List<byte[]> { key };
            }
            catch (Exception)
            {
                //todo nothing
            }
            return null;
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
                                    if (typedError.Data.Stack[0].Format.Contains("STEEMIT_MAX_VOTE_CHANGES"))
                                    {
                                        operationResult.Errors.Add(Localization.Errors.MaxVoteChanges);
                                        break;
                                    }
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
