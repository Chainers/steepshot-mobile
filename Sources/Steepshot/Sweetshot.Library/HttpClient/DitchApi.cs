using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ditch;
using Ditch.Operations.Get;
using Ditch.Operations.Post;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using Steepshot.Core.HttpClient;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Sweetshot.Library.HttpClient
{
    public class DitchApi : ISteepshotApiClient
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private OperationManager _operationManager;
        private readonly ChainInfo _chainInfo;

        private readonly string _url;
        private IApiGateway _gateway;
        public string Url => _url;

        private IApiGateway Gateway
        {
            get { return _gateway ?? (_gateway = new ApiGateway(_url)); }
        }

        private OperationManager OperationManager
        {
            get
            {
                if (_operationManager == null)
                    _operationManager = new OperationManager(_chainInfo.Url, _chainInfo.ChainId, _jsonSerializerSettings);
                return _operationManager;
            }
        }

        public DitchApi(Steepshot.Core.KnownChains chain, bool isDev)
        {
            if (chain == Steepshot.Core.KnownChains.Steem)
                _url = isDev ? "https://qa.steepshot.org/api/v1/" : "https://steepshot.org/api/v1/";
            else
                _url = isDev ? "https://qa.golos.steepshot.org/api/v1/" : "https://golos.steepshot.org/api/v1/";

            _chainInfo = ChainManager.GetChainInfo(chain == Steepshot.Core.KnownChains.Steem ? KnownChains.Steem : KnownChains.Golos);
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
            };

        }


        #region Get requests

        public async Task<OperationResult<UserPostResponse>> GetUserPosts(UserPostsRequest request)
        {
            var parameters = new List<RequestParameter>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            AddLoginParameter(parameters, request.Login);

            var response = await Gateway.Get($"{request.Login}/user/{request.Username}/posts", parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<UserPostResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<UserPostResponse>> GetUserRecentPosts(UserRecentPostsRequest request)
        {
            var parameters = new List<RequestParameter>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            AddLoginParameter(parameters, request.Login);

            var response = await Gateway.Get($"{request.Login}/recent", parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<UserPostResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<UserPostResponse>> GetPosts(PostsRequest request, CancellationTokenSource cts = null)
        {
            var parameters = new List<RequestParameter>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            AddLoginParameter(parameters, request.Login);

            var endpoint = $"{request.Login}/posts/{request.Type.ToString().ToLowerInvariant()}";
            var response = await Gateway.Get(endpoint, parameters, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<UserPostResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<UserPostResponse>> GetPostsByCategory(PostsByCategoryRequest request, CancellationTokenSource cts = null)
        {
            var parameters = new List<RequestParameter>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            AddLoginParameter(parameters, request.Login);

            var endpoint = $"{request.Login}/posts/{request.Category}/{request.Type.ToString().ToLowerInvariant()}";
            var response = await Gateway.Get(endpoint, parameters, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<UserPostResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<GetVotersResponse>> GetPostVoters(GetVotesRequest request)
        {
            var parameters = new List<RequestParameter>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);

            var endpoint = $"post/{request.Url}/voters";
            var response = await Gateway.Get(endpoint, parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<GetVotersResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<GetCommentResponse>> GetComments(GetCommentsRequest request)
        {
            var parameters = new List<RequestParameter>();
            AddLoginParameter(parameters, request.Login);

            var endpoint = string.IsNullOrEmpty(request.Login) ?
                $"post/{request.Url}/comments" :
                $"{request.Login}/post/{request.Url}/comments";
            var response = await Gateway.Get(endpoint, parameters);

            var errorResult = CheckErrors(response);
            return CreateResult<GetCommentResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<SearchResponse<SearchResult>>> GetCategories(SearchRequest request, CancellationTokenSource cts = null)
        {
            var parameters = new List<RequestParameter>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);

            var response = await Gateway.Get("categories/top", parameters, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<SearchResponse<SearchResult>>(response.Content, errorResult);
        }

        public async Task<OperationResult<SearchResponse<SearchResult>>> SearchCategories(SearchWithQueryRequest request, CancellationTokenSource cts = null)
        {
            var parameters = new List<RequestParameter>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            parameters.Add(new RequestParameter { Key = "query", Value = request.Query, Type = ParameterType.QueryString });

            var response = await Gateway.Get("categories/search", parameters, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<SearchResponse<SearchResult>>(response.Content, errorResult);
        }

        public async Task<OperationResult<LogoutResponse>> Logout(LogoutRequest request)
        {
            return new OperationResult<LogoutResponse>(new LogoutResponse(true));
        }

        public async Task<OperationResult<UserProfileResponse>> GetUserProfile(UserProfileRequest request)
        {
            var parameters = new List<RequestParameter>();
            AddLoginParameter(parameters, request.Login);

            var response = await Gateway.Get($"{request.Login}/user/{request.Username}/info", parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<UserProfileResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<UserFriendsResponse>> GetUserFriends(UserFriendsRequest request)
        {
            var parameters = new List<RequestParameter>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            AddLoginParameter(parameters, request.Login);

            var endpoint = $"{request.Login}/user/{request.Username}/{request.Type.ToString().ToLowerInvariant()}";
            var response = await Gateway.Get(endpoint, parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<UserFriendsResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<TermOfServiceResponse>> TermsOfService()
        {
            const string endpoint = "/tos";
            var response = await Gateway.Get(endpoint);
            var errorResult = CheckErrors(response);
            return CreateResult<TermOfServiceResponse>(response.Content, errorResult);
        }

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/post/spam/@joseph.kalu/test-post-127/info HTTP/1.1
        /// </summary>
        public async Task<OperationResult<Post>> GetPostInfo(PostsInfoRequest request)
        {
            var parameters = new List<RequestParameter>();
            AddLoginParameter(parameters, request.Login);

            var endpoint = string.IsNullOrEmpty(request.Login) ?
                $"post/{request.Url}/info" :
                $"{request.Login}/post/{request.Url}/info";

            var response = await Gateway.Get(endpoint, parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<Post>(response.Content, errorResult);
        }

        public async Task<OperationResult<UserSearchResponse>> SearchUser(SearchWithQueryRequest request, CancellationTokenSource cts = null)
        {
            var parameters = new List<RequestParameter>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            parameters.Add(new RequestParameter { Key = "query", Value = request.Query, Type = ParameterType.QueryString });

            var response = await Gateway.Get("user/search", parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<UserSearchResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<UserExistsResponse>> UserExistsCheck(UserExistsRequests request)
        {
            var endpoint = $"user/{request.Username}/exists";
            var response = await Gateway.Get(endpoint, new List<RequestParameter>());
            var errorResult = CheckErrors(response);
            return CreateResult<UserExistsResponse>(response.Content, errorResult);
        }

        #endregion Get requests

        #region Post requests

        public Task<OperationResult<VoteResponse>> Vote(VoteRequest request)
        {
            return Task.Run(() =>
            {
                var authPost = UrlToAuthorAndPermlink(request.Identifier);
                var op = new VoteOperation(request.Login, authPost.Item1, authPost.Item2, (short)(request.Type == VoteType.upvote ? 10000 : 0));
                var resp = OperationManager.BroadcastOperations(ToKeyArr(request.PostingKey), op);

                var rez = new OperationResult<VoteResponse>();
                if (!resp.IsError)
                {
                    var content = OperationManager.GetContent(authPost.Item1, authPost.Item2);
                    if (!content.IsError)
                    {
                        rez.Result = new VoteResponse { NewTotalPayoutReward = content.Result.NewTotalPayoutReward.Value };
                    }
                }
                else
                {
                    rez.Errors.Add(resp.GetErrorMessage());
                }
                return rez;
            });
        }

        public Task<OperationResult<FollowResponse>> Follow(FollowRequest request)
        {
            return Task.Run(() =>
            {
                var op = request.Type == Steepshot.Core.Models.Requests.FollowType.Follow
                    ? new FollowOperation(request.Login, request.Username, Ditch.Operations.Post.FollowType.Blog, request.Login)
                    : new UnfollowOperation(request.Login, request.Username, request.Login);
                var resp = OperationManager.BroadcastOperations(ToKeyArr(request.PostingKey), op);

                var rez = new OperationResult<FollowResponse>();

                if (resp.IsError)
                {
                    rez.Errors.Add(resp.GetErrorMessage());
                }
                else
                {
                    rez.Result = new FollowResponse();
                }
                return rez;
            });
        }

        public Task<OperationResult<CreateCommentResponse>> CreateComment(CreateCommentRequest request)
        {
            return Task.Run(() =>
            {
                var authPost = UrlToAuthorAndPermlink(request.Url);
                var op = new ReplyOperation(authPost.Item1, authPost.Item2, request.Login, request.Body, "{\"app\": \"steepshot/0.0.5\"}");
                var resp = OperationManager.BroadcastOperations(ToKeyArr(request.PostingKey), op);

                var rez = new OperationResult<CreateCommentResponse>();
                if (resp.IsError)
                    rez.Errors.Add(resp.GetErrorMessage());
                else
                    rez.Result = new CreateCommentResponse(true);
                return rez;
            });
        }

        public Task<OperationResult<ImageUploadResponse>> Upload(UploadImageRequest request)
        {
            return Task.Run(() =>
            {
                var op = new FollowOperation(request.Login, "steepshot", Ditch.Operations.Post.FollowType.Blog, request.Login);
                var tr = OperationManager.CreateTransaction(DynamicGlobalProperties.Default, ToKeyArr(request.PostingKey), op);
                var trx = JsonConvert.SerializeObject(tr, _jsonSerializerSettings);

                var response = Gateway.Upload("post/prepare", request.Title, request.Photo, request.Tags, request.Login, trx);
                var errorResult = CheckErrors(response.Result);

                var rez = new OperationResult<ImageUploadResponse>();
                if (!errorResult.Errors.Any())
                {
                    var upResp = JsonConvert.DeserializeObject<UploadResponce>(response.Result.Content, _jsonSerializerSettings);
                    var meta = JsonConvert.SerializeObject(upResp.Meta);
                    var post = new PostOperation("steepshot", request.Login, upResp.Payload.Title, upResp.Payload.Body, meta);
                    var rez2 = OperationManager.BroadcastOperations(ToKeyArr(request.PostingKey), post);
                    if (rez2.IsError)
                        rez.Errors.Add(rez2.GetErrorMessage());
                    else
                        rez.Result = upResp.Payload;
                }
                return rez;
            });
        }

        public Task<OperationResult<FlagResponse>> Flag(FlagRequest request)
        {
            return Task.Run(() =>
            {
                var rez = new OperationResult<FlagResponse>();

                var authAndPermlink = request.Identifier.Remove(0, request.Identifier.LastIndexOf('@') + 1);
                var authPostArr = authAndPermlink.Split('/');
                if (authPostArr.Length != 2)
                    throw new InvalidCastException($"Unexpected url format: {request.Identifier}");

                var op = new FlagOperation(request.Login, authPostArr[0], authPostArr[1]);
                var resp = OperationManager.BroadcastOperations(ToKeyArr(request.PostingKey), op);

                if (!resp.IsError)
                {
                    var content = OperationManager.GetContent(authPostArr[0], authPostArr[1]);
                    if (!content.IsError)
                    {
                        rez.Result = new FlagResponse { NewTotalPayoutReward = content.Result.NewTotalPayoutReward.Value };
                    }
                }
                else
                {
                    rez.Errors.Add(resp.GetErrorMessage());
                }
                return rez;
            });
        }

        public Task<OperationResult<LoginResponse>> LoginWithPostingKey(LoginWithPostingKeyRequest request)
        {
            return Task.Run(() =>
            {
                var op = new FollowOperation(request.Login, "steepshot", Ditch.Operations.Post.FollowType.Blog, request.Login);
                var responce = OperationManager.VerifyAuthority(ToKeyArr(request.PostingKey), op);
                return !responce.IsError
                    ? new OperationResult<LoginResponse>(new LoginResponse(true))
                    : new OperationResult<LoginResponse>(new List<string> { responce.GetErrorMessage() });
            });
        }

        #endregion Post requests

        private static Tuple<string, string> UrlToAuthorAndPermlink(string url)
        {
            var authAndPermlink = url.Remove(0, url.LastIndexOf('@') + 1);
            var authPostArr = authAndPermlink.Split('/');
            if (authPostArr.Length != 2)
                throw new InvalidCastException($"Unexpected url format: {url}");
            return new Tuple<string, string>(authPostArr[0], authPostArr[1]);
        }

        private void AddOffsetLimitParameters(List<RequestParameter> parameters, string offset, int limit)
        {
            if (!string.IsNullOrWhiteSpace(offset))
                parameters.Add(new RequestParameter { Key = "offset", Value = offset, Type = ParameterType.QueryString });

            if (limit > 0)
                parameters.Add(new RequestParameter { Key = "limit", Value = limit, Type = ParameterType.QueryString });
        }

        private void AddLoginParameter(List<RequestParameter> parameters, string login)
        {
            if (!string.IsNullOrEmpty(login))
                parameters.Add(new RequestParameter { Key = "login", Value = login, Type = ParameterType.QueryString });
        }

        private OperationResult CheckErrors(IRestResponse response)
        {
            var result = new OperationResult();
            var content = response.Content;

            // Network transport or framework errors
            if (response.ErrorException != null)
            {
                result.Errors.Add(response.ErrorMessage);
            }
            // Transport errors
            else if (response.ResponseStatus != ResponseStatus.Completed)
            {
                result.Errors.Add("ResponseStatus: " + response.ResponseStatus);
            }
            // HTTP errors
            else if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Forbidden)
            {
                var dic = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(content, _jsonSerializerSettings);
                foreach (var kvp in dic)
                {
                    result.Errors.AddRange(kvp.Value);
                }
            }
            else if (response.StatusCode != HttpStatusCode.OK &&
                     response.StatusCode != HttpStatusCode.Created)
            {
                result.Errors.Add(response.StatusDescription);
            }

            if (!result.Success)
            {
                // Checking content
                if (string.IsNullOrWhiteSpace(content))
                {
                    result.Errors.Add("Empty response content");
                }
                else if (new Regex(@"<[^>]+>").IsMatch(content))
                {
                    result.Errors.Add("Response content contains HTML : " + content);
                }
                else
                {
                    result.Errors.Add("Response content is not valid");
                }
            }

            return result;
        }

        private OperationResult<T> CreateResult<T>(string json, OperationResult error)
        {
            var result = new OperationResult<T>();

            if (error.Success)
            {
                result.Result = JsonConvert.DeserializeObject<T>(json, _jsonSerializerSettings);
            }
            else
            {
                result.Errors.AddRange(error.Errors);
            }
            return result;
        }

        private List<byte[]> ToKeyArr(string postingKey)
        {
            return new List<byte[]> { Ditch.Helpers.Base58.GetBytes(postingKey) };
        }
    }
}