using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using RestSharp.Portable;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Serializing;

namespace Steepshot.Core.HttpClient
{
    public class BaseClient
    {
        private IApiGateway _gateway;
        protected readonly string Url;

        protected IApiGateway Gateway => _gateway ?? (_gateway = new ApiGateway(Url));

        private readonly JsonNetConverter _jsonConverter;

        protected BaseClient(string url)
        {
            Url = url;
            _jsonConverter = new JsonNetConverter();
        }

        protected List<RequestParameter> CreateSessionParameter(string sessionId)
        {
            return new List<RequestParameter>();
        }

        protected List<RequestParameter> CreateOffsetLimitParameters(string offset, int limit)
        {
            var parameters = new List<RequestParameter>();
            if (!string.IsNullOrWhiteSpace(offset))
            {
                parameters.Add(new RequestParameter { Key = "offset", Value = offset, Type = ParameterType.QueryString });
            }
            if (limit > 0)
            {
                parameters.Add(new RequestParameter { Key = "limit", Value = limit, Type = ParameterType.QueryString });
            }
            return parameters;
        }

        protected OperationResult CheckErrors(IRestResponse response)
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

        protected OperationResult<T> CreateResult<T>(string json, OperationResult error)
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


        #region Get requests

        public async Task<OperationResult<UserPostResponse>> GetUserPosts(UserPostsRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var parameters2 = CreateOffsetLimitParameters(request.Offset, request.Limit);
            parameters2.AddRange(parameters);
            var endpoint = $"user/{request.Username}/posts";
            if (!string.IsNullOrWhiteSpace(request.Login)) endpoint = request.Login + "/" + endpoint;

            var response = await Gateway.Get(endpoint, parameters2, cts);
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

            var response = await Gateway.Get(endpoint, parameters2, cts);
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

            var response = await Gateway.Get(endpoint, parameters2, cts);
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

            var response = await Gateway.Get(endpoint, parameters2, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<UserPostResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<SearchResponse<VotersResult>>> GetPostVoters(InfoRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var parameters2 = CreateOffsetLimitParameters(request.Offset, request.Limit);
            parameters2.AddRange(parameters);
            var endpoint = $"post/{request.Url}/voters";

            var response = await Gateway.Get(endpoint, parameters, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<SearchResponse<VotersResult>>(response.Content, errorResult);
        }

        public async Task<OperationResult<GetCommentResponse>> GetComments(InfoRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var endpoint = $"post/{request.Url}/comments";
            if (!string.IsNullOrWhiteSpace(request.Login)) endpoint = request.Login + "/" + endpoint;

            var response = await Gateway.Get(endpoint, parameters, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<GetCommentResponse>(response.Content, errorResult);
        }


        public async Task<OperationResult<UserProfileResponse>> GetUserProfile(UserProfileRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var endpoint = $"user/{request.Username}/info";
            if (!string.IsNullOrWhiteSpace(request.Login)) endpoint = request.Login + "/" + endpoint;

            var response = await Gateway.Get(endpoint, parameters, cts);
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

            var response = await Gateway.Get(endpoint, parameters2, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<UserFriendsResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<TermOfServiceResponse>> TermsOfService(CancellationTokenSource cts)
        {
            const string endpoint = "tos";

            var response = await Gateway.Get(endpoint, new List<RequestParameter>(), cts);
            var errorResult = CheckErrors(response);
            return CreateResult<TermOfServiceResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<Post>> GetPostInfo(InfoRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var endpoint = $"post/{request.Url}/info";
            if (!string.IsNullOrWhiteSpace(request.Login)) endpoint = request.Login + "/" + endpoint;

            var response = await Gateway.Get(endpoint, parameters, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<Post>(response.Content, errorResult);
        }

        public async Task<OperationResult<SearchResponse<UserSearchResult>>> SearchUser(SearchWithQueryRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var parameters2 = CreateOffsetLimitParameters(request.Offset, request.Limit);
            parameters2.AddRange(parameters);
            parameters2.Add(new RequestParameter { Key = "query", Value = request.Query, Type = ParameterType.QueryString });

            var response = await Gateway.Get("user/search", parameters2, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<SearchResponse<UserSearchResult>>(response.Content, errorResult);
        }

        public async Task<OperationResult<UserExistsResponse>> UserExistsCheck(UserExistsRequests request, CancellationTokenSource cts)
        {
            var endpoint = $"user/{request.Username}/exists";

            var response = await Gateway.Get(endpoint, new List<RequestParameter>(), cts);
            var errorResult = CheckErrors(response);
            return CreateResult<UserExistsResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<SearchResponse<SearchResult>>> GetCategories(SearchRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var parameters2 = CreateOffsetLimitParameters(request.Offset, request.Limit);
            parameters2.AddRange(parameters);

            var response = await _gateway.Get("categories/top", parameters2, cts);
            var errorResult = CheckErrors(response);

            var result = CreateResult<SearchResponse<SearchResult>>(response.Content, errorResult);
            if (result.Success)
            {
                foreach (var category in result.Result.Results)
                {
                    category.Name = Ditch.Helpers.Transliteration.ToRus(category.Name);
                }
            }

            return result;
        }


        public async Task<OperationResult<SearchResponse<SearchResult>>> SearchCategories(SearchWithQueryRequest request, CancellationTokenSource cts = null)
        {
            var query = Ditch.Helpers.Transliteration.ToEng(request.Query);
            if (query != request.Query)
            {
                query = $"ru--{query}";
            }
            request.Query = query;

            var parameters = CreateSessionParameter(request.SessionId);
            var parameters2 = CreateOffsetLimitParameters(request.Offset, request.Limit);
            parameters2.AddRange(parameters);
            parameters2.Add(new RequestParameter { Key = "query", Value = request.Query, Type = ParameterType.QueryString });

            var response = await Gateway.Get("categories/search", parameters2, cts);
            var errorResult = CheckErrors(response);
            var result = CreateResult<SearchResponse<SearchResult>>(response.Content, errorResult);

            if (result.Success)
            {
                foreach (var categories in result.Result.Results)
                {
                    categories.Name = Ditch.Helpers.Transliteration.ToRus(categories.Name);
                }
            }

            return result;
        }
        #endregion Get requests


        public async Task<OperationResult<UploadResponse>> UploadWithPrepare(UploadImageRequest request, string trx, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);

            var response = await Gateway.Upload("post/prepare", request.Title, request.Photo, parameters, request.Tags, request.Login, trx, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<UploadResponse>(response.Content, errorResult);
        }
    }
}