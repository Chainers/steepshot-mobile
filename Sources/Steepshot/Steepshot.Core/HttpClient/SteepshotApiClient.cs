using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RestSharp.Portable;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Serializing;

namespace Steepshot.Core.HttpClient
{
    public class SteepshotApiClient : ISteepshotApiClient
    {
        private readonly IApiGateway _gateway;
        private readonly IJsonConverter _jsonConverter;

        public SteepshotApiClient(string url)
        {
            _gateway = new ApiGateway(url);
            _jsonConverter = new JsonNetConverter();
        }

        public async Task<OperationResult<LoginResponse>> LoginWithPostingKey(LoginWithPostingKeyRequest request)
        {
            return await Authenticate("login-with-posting", request);
        }

        private async Task<OperationResult<LoginResponse>> Authenticate(string endpoint, ILoginRequest request)
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

            var response = await _gateway.Post(endpoint, parameters);

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

        public async Task<OperationResult<UserPostResponse>> GetUserPosts(UserPostsRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var parameters2 = CreateOffsetLimitParameters(request.Offset, request.Limit);
            parameters2.AddRange(parameters);

            var response = await _gateway.Get($"user/{request.Username}/posts", parameters2);
            var errorResult = CheckErrors(response);
            return CreateResult<UserPostResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<UserPostResponse>> GetUserRecentPosts(UserRecentPostsRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var parameters2 = CreateOffsetLimitParameters(request.Offset, request.Limit);
            parameters2.AddRange(parameters);

            var response = await _gateway.Get("recent", parameters2);
            var errorResult = CheckErrors(response);
            return CreateResult<UserPostResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<UserPostResponse>> GetPosts(PostsRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var parameters2 = CreateOffsetLimitParameters(request.Offset, request.Limit);
            parameters2.AddRange(parameters);

            var endpoint = $"posts/{request.Type.ToString().ToLowerInvariant()}";
            var response = await _gateway.Get(endpoint, parameters2);
            var errorResult = CheckErrors(response);
            return CreateResult<UserPostResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<UserPostResponse>> GetPostsByCategory(PostsByCategoryRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var parameters2 = CreateOffsetLimitParameters(request.Offset, request.Limit);
            parameters2.AddRange(parameters);

            var endpoint = $"posts/{request.Category}/{request.Type.ToString().ToLowerInvariant()}";
            var response = await _gateway.Get(endpoint, parameters2);
            var errorResult = CheckErrors(response);
            return CreateResult<UserPostResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<SearchResponse<VotersResult>>> GetPostVoters(InfoRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            //var parameters2 = CreateOffsetLimitParameters(request.Offset, request.Limit);
            //parameters2.AddRange(parameters);

            var endpoint = $"post/{request.Url}/voters";
            var response = await _gateway.Get(endpoint, parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<SearchResponse<VotersResult>>(response.Content, errorResult);
        }

        public async Task<OperationResult<VoteResponse>> Vote(VoteRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            parameters.Add(new RequestParameter
            {
                Key = "application/json",
                Value = request,
                Type = ParameterType.RequestBody
            });

            var endpoint = $"post/{request.Identifier}/{request.Type.GetDescription()}";
            var response = await _gateway.Post(endpoint, parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<VoteResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<FollowResponse>> Follow(FollowRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);

            var endpoint = $"user/{request.Username}/{request.Type.ToString().ToLowerInvariant()}";
            var response = await _gateway.Post(endpoint, parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<FollowResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<GetCommentResponse>> GetComments(InfoRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var response = await _gateway.Get($"post/{request.Url}/comments", parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<GetCommentResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<CreateCommentResponse>> CreateComment(CreateCommentRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            parameters.Add(new RequestParameter
            {
                Key = "application/json",
                Value = request,
                Type = ParameterType.RequestBody
            });

            var response = await _gateway.Post($"post/{request.Url}/comment", parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<CreateCommentResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<ImageUploadResponse>> Upload(UploadImageRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);

            var response = await _gateway.Upload("post", request.Title, request.Photo, parameters, request.Tags);
            var errorResult = CheckErrors(response);
            return CreateResult<ImageUploadResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<SearchResponse<SearchResult>>> GetCategories(SearchRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var parameters2 = CreateOffsetLimitParameters(request.Offset, request.Limit);
            parameters2.AddRange(parameters);

            var response = await _gateway.Get("categories/top", parameters2);
            var errorResult = CheckErrors(response);
            return CreateResult<SearchResponse<SearchResult>>(response.Content, errorResult);
        }

        public async Task<OperationResult<SearchResponse<SearchResult>>> SearchCategories(SearchWithQueryRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var parameters2 = CreateOffsetLimitParameters(request.Offset, request.Limit);
            parameters2.AddRange(parameters);
            parameters2.Add(new RequestParameter {Key = "query", Value = request.Query, Type = ParameterType.QueryString});

            var response = await _gateway.Get("categories/search", parameters2);
            var errorResult = CheckErrors(response);
            return CreateResult<SearchResponse<SearchResult>>(response.Content, errorResult);
        }

        public async Task<OperationResult<LogoutResponse>> Logout(LogoutRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);

            var response = await _gateway.Post("logout", parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<LogoutResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<UserProfileResponse>> GetUserProfile(UserProfileRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);

            var response = await _gateway.Get($"user/{request.Username}/info", parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<UserProfileResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<UserFriendsResponse>> GetUserFriends(UserFriendsRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var parameters2 = CreateOffsetLimitParameters(request.Offset, request.Limit);
            parameters2.AddRange(parameters);

            var endpoint = $"user/{request.Username}/{request.Type.ToString().ToLowerInvariant()}";
            var response = await _gateway.Get(endpoint, parameters2);
            var errorResult = CheckErrors(response);
            return CreateResult<UserFriendsResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<TermOfServiceResponse>> TermsOfService()
        {
            const string endpoint = "tos";
            var response = await _gateway.Get(endpoint, new List<RequestParameter>());
            var errorResult = CheckErrors(response);
            return CreateResult<TermOfServiceResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<Post>> GetPostInfo(InfoRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);

            var endpoint = $"post/{request.Url}/info";
            var response = await _gateway.Get(endpoint, parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<Post>(response.Content, errorResult);
        }

        public async Task<OperationResult<SearchResponse<UserSearchResult>>> SearchUser(SearchWithQueryRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var parameters2 = CreateOffsetLimitParameters(request.Offset, request.Limit);
            parameters2.AddRange(parameters);
            parameters2.Add(new RequestParameter {Key = "query", Value = request.Query, Type = ParameterType.QueryString});

            var response = await _gateway.Get("user/search", parameters2);
            var errorResult = CheckErrors(response);
            return CreateResult<SearchResponse<UserSearchResult>>(response.Content, errorResult);
        }

        public async Task<OperationResult<UserExistsResponse>> UserExistsCheck(UserExistsRequests request)
        {
            var endpoint = $"user/{request.Username}/exists";
            var response = await _gateway.Get(endpoint, new List<RequestParameter>());
            var errorResult = CheckErrors(response);
            return CreateResult<UserExistsResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<FlagResponse>> Flag(FlagRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            parameters.Add(new RequestParameter
            {
                Key = "application/json",
                Value = request,
                Type = ParameterType.RequestBody
            });

            var endpoint = $"post/{request.Identifier}/{request.Type.GetDescription()}";
            var response = await _gateway.Post(endpoint, parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<FlagResponse>(response.Content, errorResult);
        }

        private List<RequestParameter> CreateSessionParameter(string sessionId)
        {
            return new List<RequestParameter>();
        }

        private List<RequestParameter> CreateOffsetLimitParameters(string offset, int limit)
        {
            var parameters = new List<RequestParameter>();
            if (!string.IsNullOrWhiteSpace(offset))
            {
                parameters.Add(new RequestParameter {Key = "offset", Value = offset, Type = ParameterType.QueryString});
            }
            if (limit > 0)
            {
                parameters.Add(new RequestParameter {Key = "limit", Value = limit, Type = ParameterType.QueryString});
            }
            return parameters;
        }

        private OperationResult CheckErrors(IRestResponse response)
        {
            var result = new OperationResult();
            var content = response.Content;

            // HTTP errors
            if (response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.Forbidden)
            {
                var dic = _jsonConverter.Deserialize<Dictionary<string, List<string>>>(content);
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
            }

            return result;
        }

        private OperationResult<T> CreateResult<T>(string json, OperationResult error)
        {
            var result = new OperationResult<T>();

            if (error.Success)
            {
                result.Result = _jsonConverter.Deserialize<T>(json);
            }
            else
            {
                result.Errors.AddRange(error.Errors);
            }

            return result;
        }
    }
}