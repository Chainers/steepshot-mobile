using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using RestSharp.Portable;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Serializing;
using Steepshot.Core.Services;
using Steepshot.Core.Utils;

namespace Steepshot.Core.HttpClient
{
    public class BaseClient
    {
        private IApiGateway _gateway;
        protected readonly string Url;

        protected IApiGateway Gateway => _gateway ?? (_gateway = new ApiGateway(Url));

        private IConnectionService _connectionService;
        private IConnectionService ConnectionService => _connectionService ?? (_connectionService = AppSettings.Container.Resolve<IConnectionService>());

        private readonly JsonNetConverter _jsonConverter;

        protected BaseClient(string url)
        {
            Url = url;
            _jsonConverter = new JsonNetConverter();
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
            OperationResult errorResult = CheckInternetConnection();
            IRestResponse response = null;
            if (errorResult == null)
            {
                var parameters = new KeyValueList();
                AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
                AddLoginParameter(parameters, request.Login);
                AddCensorParameters(parameters, request);

                var endpoint = $"user/{request.Username}/posts";
                if (!string.IsNullOrWhiteSpace(request.Login))
                    endpoint = request.Login + "/" + endpoint;

                response = await Gateway.Get(GatewayVersion.V1, endpoint, parameters, cts);
                errorResult = CheckErrors(response);
            }
            return CreateResult<UserPostResponse>(response?.Content, errorResult);
        }

        public async Task<OperationResult<UserPostResponse>> GetUserRecentPosts(CensoredNamedRequestWithOffsetLimitFields request, CancellationTokenSource cts)
        {
            OperationResult errorResult = CheckInternetConnection();
            IRestResponse response = null;
            if (errorResult == null)
            {
                var parameters = new KeyValueList();
                AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
                AddLoginParameter(parameters, request.Login);
                AddCensorParameters(parameters, request);

                var endpoint = "recent";
                if (!string.IsNullOrWhiteSpace(request.Login))
                    endpoint = request.Login + "/" + endpoint;

                response = await Gateway.Get(GatewayVersion.V1, endpoint, parameters, cts);
                errorResult = CheckErrors(response);
            }
            return CreateResult<UserPostResponse>(response?.Content, errorResult);
        }

        public async Task<OperationResult<UserPostResponse>> GetPosts(PostsRequest request, CancellationTokenSource cts)
        {
            OperationResult errorResult = CheckInternetConnection();
            IRestResponse response = null;
            if (errorResult == null)
            {
                var parameters = new KeyValueList();
                AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
                AddLoginParameter(parameters, request.Login);
                AddCensorParameters(parameters, request);

                var endpoint = $"posts/{request.Type.ToString().ToLowerInvariant()}";
                if (!string.IsNullOrWhiteSpace(request.Login))
                    endpoint = request.Login + "/" + endpoint;

                response = await Gateway.Get(GatewayVersion.V1, endpoint, parameters, cts);
                errorResult = CheckErrors(response);
            }
            return CreateResult<UserPostResponse>(response?.Content, errorResult);
        }

        public async Task<OperationResult<UserPostResponse>> GetPostsByCategory(PostsByCategoryRequest request, CancellationTokenSource cts)
        {
            OperationResult errorResult = CheckInternetConnection();
            IRestResponse response = null;
            if (errorResult == null)
            {
                var parameters = new KeyValueList();
                AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
                AddLoginParameter(parameters, request.Login);
                AddCensorParameters(parameters, request);

                var endpoint = $"posts/{request.Category}/{request.Type.ToString().ToLowerInvariant()}";
                if (!string.IsNullOrWhiteSpace(request.Login))
                    endpoint = request.Login + "/" + endpoint;

                response = await Gateway.Get(GatewayVersion.V1, endpoint, parameters, cts);
                errorResult = CheckErrors(response);
            }
            return CreateResult<UserPostResponse>(response?.Content, errorResult);
        }

        public async Task<OperationResult<SearchResponse<VotersResult>>> GetPostVoters(InfoRequest request, CancellationTokenSource cts)
        {
            OperationResult errorResult = CheckInternetConnection();
            IRestResponse response = null;
            if (errorResult == null)
            {
                var parameters = new KeyValueList();
                AddOffsetLimitParameters(parameters, request.Offset, request.Limit);

                var endpoint = $"post/{request.Url}/voters";

                response = await Gateway.Get(GatewayVersion.V1, endpoint, parameters, cts);
                errorResult = CheckErrors(response);
            }
            return CreateResult<SearchResponse<VotersResult>>(response?.Content, errorResult);
        }

        public async Task<OperationResult<GetCommentResponse>> GetComments(NamedInfoRequest request, CancellationTokenSource cts)
        {
            OperationResult errorResult = CheckInternetConnection();
            IRestResponse response = null;
            if (errorResult == null)
            {
                var parameters = new KeyValueList();
                AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
                AddLoginParameter(parameters, request.Login);

                var endpoint = $"post/{request.Url}/comments";
                if (!string.IsNullOrWhiteSpace(request.Login))
                    endpoint = request.Login + "/" + endpoint;

                response = await Gateway.Get(GatewayVersion.V1, endpoint, parameters, cts);
                errorResult = CheckErrors(response);
            }
            return CreateResult<GetCommentResponse>(response?.Content, errorResult);
        }

        public async Task<OperationResult<UserProfileResponse>> GetUserProfile(UserProfileRequest request, CancellationTokenSource cts)
        {
            OperationResult errorResult = CheckInternetConnection();
            IRestResponse response = null;
            if (errorResult == null)
            {
                var parameters = new KeyValueList();
                AddLoginParameter(parameters, request.Login);

                var endpoint = $"user/{request.Username}/info";
                if (!string.IsNullOrWhiteSpace(request.Login))
                    endpoint = request.Login + "/" + endpoint;

                response = await Gateway.Get(GatewayVersion.V1, endpoint, parameters, cts);
                errorResult = CheckErrors(response);
            }

            return CreateResult<UserProfileResponse>(response?.Content, errorResult);
        }

        public async Task<OperationResult<UserFriendsResponse>> GetUserFriends(UserFriendsRequest request, CancellationTokenSource cts)
        {
            OperationResult errorResult = CheckInternetConnection();
            IRestResponse response = null;
            if (errorResult == null)
            {
                var parameters = new KeyValueList();
                AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
                AddLoginParameter(parameters, request.Login);

                var endpoint = $"user/{request.Username}/{request.Type.ToString().ToLowerInvariant()}";
                if (!string.IsNullOrWhiteSpace(request.Login))
                    endpoint = request.Login + "/" + endpoint;

                response = await Gateway.Get(GatewayVersion.V1, endpoint, parameters, cts);
                errorResult = CheckErrors(response);
            }
            return CreateResult<UserFriendsResponse>(response?.Content, errorResult);
        }

        public async Task<OperationResult<TermOfServiceResponse>> TermsOfService(CancellationTokenSource cts)
        {
            OperationResult errorResult = CheckInternetConnection();
            IRestResponse response = null;
            if (errorResult == null)
            {
                response = await Gateway.Get(GatewayVersion.V1, "tos", new KeyValueList(), cts);
                errorResult = CheckErrors(response);
            }
            return CreateResult<TermOfServiceResponse>(response?.Content, errorResult);
        }

        public async Task<OperationResult<Post>> GetPostInfo(NamedInfoRequest request, CancellationTokenSource cts)
        {
            OperationResult errorResult = CheckInternetConnection();
            IRestResponse response = null;
            if (errorResult == null)
            {
                var parameters = new KeyValueList();
                AddLoginParameter(parameters, request.Login);
                AddCensorParameters(parameters, request);
                var endpoint = $"post/{request.Url}/info";
                if (!string.IsNullOrWhiteSpace(request.Login))
                    endpoint = request.Login + "/" + endpoint;

                response = await Gateway.Get(GatewayVersion.V1, endpoint, parameters, cts);
                errorResult = CheckErrors(response);
            }
            return CreateResult<Post>(response?.Content, errorResult);
        }

        public async Task<OperationResult<SearchResponse<UserFriend>>> SearchUser(SearchWithQueryRequest request, CancellationTokenSource cts)
        {
            OperationResult errorResult = CheckInternetConnection();
            IRestResponse response = null;
            if (errorResult == null)
            {
                var parameters = new KeyValueList();
                AddLoginParameter(parameters, request.Login);
                AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
                parameters.Add("query", request.Query);

                response = await Gateway.Get(GatewayVersion.V1P1, "user/search", parameters, cts);
                errorResult = CheckErrors(response);
            }
            return CreateResult<SearchResponse<UserFriend>>(response?.Content, errorResult);
        }

        public async Task<OperationResult<UserExistsResponse>> UserExistsCheck(UserExistsRequests request, CancellationTokenSource cts)
        {
            OperationResult errorResult = CheckInternetConnection();
            IRestResponse response = null;
            if (errorResult == null)
            {
                response = await Gateway.Get(GatewayVersion.V1, $"user/{request.Username}/exists", new KeyValueList(), cts);
                errorResult = CheckErrors(response);
            }
            return CreateResult<UserExistsResponse>(response?.Content, errorResult);
        }

        public async Task<OperationResult<SearchResponse<SearchResult>>> GetCategories(OffsetLimitFields request, CancellationTokenSource cts)
        {
            OperationResult errorResult = CheckInternetConnection();
            IRestResponse response = null;
            if (errorResult == null)
            {
                var parameters = new KeyValueList();
                AddOffsetLimitParameters(parameters, request.Offset, request.Limit);

                response = await Gateway.Get(GatewayVersion.V1, "categories/top", parameters, cts);
                errorResult = CheckErrors(response);
            }
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

        public async Task<OperationResult<SearchResponse<SearchResult>>> SearchCategories(SearchWithQueryRequest request, CancellationTokenSource cts = null)
        {
            OperationResult errorResult = CheckInternetConnection();
            IRestResponse response = null;
            if (errorResult == null)
            {
                var query = Ditch.Helpers.Transliteration.ToEng(request.Query);
                if (query != request.Query)
                {
                    query = $"ru--{query}";
                }
                request.Query = query;

                var parameters = new KeyValueList();
                AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
                parameters.Add("query", request.Query);

                response = await Gateway.Get(GatewayVersion.V1, "categories/search", parameters, cts);
                errorResult = CheckErrors(response);
            }
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

        protected async void Trace(string endpoint, string login, List<string> resultErrors, string target)
        {
            var parameters = new KeyValueList();
            AddLoginParameter(parameters, login);
            parameters.Add("errors", resultErrors == null ? string.Empty : string.Join(Environment.NewLine, resultErrors));
            if (!string.IsNullOrEmpty(target))
                parameters.Add("target", target);
            var t = await Gateway.Post(GatewayVersion.V1, $@"log/{endpoint}", parameters, null);
        }

        #endregion Get requests

        private void AddOffsetLimitParameters(KeyValueList parameters, string offset, int limit)
        {
            if (!string.IsNullOrWhiteSpace(offset))
                parameters.Add("offset", offset);

            if (limit > 0)
                parameters.Add("limit", limit);
        }

        private void AddLoginParameter(KeyValueList parameters, string login)
        {
            if (!string.IsNullOrEmpty(login))
                parameters.Add("username", login);
        }

        private void AddCensorParameters(KeyValueList parameters, CensoredNamedRequestWithOffsetLimitFields request)
        {
            parameters.Add("show_nsfw", Convert.ToInt32(request.ShowNsfw));
            parameters.Add("show_low_rated", Convert.ToInt32(request.ShowLowRated));
        }

        public async Task<OperationResult<UploadResponse>> UploadWithPrepare(UploadImageRequest request, string trx, CancellationTokenSource cts)
        {
            OperationResult errorResult = CheckInternetConnection();
            IRestResponse response = null;
            if (errorResult == null)
            {
                var parameters = new KeyValueList();
                if (!request.IsNeedRewards)
                    parameters.Add("set_beneficiary", "steepshot_no_rewards");

                response = await Gateway.Upload(GatewayVersion.V1, "post/prepare", request, parameters, trx, cts);
                errorResult = CheckErrors(response);
            }
            return CreateResult<UploadResponse>(response?.Content, errorResult);
        }

        protected OperationResult CheckInternetConnection()
        {
            var available = ConnectionService.IsConnectionAvailable();
            if (!available)
            {
                return new OperationResult { Errors = new List<string> { Localization.Errors.InternetUnavailable } };
            }
            return null;
        }
    }
}
