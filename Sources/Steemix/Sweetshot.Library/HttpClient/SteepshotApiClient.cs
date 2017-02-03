using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RestSharp;
using Sweetshot.Library.Extensions;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;
using Sweetshot.Library.Serializing;

namespace Sweetshot.Library.HttpClient
{
    public class SteepshotApiClient
    {
        private readonly IApiGateway _gateway;
        private readonly IJsonConverter _jsonConverter;

        public SteepshotApiClient(string url)
        {
            _gateway = new ApiGateway(url);
            _jsonConverter = new JsonNetConverter();
        }

        /// <summary>
        ///     Examples: 
        ///     1) POST https://steepshot.org/api/v1/login HTTP/1.1
        ///             {"username":"joseph.kalu","password":"test1234"}
        /// </summary>
        public async Task<OperationResult<LoginResponse>> Login(LoginRequest request)
        {
            return await Authenticate("login", request);
        }

        /// <summary>
        /// </summary>
        public async Task<OperationResult<LoginResponse>> Register(RegisterRequest request)
        {
            return await Authenticate("register", request);
        }

        private async Task<OperationResult<LoginResponse>> Authenticate(string endpoint, LoginRequest request)
        {
            var parameters = new List<RequestParameter>
            {
                new RequestParameter
                {
                    Key = "application/json",
                    Value = _jsonConverter.Serialize(request),
                    Type = ParameterType.RequestBody
                }
            };

            var response = await _gateway.Post(endpoint, parameters);

            var errorResult = CheckErrors(response);
            var result = CreateResult<LoginResponse>(response.Content, errorResult);
            if (result.Success)
            {
                foreach (var cookie in response.Cookies)
                {
                    if (cookie.Name == "sessionid")
                    {
                        result.Result.SessionId = cookie.Value;
                    }
                }

                if (string.IsNullOrWhiteSpace(result.Result.SessionId))
                {
                    result.Success = false;
                    result.Errors.Add("SessionId field is missing.");
                }
            }

            return result;
        }

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/user/joseph.kalu/posts HTTP/1.1
        ///     2) GET https://steepshot.org/api/v1/user/joseph.kalu/posts?offset=%2Fcat1%2F%40joseph.kalu%2Fcat636203389144533548&limit=3 HTTP/1.1
        ///            Cookie: sessionid=q9umzz8q17bclh8yvkkipww3e96dtdn3
        /// </summary>
        public async Task<OperationResult<UserPostResponse>> GetUserPosts(UserPostsRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var parameters2 = CreateOffsetLimitParameters(request.Offset, request.Limit);
            parameters2.AddRange(parameters);

            var response = await _gateway.Get($"/user/{request.Username}/posts", parameters2);
            var errorResult = CheckErrors(response);
            return CreateResult<UserPostResponse>(response.Content, errorResult);
        }

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/recent HTTP/1.1
        ///            Cookie: sessionid=h0loy20ff472dzlmwpafyd6aix07v3q6
        ///     2) GET https://steepshot.org/api/v1/recent?offset=%2Fhealth%2F%40heiditravels%2Fwhat-are-you-putting-on-your-face&limit=3 HTTP/1.1
        ///            Cookie: sessionid=h0loy20ff472dzlmwpafyd6aix07v3q6
        /// </summary>
        public async Task<OperationResult<UserPostResponse>> GetUserRecentPosts(UserRecentPostsRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var parameters2 = CreateOffsetLimitParameters(request.Offset, request.Limit);
            parameters2.AddRange(parameters);

            var response = await _gateway.Get("/recent", parameters2);
            var errorResult = CheckErrors(response);
            return CreateResult<UserPostResponse>(response.Content, errorResult);
        }

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/posts/new HTTP/1.1
        ///     2) GET https://steepshot.org/api/v1/posts/hot HTTP/1.1
        ///     3) GET https://steepshot.org/api/v1/posts/top HTTP/1.1
        ///     4) GET https://steepshot.org/api/v1/posts/top?offset=%2Fsteemit%2F%40heiditravels%2Felevate-your-social-media-experience-with-steemit&limit=3 HTTP/1.1
        /// </summary>
        public async Task<OperationResult<UserPostResponse>> GetPosts(PostsRequest request)
        {
            var parameters = CreateOffsetLimitParameters(request.Offset, request.Limit);

            var endpoint = $"/posts/{request.Type.ToString().ToLowerInvariant()}";
            var response = await _gateway.Get(endpoint, parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<UserPostResponse>(response.Content, errorResult);
        }

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/posts/food/top HTTP/1.1
        ///     2) GET https://steepshot.org/api/v1/posts/food/top?offset=%2Ftravel%2F%40sweetsssj%2Ftravel-with-me-39-my-appointment-with-gulangyu&limit=5 HTTP/1.1
        /// </summary>
        /// 
        public async Task<OperationResult<UserPostResponse>> GetPostsByCategory(PostsByCategoryRequest request)
        {
            var parameters = CreateOffsetLimitParameters(request.Offset, request.Limit);

            var endpoint = $"/posts/{request.Category}/{request.Type.ToString().ToLowerInvariant()}";
            var response = await _gateway.Get(endpoint, parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<UserPostResponse>(response.Content, errorResult);
        }

        /// <summary>
        ///     Examples:
        ///     1) POST https://steepshot.org/api/v1/post/cat1/@joseph.kalu/cat636206825039716128/upvote HTTP/1.1
        ///             Cookie: sessionid=q9umzz8q17bclh8yvkkipww3e96dtdn3
        ///             {"identifier":"/cat1/@joseph.kalu/cat636206825039716128"}
        ///     2) POST https://steepshot.org/api/v1/post//cat1/@joseph.kalu/cat636206825039716128/downvote HTTP/1.1
        ///             Cookie: sessionid=idf14yl65njwggzf41t58bjjiiw2z006
        ///             {"identifier":"/cat1/@joseph.kalu/cat636206825039716128"}
        /// </summary>
        public async Task<OperationResult<VoteResponse>> Vote(VoteRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            parameters.Add(new RequestParameter
            {
                Key = "application/json",
                Value = _jsonConverter.Serialize(request),
                Type = ParameterType.RequestBody
            });

            var endpoint = $"/post/{request.Identifier}/{request.Type.GetDescription()}";
            var response = await _gateway.Post(endpoint, parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<VoteResponse>(response.Content, errorResult);
        }

        /// <summary>
        ///     Examples:
        ///     1) POST https://steepshot.org/api/v1/user/asduj/follow HTTP/1.1
        ///             Cookie: sessionid=neg365kgpokr5kz8sia2eohc854z15od
        ///     2) POST https://steepshot.org/api/v1/user/asduj/unfollow HTTP/1.1
        ///             Cookie: sessionid=mobma1s0mrt7lhwutshrodqcvvbi7vgr
        /// </summary>
        public async Task<OperationResult<FollowResponse>> Follow(FollowRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);

            var endpoint = $"/user/{request.Username}/{request.Type.ToString().ToLowerInvariant()}";
            var response = await _gateway.Post(endpoint, parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<FollowResponse>(response.Content, errorResult);
        }

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/post/@joseph.kalu/cat636203355240074655/comments HTTP/1.1
        /// </summary>
        public async Task<OperationResult<GetCommentResponse>> GetComments(GetCommentsRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);

            var response = await _gateway.Get($"/post/{request.Url}/comments", parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<GetCommentResponse>(response.Content, errorResult);
        }

        /// <summary>
        ///     Examples:
        ///     1) POST https://steepshot.org/api/v1/post/@joseph.kalu/cat636203355240074655/comment HTTP/1.1
        ///             Cookie: sessionid=gyhzep1qsqlbuuqsduji2vkrr2gdcp01
        ///             {"url":"@joseph.kalu/cat636203355240074655","body":"nailed it !","title":"свитшот"}
        /// </summary>
        public async Task<OperationResult<CreateCommentResponse>> CreateComment(CreateCommentRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            parameters.Add(new RequestParameter
            {
                Key = "application/json",
                Value = _jsonConverter.Serialize(request),
                Type = ParameterType.RequestBody
            });

            var response = await _gateway.Post($"/post/{request.Url}/comment", parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<CreateCommentResponse>(response.Content, errorResult);
        }

        /// <summary>
        ///     Examples:
        ///     1) POST https://steepshot.org/api/v1/post HTTP/1.1
        ///             Cookie: sessionid=qps2cjt685or8g5kbyq0ybdti9nzf9ly
        ///             Content-Disposition: form-data; name="title"
        ///             cat636206837437954906
        ///             Content-Disposition: form-data; name="tags"
        ///             cat1
        ///             Content-Disposition: form-data; name="tags"
        ///             cat2
        ///             Content-Disposition: form-data; name="tags"
        ///             cat3
        ///             Content-Disposition: form-data; name="tags"
        ///             cat4
        ///             Content-Disposition: form-data; name="photo"; filename="cat636206837437954906"
        ///             Content-Type: application/octet-stream
        /// </summary>
        public async Task<OperationResult<ImageUploadResponse>> Upload(UploadImageRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);

            var response = await _gateway.Upload("post", request.Title, request.Photo, parameters, request.Tags);
            var errorResult = CheckErrors(response);
            return CreateResult<ImageUploadResponse>(response.Content, errorResult);
        }

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/categories/top HTTP/1.1
        ///     2) GET https://steepshot.org/api/v1/categories/top?offset=food&limit=5 HTTP/1.1
        /// </summary>
        public async Task<OperationResult<CategoriesResponse>> GetCategories(CategoriesRequest request)
        {
            var parameters = CreateOffsetLimitParameters(request.Offset, request.Limit);

            var response = await _gateway.Get("categories/top", parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<CategoriesResponse>(response.Content, errorResult);
        }

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/categories/search?query=foo HTTP/1.1
        ///     2) GET https://steepshot.org/api/v1/categories/search?offset=life&limit=5&query=lif HTTP/1.1
        /// </summary>
        public async Task<OperationResult<CategoriesResponse>> SearchCategories(SearchCategoriesRequest request)
        {
            var parameters = CreateOffsetLimitParameters(request.Offset, request.Limit);
            parameters.Add(
                new RequestParameter {Key = "query", Value = request.Query, Type = ParameterType.QueryString});

            var response = await _gateway.Get("categories/search", parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<CategoriesResponse>(response.Content, errorResult);
        }

        /// <summary>
        ///     Examples:
        ///     1) POST https://steepshot.org/api/v1/user/change-password HTTP/1.1
        ///             Cookie: sessionid=oh3f8vua8a5s2au5ovfqhsi6zvqgjfif
        ///             {"old_password":"test1234","new_password":"test12345"}
        /// </summary>
        public async Task<OperationResult<ChangePasswordResponse>> ChangePassword(ChangePasswordRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            parameters.Add(new RequestParameter
            {
                Key = "application/json",
                Value = _jsonConverter.Serialize(request),
                Type = ParameterType.RequestBody
            });

            var response = await _gateway.Post("user/change-password", parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<ChangePasswordResponse>(response.Content, errorResult);
        }

        /// <summary>
        ///     Examples:
        ///     1) POST https://steepshot.org/api/v1/logout HTTP/1.1
        ///             Cookie: sessionid=rm8haiqibvsvpv7f495mg17sdzje29aw
        /// </summary>
        public async Task<OperationResult<LogoutResponse>> Logout(LogoutRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);

            var response = await _gateway.Post("logout", parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<LogoutResponse>(response.Content, errorResult);
        }

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/user/joseph.kalu/info HTTP/1.1
        /// </summary>
        public async Task<OperationResult<UserProfileResponse>> GetUserProfile(UserProfileRequest request)
        {
            var response = await _gateway.Get($"/user/{request.Username}/info", new List<RequestParameter>());
            var errorResult = CheckErrors(response);
            return CreateResult<UserProfileResponse>(response.Content, errorResult);
        }

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/user/joseph.kalu/following HTTP/1.1
        ///     2) GET https://steepshot.org/api/v1/user/joseph.kalu/followers HTTP/1.1
        ///     3) GET https://steepshot.org/api/v1/user/joseph.kalu/followers?offset=vivianupman&limit=5 HTTP/1.1
        /// </summary>
        public async Task<OperationResult<UserFriendsResponse>> GetUserFriends(UserFriendsRequest request)
        {
            var parameters = CreateOffsetLimitParameters(request.Offset, request.Limit);

            var endpoint = $"/user/{request.Username}/{request.Type.ToString().ToLowerInvariant()}";
            var response = await _gateway.Get(endpoint, parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<UserFriendsResponse>(response.Content, errorResult);
        }

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/tos HTTP/1.1
        /// </summary>
        public async Task<OperationResult<TermOfServiceResponse>> TermsOfService()
        {
            const string endpoint = "/tos";
            var response = await _gateway.Get(endpoint, new List<RequestParameter>());
            var errorResult = CheckErrors(response);
            return CreateResult<TermOfServiceResponse>(response.Content, errorResult);
        }

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/post/spam/@joseph.kalu/test-post-127/info HTTP/1.1
        /// </summary>
        public async Task<OperationResult<Post>> GetPostInfo(PostsInfoRequest request)
        {
            var endpoint = $"/post/{request.Url}/info";
            var response = await _gateway.Get(endpoint, new List<RequestParameter>());
            var errorResult = CheckErrors(response);
            return CreateResult<Post>(response.Content, errorResult);
        }

        private List<RequestParameter> CreateSessionParameter(string sessionId)
        {
            var parameters = new List<RequestParameter>();
            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                parameters.Add(new RequestParameter
                {
                    Key = "sessionid",
                    Value = sessionId,
                    Type = ParameterType.Cookie
                });
            }
            return parameters;
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
            else if (response.StatusCode == HttpStatusCode.BadRequest ||
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
            else
            {
                result.Success = true;
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
                result.Result = _jsonConverter.Deserialize<T>(json);
            }
            else
            {
                result.Errors.AddRange(error.Errors);
                result.Success = false;
            }

            return result;
        }
    }
}