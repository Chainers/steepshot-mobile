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
        private readonly Regex _errorMsg = new Regex(@"(?<=[a-z_>=\s]+:\s+)[a-z\s0-9.]*", RegexOptions.IgnoreCase);
        private OperationManager _operationManager;

        private OperationManager OperationManager => _operationManager ?? (_operationManager = new OperationManager(_chainInfo.Url, _chainInfo.ChainId));

        public DitchApi(KnownChains chain, bool isDev) : base(ChainToUrl(chain, isDev))
        {
            _chainInfo = ChainManager.GetChainInfo(chain == KnownChains.Steem ? Ditch.KnownChains.Steem : Ditch.KnownChains.Golos);
            _jsonConverter = new JsonNetConverter();
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
            return await Task.Run(() =>
            {
                var authPost = UrlToAuthorAndPermlink(request.Identifier);
                var weigth = (short)(request.Type == VoteType.Up ? 10000 : 0);
                var op = new VoteOperation(request.Login, authPost.Item1, authPost.Item2, weigth);
                var resp = OperationManager.BroadcastOperations(ToKeyArr(request.PostingKey), op);

                var result = new OperationResult<VoteResponse>();
                if (!resp.IsError)
                {
                    var content = OperationManager.GetContent(authPost.Item1, authPost.Item2);
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

                Trace($"post/{request.Identifier}/{request.Type.GetDescription()}", request.Login, result.Errors);
                return result;
            });
        }

        public async Task<OperationResult<FollowResponse>> Follow(FollowRequest request, CancellationTokenSource cts)
        {
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

                Trace($"user/{request.Username}/{request.Type.ToString().ToLowerInvariant()}", request.Login, result.Errors);
                return result;
            });
        }

        public async Task<OperationResult<LoginResponse>> LoginWithPostingKey(AuthorizedRequest request, CancellationTokenSource cts)
        {
            return await Task.Run(() =>
            {
                var op = new FollowOperation(request.Login, "steepshot", Ditch.Operations.Enums.FollowType.blog, request.Login);
                var resp = OperationManager.VerifyAuthority(ToKeyArr(request.PostingKey), op);

                var result = new OperationResult<LoginResponse>();

                if (!resp.IsError)
                    result.Result = new LoginResponse(true);
                else
                    OnError(resp, result);

                Trace("login-with-posting", request.Login, result.Errors);
                return result;
            });
        }

        public async Task<OperationResult<CreateCommentResponse>> CreateComment(CreateCommentRequest request, CancellationTokenSource cts)
        {
            return await Task.Run(() =>
            {
                var authPost = UrlToAuthorAndPermlink(request.Url);
                var op = new ReplyOperation(authPost.Item1, authPost.Item2, request.Login, request.Body, "{\"app\": \"steepshot/0.0.5\"}");

                var resp = OperationManager.BroadcastOperations(ToKeyArr(request.PostingKey), op);

                var result = new OperationResult<CreateCommentResponse>();
                if (!resp.IsError)
                    result.Result = new CreateCommentResponse(true);
                else
                    OnError(resp, result);
                Trace($"post/{request.Url}/comment", request.Login, result.Errors);
                return result;
            });
        }

        public async Task<OperationResult<ImageUploadResponse>> Upload(UploadImageRequest request, CancellationTokenSource cts)
        {
            return await Task.Run(async () =>
            {
                var op = new FollowOperation(request.Login, "steepshot", Ditch.Operations.Enums.FollowType.blog, request.Login);
                var tr = OperationManager.CreateTransaction(DynamicGlobalPropertyApiObj.Default, ToKeyArr(request.PostingKey), op);
                var trx = _jsonConverter.Serialize(tr);

                Ditch.Helpers.Transliteration.PrepareTags(request.Tags);
                var uploadResponse = await UploadWithPrepare(request, trx, cts);

                var result = new OperationResult<ImageUploadResponse>();
                if (uploadResponse.Success)
                {
                    var upResp = uploadResponse.Result;
                    var meta = _jsonConverter.Serialize(upResp.Meta);
                    var post = new PostOperation("steepshot", request.Login, request.Title, upResp.Payload.Body, meta);
                    var resp = OperationManager.BroadcastOperations(ToKeyArr(request.PostingKey), post);
                    if (!resp.IsError)
                        result.Result = upResp.Payload;
                    else
                        OnError(resp, result);
                }
                else
                {
                    result.Errors.AddRange(uploadResponse.Errors);
                }
                Trace("post", request.Login, result.Errors);
                return result;
            });
        }

        public async Task<OperationResult<LogoutResponse>> Logout(AuthorizedRequest request, CancellationTokenSource cts)
        {
            return await Task.Run(() => new OperationResult<LogoutResponse>
            {
                Result = new LogoutResponse(true)
            });
        }

        #endregion Post requests

        private Tuple<string, string> UrlToAuthorAndPermlink(string url)
        {
            var authAndPermlink = url.Remove(0, url.LastIndexOf('@') + 1);
            var authPostArr = authAndPermlink.Split('/');
            if (authPostArr.Length != 2) throw new InvalidCastException($"Unexpected url format: {url}");
            return new Tuple<string, string>(authPostArr[0], authPostArr[1]);
        }

        private IEnumerable<byte[]> ToKeyArr(string postingKey)
        {
            return new List<byte[]> { Ditch.Helpers.Base58.GetBytes(postingKey) };
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
                                operationResult.Errors.Add("Can not connect to the server, check for an Internet connection and try again.");
                                break;
                            }
                        case (int)ErrorCodes.ResponseTimeoutError:
                            {
                                operationResult.Errors.Add("The server does not respond to the request. Check your internet connection and try again.");
                                break;
                            }
                        default:
                            {
                                operationResult.Errors.Add("An unexpected error occurred. Check the Internet or try restarting the application.");
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
                                    operationResult.Errors.Add("Invalid private posting key!");
                                    break;
                                }
                                goto default;
                            }
                        default:
                            {
                                operationResult.Errors.Add($"The server did not accept the request! Reason ({typedError.Data.Code}) {typedError.Data.Message}");
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