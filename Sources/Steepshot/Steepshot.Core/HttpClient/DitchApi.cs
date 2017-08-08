using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ditch;
using Ditch.JsonRpc;
using Ditch.Operations.Get;
using Ditch.Operations.Post;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Serializing;

namespace Steepshot.Core.HttpClient
{
    public class DitchApi : BaseClient, ISteepshotApiClient
    {
        private readonly ISteepshotApiClient _steepshotApi;
        private readonly OperationManager _operationManager;
        private readonly JsonNetConverter _jsonConverter;

        public DitchApi(string url, KnownChains chain)
        {
            _jsonConverter = new JsonNetConverter();
            _steepshotApi = new SteepshotApiClient(url);

            var chainInfo = ChainManager.GetChainInfo(chain == KnownChains.Steem ? Ditch.KnownChains.Steem : Ditch.KnownChains.Golos);
            _operationManager = new OperationManager(chainInfo.Url, chainInfo.ChainId);
        }

        public async Task<OperationResult<LoginResponse>> LoginWithPostingKey(LoginWithPostingKeyRequest request, CancellationTokenSource cts)
        {
            return await Task.Run(() =>
            {
                var op = new FollowOperation(request.Login, "steepshot", Ditch.Operations.Post.FollowType.Blog, request.Login);
                var rpcResponse = _operationManager.VerifyAuthority(ToKeyArr(request.PostingKey), op);
                
                var result = new OperationResult<LoginResponse>();
                if (rpcResponse.IsError)
                {
                    result.Result = new LoginResponse {Message = "User was logged in."};
                }
                else
                {
                    result.Errors.Add(rpcResponse.GetErrorMessage());
                }
                return result;
            });
        }

        public async Task<OperationResult<UserPostResponse>> GetUserPosts(UserPostsRequest request, CancellationTokenSource cts)
        {
            return await _steepshotApi.GetUserPosts(request, cts);
        }

        public async Task<OperationResult<UserPostResponse>> GetUserRecentPosts(UserRecentPostsRequest request, CancellationTokenSource cts)
        {
            return await _steepshotApi.GetUserRecentPosts(request, cts);
        }

        public async Task<OperationResult<UserPostResponse>> GetPosts(PostsRequest request, CancellationTokenSource cts)
        {
            return await _steepshotApi.GetPosts(request, cts);
        }

        public async Task<OperationResult<UserPostResponse>> GetPostsByCategory(PostsByCategoryRequest request, CancellationTokenSource cts)
        {
            return await _steepshotApi.GetPostsByCategory(request, cts);
        }

        public async Task<OperationResult<SearchResponse<VotersResult>>> GetPostVoters(InfoRequest request, CancellationTokenSource cts)
        {
            return await _steepshotApi.GetPostVoters(request, cts);
        }

        public async Task<OperationResult<VoteResponse>> Vote(VoteRequest request, CancellationTokenSource cts)
        {
            return await Task.Run(() =>
            {
                var authPost = UrlToAuthorAndPermlink(request.Identifier);
                var op = new VoteOperation(request.Login, authPost.Item1, authPost.Item2, (short) (request.Type == VoteType.Up ? 10000 : 0));
                var response = _operationManager.BroadcastOperations(ToKeyArr(request.SessionId), op);

                var result = new OperationResult<VoteResponse>();
                if (!response.IsError)
                {
                    var content = _operationManager.GetContent(authPost.Item1, authPost.Item2);
                    if (!content.IsError)
                    {
                        //Convert Money type to double
                        result.Result = new VoteResponse
                        {
                            NewTotalPayoutReward = new Models.Money(content.Result.NewTotalPayoutReward.ToString())
                        };
                    }
                }
                else
                {
                    result.Errors.Add(response.GetErrorMessage());
                }
                return result;
            });
        }

        public async Task<OperationResult<FollowResponse>> Follow(FollowRequest request, CancellationTokenSource cts)
        {
            return await Task.Run(() =>
            {
                var op = request.Type == Models.Requests.FollowType.Follow
                    ? new FollowOperation(request.Login, request.Username, Ditch.Operations.Post.FollowType.Blog, request.Login)
                    : new UnfollowOperation(request.Login, request.Username, request.Login);
                
                var response = _operationManager.BroadcastOperations(ToKeyArr(request.SessionId), op);

                var result = new OperationResult<FollowResponse>();
                if (response.IsError)
                {
                    result.Errors.Add(response.GetErrorMessage());
                }
                else
                {
                    result.Result = new FollowResponse();
                }
                return result;
            });
        }
        
        public async Task<OperationResult<GetCommentResponse>> GetComments(InfoRequest request, CancellationTokenSource cts)
        {
            return await _steepshotApi.GetComments(request, cts);
        }

        public async Task<OperationResult<CreateCommentResponse>> CreateComment(CreateCommentRequest request, CancellationTokenSource cts)
        {
            return await Task.Run(() =>
            {
                var authPost = UrlToAuthorAndPermlink(request.Url);
                var op = new ReplyOperation(authPost.Item1, authPost.Item2, request.Login, request.Body, "{\"app\": \"steepshot/0.0.5\"}");
                
                var response = _operationManager.BroadcastOperations(ToKeyArr(request.SessionId), op);

                var result = new OperationResult<CreateCommentResponse>();
                if (response.IsError)
                {
                    result.Errors.Add(response.GetErrorMessage());
                }
                else
                {
                    result.Result = new CreateCommentResponse {Message = "Comment created"};
                }
                return result;
            });
        }

        public async Task<OperationResult<ImageUploadResponse>> Upload(UploadImageRequest request, CancellationTokenSource cts)
        {
            return await Task.Run(async () =>
            {
                var op = new FollowOperation(request.Login, "steepshot", Ditch.Operations.Post.FollowType.Blog, request.Login);
                var tr = _operationManager.CreateTransaction(DynamicGlobalProperties.Default, ToKeyArr(request.SessionId), op);

                var trx = _jsonConverter.Serialize(tr);
                Ditch.Helpers.Transliteration.PrepareTags(request.Tags.ToArray());

                var result = new OperationResult<ImageUploadResponse>();

                var uploadResponse = await _steepshotApi.UploadWithPrepare(request, request.Login, trx, cts);
                if (uploadResponse.Success)
                {
                    var meta = _jsonConverter.Serialize(uploadResponse.Result.Meta);
                    var post = new PostOperation("steepshot", request.Login, uploadResponse.Result.Payload.Title, uploadResponse.Result.Payload.Body, meta);

                    var response = _operationManager.BroadcastOperations(ToKeyArr(request.SessionId), post);
                    if (response.IsError)
                    {
                        result.Errors.Add(response.GetErrorMessage());
                    }
                    else
                    {
                        result.Result = uploadResponse.Result.Payload;
                    }
                }

                return result;
            });
        }

        public Task<OperationResult<UploadResponse>> UploadWithPrepare(UploadImageRequest request, string username, string trx, CancellationTokenSource cts = null)
        {
            throw new NotImplementedException();
        }

        public async Task<OperationResult<SearchResponse<SearchResult>>> GetCategories(SearchRequest request, CancellationTokenSource cts = null)
        {
            var result = await _steepshotApi.GetCategories(request, cts);
            return await Task.Run(() =>
            {
                if (result.Success)
                {
                    foreach (var category in result.Result.Results)
                    {
                        category.Name = Ditch.Helpers.Transliteration.ToRus(category.Name);
                    }
                }

                return result;
            });
        }

        public async Task<OperationResult<SearchResponse<SearchResult>>> SearchCategories(SearchWithQueryRequest request, CancellationTokenSource cts = null)
        {
            var query = Ditch.Helpers.Transliteration.ToEng(request.Query);
            if (query != request.Query)
            {
                query = $"ru--{query}";
            }
            request.Query = query;

            var result = await _steepshotApi.SearchCategories(request, cts);
            if (result.Success)
            {
                foreach (var categories in result.Result.Results)
                {
                    categories.Name = Ditch.Helpers.Transliteration.ToRus(categories.Name);
                }
            }

            return result;
        }

        public async Task<OperationResult<LogoutResponse>> Logout(LogoutRequest request, CancellationTokenSource cts)
        {
            return await Task.Run(() => new OperationResult<LogoutResponse>
            {
                Result = new LogoutResponse {Message = "User is logged out"}
            });
        }

        public async Task<OperationResult<UserProfileResponse>> GetUserProfile(UserProfileRequest request, CancellationTokenSource cts)
        {
            return await _steepshotApi.GetUserProfile(request, cts);
        }

        public async Task<OperationResult<UserFriendsResponse>> GetUserFriends(UserFriendsRequest request, CancellationTokenSource cts)
        {
            return await _steepshotApi.GetUserFriends(request, cts);
        }

        public async Task<OperationResult<TermOfServiceResponse>> TermsOfService(CancellationTokenSource cts)
        {
            return await _steepshotApi.TermsOfService(cts);
        }

        public async Task<OperationResult<Post>> GetPostInfo(InfoRequest request, CancellationTokenSource cts)
        {
            return await _steepshotApi.GetPostInfo(request, cts);
        }

        public async Task<OperationResult<SearchResponse<UserSearchResult>>> SearchUser(SearchWithQueryRequest request, CancellationTokenSource cts)
        {
            return await _steepshotApi.SearchUser(request, cts);
        }

        public async Task<OperationResult<UserExistsResponse>> UserExistsCheck(UserExistsRequests request, CancellationTokenSource cts)
        {
            return await _steepshotApi.UserExistsCheck(request, cts);
        }

        public async Task<OperationResult<FlagResponse>> Flag(FlagRequest request, CancellationTokenSource cts)
        {
            return await Task.Run(() =>
            {
                var result = new OperationResult<FlagResponse>();

                var authAndPermlink = request.Identifier.Remove(0, request.Identifier.LastIndexOf('@') + 1);
                var authPostArr = authAndPermlink.Split('/');
                if (authPostArr.Length != 2)
                {
                    throw new InvalidCastException($"Unexpected url format: {request.Identifier}");
                }

                var op = new FlagOperation(request.Login, authPostArr[0], authPostArr[1]);
                var response = _operationManager.BroadcastOperations(ToKeyArr(request.SessionId), op);

                if (!response.IsError)
                {
                    var content = _operationManager.GetContent(authPostArr[0], authPostArr[1]);
                    if (!content.IsError)
                    {
                        result.Result = new FlagResponse
                        {
                            NewTotalPayoutReward = content.Result.NewTotalPayoutReward.Value
                        };
                    }
                }
                else
                {
                    result.Errors.Add(response.GetErrorMessage());
                }
                return result;
            });
        }

        private Tuple<string, string> UrlToAuthorAndPermlink(string url)
        {
            var authAndPermlink = url.Remove(0, url.LastIndexOf('@') + 1);
            var authPostArr = authAndPermlink.Split('/');
            if (authPostArr.Length != 2) throw new InvalidCastException($"Unexpected url format: {url}");
            return new Tuple<string, string>(authPostArr[0], authPostArr[1]);
        }

        private IEnumerable<byte[]> ToKeyArr(string postingKey)
        {
            return new List<byte[]> {Ditch.Helpers.Base58.GetBytes(postingKey)};
        }
    }
}