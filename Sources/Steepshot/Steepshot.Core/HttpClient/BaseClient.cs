using System;
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
        protected volatile bool EnableRead;
        protected ApiGateway Gateway;

        protected CancellationTokenSource CtsMain;
        protected readonly JsonNetConverter _jsonConverter;

        protected BaseClient()
        {
            _jsonConverter = new JsonNetConverter();
            CtsMain = new CancellationTokenSource();
            //Gateway = new ApiGateway();
        }

        #region Get requests

        public async Task<OperationResult<UserPostResponse>> GetUserPosts(UserPostsRequest request, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            AddLoginParameter(parameters, request.Login);
            AddCensorParameters(parameters, request);

            var endpoint = $"user/{request.Username}/posts";

            var response = await Gateway.Get(GatewayVersion.V1P1, endpoint, parameters, ct);
            var errorResult = CheckErrors(response);

            return CreateResult<UserPostResponse>(response?.Content, errorResult);
        }

        public async Task<OperationResult<UserPostResponse>> GetUserRecentPosts(CensoredNamedRequestWithOffsetLimitFields request, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            AddLoginParameter(parameters, request.Login);
            AddCensorParameters(parameters, request);

            var endpoint = "recent";

            var response = await Gateway.Get(GatewayVersion.V1P1, endpoint, parameters, ct);
            var errorResult = CheckErrors(response);
            return CreateResult<UserPostResponse>(response?.Content, errorResult);
        }

        public async Task<OperationResult<UserPostResponse>> GetPosts(PostsRequest request, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            AddLoginParameter(parameters, request.Login);
            AddCensorParameters(parameters, request);

            var endpoint = $"posts/{request.Type.ToString().ToLowerInvariant()}";

            var response = await Gateway.Get(GatewayVersion.V1P1, endpoint, parameters, ct);
            var errorResult = CheckErrors(response);

            return CreateResult<UserPostResponse>(response?.Content, errorResult);
        }

        public async Task<OperationResult<UserPostResponse>> GetPostsByCategory(PostsByCategoryRequest request, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            AddLoginParameter(parameters, request.Login);
            AddCensorParameters(parameters, request);

            var endpoint = $"posts/{request.Category}/{request.Type.ToString().ToLowerInvariant()}";

            var response = await Gateway.Get(GatewayVersion.V1P1, endpoint, parameters, ct);
            var errorResult = CheckErrors(response);

            return CreateResult<UserPostResponse>(response?.Content, errorResult);
        }

        public async Task<OperationResult<SearchResponse<UserFriend>>> GetPostVoters(InfoRequest request, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            if (!string.IsNullOrEmpty(request.Login))
                AddLoginParameter(parameters, request.Login);

            var endpoint = $"post/{request.Url}/voters";

            var response = await Gateway.Get(GatewayVersion.V1P1, endpoint, parameters, ct);
            var errorResult = CheckErrors(response);

            return CreateResult<SearchResponse<UserFriend>>(response?.Content, errorResult);
        }

        public async Task<OperationResult<UserPostResponse>> GetComments(NamedInfoRequest request, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            AddLoginParameter(parameters, request.Login);

            var endpoint = $"post/{request.Url}/comments";

            var response = await Gateway.Get(GatewayVersion.V1P1, endpoint, parameters, ct);
            var errorResult = CheckErrors(response);

            return CreateResult<UserPostResponse>(response?.Content, errorResult);
        }

        public async Task<OperationResult<UserProfileResponse>> GetUserProfile(UserProfileRequest request, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var parameters = new Dictionary<string, object>();
            AddLoginParameter(parameters, request.Login);
            parameters.Add("show_nsfw", Convert.ToInt32(request.ShowNsfw));
            parameters.Add("show_low_rated", Convert.ToInt32(request.ShowLowRated));

            var endpoint = $"user/{request.Username}/info";

            var response = await Gateway.Get(GatewayVersion.V1P1, endpoint, parameters, ct);
            var errorResult = CheckErrors(response);

            return CreateResult<UserProfileResponse>(response?.Content, errorResult);
        }

        public async Task<OperationResult<UserFriendsResponse>> GetUserFriends(UserFriendsRequest request, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            AddLoginParameter(parameters, request.Login);

            var endpoint = $"user/{request.Username}/{request.Type.ToString().ToLowerInvariant()}";

            var response = await Gateway.Get(GatewayVersion.V1P1, endpoint, parameters, ct);
            var errorResult = CheckErrors(response);

            return CreateResult<UserFriendsResponse>(response?.Content, errorResult);
        }

        public async Task<OperationResult<Post>> GetPostInfo(NamedInfoRequest request, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var parameters = new Dictionary<string, object>();
            AddLoginParameter(parameters, request.Login);
            AddCensorParameters(parameters, request);

            var endpoint = $"post/{request.Url}/info";

            var response = await Gateway.Get(GatewayVersion.V1P1, endpoint, parameters, ct);
            var errorResult = CheckErrors(response);

            return CreateResult<Post>(response?.Content, errorResult);
        }

        public async Task<OperationResult<SearchResponse<UserFriend>>> SearchUser(SearchWithQueryRequest request, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var parameters = new Dictionary<string, object>();
            AddLoginParameter(parameters, request.Login);
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            parameters.Add("query", request.Query);

            var endpoint = "user/search";

            var response = await Gateway.Get(GatewayVersion.V1P1, endpoint, parameters, ct);
            var errorResult = CheckErrors(response);

            return CreateResult<SearchResponse<UserFriend>>(response?.Content, errorResult);
        }

        public async Task<OperationResult<UserExistsResponse>> UserExistsCheck(UserExistsRequests request, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var parameters = new Dictionary<string, object>();
            var endpoint = $"user/{request.Username}/exists";

            var response = await Gateway.Get(GatewayVersion.V1, endpoint, parameters, ct);
            var errorResult = CheckErrors(response);

            return CreateResult<UserExistsResponse>(response?.Content, errorResult);
        }

        public async Task<OperationResult<SearchResponse<SearchResult>>> GetCategories(OffsetLimitFields request, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            var endpoint = "categories/top";

            var response = await Gateway.Get(GatewayVersion.V1, endpoint, parameters, ct);
            var errorResult = CheckErrors(response);

            var result = CreateResult<SearchResponse<SearchResult>>(response?.Content, errorResult);
            if (result.Success)
            {
                foreach (var category in result.Result.Results)
                {
                    category.Name = Ditch.Helpers.Transliteration.ToRus(category.Name);
                }
            }

            return result;
        }

        public async Task<OperationResult<SearchResponse<SearchResult>>> SearchCategories(SearchWithQueryRequest request, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var query = Ditch.Helpers.Transliteration.ToEng(request.Query);
            if (query != request.Query)
            {
                query = $"ru--{query}";
            }
            request.Query = query;

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            parameters.Add("query", request.Query);
            var endpoint = "categories/search";

            var response = await Gateway.Get(GatewayVersion.V1, endpoint, parameters, ct);
            var errorResult = CheckErrors(response);

            var result = CreateResult<SearchResponse<SearchResult>>(response?.Content, errorResult);

            if (result.Success)
            {
                foreach (var categories in result.Result.Results)
                {
                    categories.Name = Ditch.Helpers.Transliteration.ToRus(categories.Name);
                }
            }

            return result;
        }

        protected async Task Trace(string endpoint, string login, List<string> resultErrors, string target, CancellationToken ct)
        {
            if (!EnableRead)
                return;

            try
            {
                var parameters = new Dictionary<string, object>();
                AddLoginParameter(parameters, login);
                parameters.Add("error", resultErrors == null ? string.Empty : string.Join(Environment.NewLine, resultErrors));
                if (!string.IsNullOrEmpty(target))
                    parameters.Add("target", target);
                await Gateway.Post(GatewayVersion.V1, $@"log/{endpoint}", parameters, ct);
            }
            catch
            {
                //todo nothing
            }
        }

        #endregion Get requests

        private void AddOffsetLimitParameters(Dictionary<string, object> parameters, string offset, int limit)
        {
            if (!string.IsNullOrWhiteSpace(offset))
                parameters.Add("offset", offset);

            if (limit > 0)
                parameters.Add("limit", limit);
        }

        private void AddLoginParameter(Dictionary<string, object> parameters, string login)
        {
            if (!string.IsNullOrEmpty(login))
                parameters.Add("username", login);
        }

        private void AddCensorParameters(Dictionary<string, object> parameters, CensoredNamedRequestWithOffsetLimitFields request)
        {
            parameters.Add("show_nsfw", Convert.ToInt32(request.ShowNsfw));
            parameters.Add("show_low_rated", Convert.ToInt32(request.ShowLowRated));
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
                    result.Errors.Add(Localization.Errors.EmptyResponseContent);
                }
                else if (new Regex(@"<[^>]+>").IsMatch(content))
                {
                    result.Errors.Add(Localization.Errors.ResponseContentContainsHtml + content);
                }
            }

            return result;
        }

        protected virtual OperationResult<T> CreateResult<T>(string json, OperationResult error)
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
