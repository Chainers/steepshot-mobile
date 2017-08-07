using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RestSharp.Portable;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.HttpClient
{
    public class SteepshotApiClient : BaseClient, ISteepshotApiClient
    {
        private readonly IApiGateway _gateway;

        public SteepshotApiClient(string url)
        {
            _gateway = new ApiGateway(url);
        }

        public async Task<OperationResult<LoginResponse>> LoginWithPostingKey(LoginWithPostingKeyRequest request, CancellationTokenSource cts)
        {
            var parameters = new List<RequestParameter>
            {
                new RequestParameter
                {
                    Key = "application/json",
                    Value = request,
                    Type = ParameterType.RequestBody
                }
            };
            var endpoint = $"login-with-posting";
            
            var response = await _gateway.Post(endpoint, parameters, cts);

            var errorResult = CheckErrors(response);
            var result = CreateResult<LoginResponse>(response.Content, errorResult);
            if (result.Success)
            {
                foreach (var cookie in response.Headers.GetValues("Set-Cookie"))
                {
                    if (cookie.StartsWith("sessionid"))
                    {
                        result.Result.SessionId = cookie.Split(';').First().Split('=').Last();
                    }
                }

                if (string.IsNullOrWhiteSpace(result.Result.SessionId))
                {
                    result.Errors.Add("SessionId field is missing.");
                }
            }

            return result;
        }

        public async Task<OperationResult<UserPostResponse>> GetUserPosts(UserPostsRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var parameters2 = CreateOffsetLimitParameters(request.Offset, request.Limit);
            parameters2.AddRange(parameters);
            var endpoint = $"user/{request.Username}/posts";
            if (!string.IsNullOrWhiteSpace(request.Login)) endpoint = request.Login + "/" + endpoint;

            var response = await _gateway.Get(endpoint, parameters2, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<UserPostResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<UserPostResponse>> GetUserRecentPosts(UserRecentPostsRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var parameters2 = CreateOffsetLimitParameters(request.Offset, request.Limit);
            parameters2.AddRange(parameters);
            var endpoint = "recent";
            if (!string.IsNullOrWhiteSpace(request.Login)) endpoint = request.Login + "/" + endpoint; 
            
            var response = await _gateway.Get(endpoint, parameters2, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<UserPostResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<UserPostResponse>> GetPosts(PostsRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var parameters2 = CreateOffsetLimitParameters(request.Offset, request.Limit);
            parameters2.AddRange(parameters);
            var endpoint = $"posts/{request.Type.ToString().ToLowerInvariant()}";
            if (!string.IsNullOrWhiteSpace(request.Login)) endpoint = request.Login + "/" + endpoint; 
            
            var response = await _gateway.Get(endpoint, parameters2, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<UserPostResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<UserPostResponse>> GetPostsByCategory(PostsByCategoryRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var parameters2 = CreateOffsetLimitParameters(request.Offset, request.Limit);
            parameters2.AddRange(parameters);
            var endpoint = $"posts/{request.Category}/{request.Type.ToString().ToLowerInvariant()}";
            if (!string.IsNullOrWhiteSpace(request.Login)) endpoint = request.Login + "/" + endpoint; 
            
            var response = await _gateway.Get(endpoint, parameters2, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<UserPostResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<SearchResponse<VotersResult>>> GetPostVoters(InfoRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var parameters2 = CreateOffsetLimitParameters(request.Offset, request.Limit);
            parameters2.AddRange(parameters);
            var endpoint = $"post/{request.Url}/voters";
            
            var response = await _gateway.Get(endpoint, parameters, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<SearchResponse<VotersResult>>(response.Content, errorResult);
        }

        public async Task<OperationResult<VoteResponse>> Vote(VoteRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            parameters.Add(new RequestParameter
            {
                Key = "application/json",
                Value = request,
                Type = ParameterType.RequestBody
            });
            var endpoint = $"post/{request.Identifier}/{request.Type.GetDescription()}";
            
            var response = await _gateway.Post(endpoint, parameters, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<VoteResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<FollowResponse>> Follow(FollowRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var endpoint = $"user/{request.Username}/{request.Type.ToString().ToLowerInvariant()}";
            
            var response = await _gateway.Post(endpoint, parameters, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<FollowResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<GetCommentResponse>> GetComments(InfoRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var endpoint = $"post/{request.Url}/comments";
            if (!string.IsNullOrWhiteSpace(request.Login)) endpoint = request.Login + "/" + endpoint; 
            
            var response = await _gateway.Get(endpoint, parameters, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<GetCommentResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<CreateCommentResponse>> CreateComment(CreateCommentRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            parameters.Add(new RequestParameter
            {
                Key = "application/json",
                Value = request,
                Type = ParameterType.RequestBody
            });
            var endpoint = $"post/{request.Url}/comment";
            
            var response = await _gateway.Post(endpoint, parameters, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<CreateCommentResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<ImageUploadResponse>> Upload(UploadImageRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var endpoint = $"post";
            if (!string.IsNullOrWhiteSpace(request.Login)) endpoint = endpoint + "/" + request.Login; // TODO Fuuuuuck, shitty code.
            
            var response = await _gateway.Upload(endpoint, request.Title, request.Photo, parameters, request.Tags, request.Username, request.Trx, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<ImageUploadResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<SearchResponse<SearchResult>>> GetCategories(SearchRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var parameters2 = CreateOffsetLimitParameters(request.Offset, request.Limit);
            parameters2.AddRange(parameters);
            var endpoint = $"categories/top";
            
            var response = await _gateway.Get(endpoint, parameters2, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<SearchResponse<SearchResult>>(response.Content, errorResult);
        }

        public async Task<OperationResult<SearchResponse<SearchResult>>> SearchCategories(SearchWithQueryRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var parameters2 = CreateOffsetLimitParameters(request.Offset, request.Limit);
            parameters2.AddRange(parameters);
            parameters2.Add(new RequestParameter {Key = "query", Value = request.Query, Type = ParameterType.QueryString});
            var endpoint = $"categories/search";
            
            var response = await _gateway.Get(endpoint, parameters2, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<SearchResponse<SearchResult>>(response.Content, errorResult);
        }

        public async Task<OperationResult<LogoutResponse>> Logout(LogoutRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var endpoint = $"logout";
            
            var response = await _gateway.Post(endpoint, parameters, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<LogoutResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<UserProfileResponse>> GetUserProfile(UserProfileRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var endpoint = $"user/{request.Username}/info";
            if (!string.IsNullOrWhiteSpace(request.Login)) endpoint = request.Login + "/" + endpoint; 
            
            var response = await _gateway.Get(endpoint, parameters, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<UserProfileResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<UserFriendsResponse>> GetUserFriends(UserFriendsRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var parameters2 = CreateOffsetLimitParameters(request.Offset, request.Limit);
            parameters2.AddRange(parameters);
            var endpoint = $"user/{request.Username}/{request.Type.ToString().ToLowerInvariant()}";
            if (!string.IsNullOrWhiteSpace(request.Login)) endpoint = request.Login + "/" + endpoint;
            
            var response = await _gateway.Get(endpoint, parameters2, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<UserFriendsResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<TermOfServiceResponse>> TermsOfService(CancellationTokenSource cts)
        {
            const string endpoint = "tos";
            
            var response = await _gateway.Get(endpoint, new List<RequestParameter>(), cts);
            var errorResult = CheckErrors(response);
            return CreateResult<TermOfServiceResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<Post>> GetPostInfo(InfoRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var endpoint = $"post/{request.Url}/info";
            if (!string.IsNullOrWhiteSpace(request.Login)) endpoint = request.Login + "/" + endpoint;
            
            var response = await _gateway.Get(endpoint, parameters, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<Post>(response.Content, errorResult);
        }

        public async Task<OperationResult<SearchResponse<UserSearchResult>>> SearchUser(SearchWithQueryRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var parameters2 = CreateOffsetLimitParameters(request.Offset, request.Limit);
            parameters2.AddRange(parameters);
            parameters2.Add(new RequestParameter {Key = "query", Value = request.Query, Type = ParameterType.QueryString});
            var endpoint = $"user/search";
            
            var response = await _gateway.Get(endpoint, parameters2, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<SearchResponse<UserSearchResult>>(response.Content, errorResult);
        }

        public async Task<OperationResult<UserExistsResponse>> UserExistsCheck(UserExistsRequests request, CancellationTokenSource cts)
        {
            var endpoint = $"user/{request.Username}/exists";
            
            var response = await _gateway.Get(endpoint, new List<RequestParameter>(), cts);
            var errorResult = CheckErrors(response);
            return CreateResult<UserExistsResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<FlagResponse>> Flag(FlagRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            parameters.Add(new RequestParameter
            {
                Key = "application/json",
                Value = request,
                Type = ParameterType.RequestBody
            });
            var endpoint = $"post/{request.Identifier}/{request.Type.GetDescription()}";
            
            var response = await _gateway.Post(endpoint, parameters, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<FlagResponse>(response.Content, errorResult);
        }
    }
}