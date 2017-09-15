﻿using System;
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

        protected List<RequestParameter> CreateSessionParameter(string sessionId)
        {
            return new List<RequestParameter>();
        }

        protected List<RequestParameter> CreateOffsetLimitParameters(string offset, int limit)
        {
            var parameters = new List<RequestParameter>();
            if (!string.IsNullOrWhiteSpace(offset))
            {
                parameters.Add(new RequestParameter { Key = "offset", Value = offset, Type = ParameterType.GetOrPost });
            }
            if (limit > 0)
            {
                parameters.Add(new RequestParameter { Key = "limit", Value = limit, Type = ParameterType.GetOrPost });
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
                var parameters = new List<RequestParameter>();
                AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
                AddLoginParameter(parameters, request.Login);
                AddCensorParameters(parameters, request);

                var endpoint = $"user/{request.Username}/posts";
                if (!string.IsNullOrWhiteSpace(request.Login))
                    endpoint = request.Login + "/" + endpoint;

                response = await Gateway.Get(endpoint, parameters, cts);
                errorResult = CheckErrors(response);
            }
            return CreateResult<UserPostResponse>(response?.Content, errorResult);
        }

        public async Task<OperationResult<UserPostResponse>> GetUserRecentPosts(CensoredPostsRequests request, CancellationTokenSource cts)
        {
            OperationResult errorResult = CheckInternetConnection();
            IRestResponse response = null;
            if (errorResult == null)
            {
                var parameters = new List<RequestParameter>();
                AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
                AddLoginParameter(parameters, request.Login);
                AddCensorParameters(parameters, request);

                var endpoint = "recent";
                if (!string.IsNullOrWhiteSpace(request.Login))
                    endpoint = request.Login + "/" + endpoint;

                response = await Gateway.Get(endpoint, parameters, cts);
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
                var parameters = new List<RequestParameter>();
                AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
                AddLoginParameter(parameters, request.Login);
                AddCensorParameters(parameters, request);

                var endpoint = $"posts/{request.Type.ToString().ToLowerInvariant()}";
                if (!string.IsNullOrWhiteSpace(request.Login))
                    endpoint = request.Login + "/" + endpoint;

                response = await Gateway.Get(endpoint, parameters, cts);
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
                var parameters = new List<RequestParameter>();
                AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
                AddLoginParameter(parameters, request.Login);
                AddCensorParameters(parameters, request);

                var endpoint = $"posts/{request.Category}/{request.Type.ToString().ToLowerInvariant()}";
                if (!string.IsNullOrWhiteSpace(request.Login))
                    endpoint = request.Login + "/" + endpoint;

                response = await Gateway.Get(endpoint, parameters, cts);
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
                var parameters = new List<RequestParameter>();
                AddOffsetLimitParameters(parameters, request.Offset, request.Limit);

                var endpoint = $"post/{request.Url}/voters";

                response = await Gateway.Get(endpoint, parameters, cts);
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
                var parameters = new List<RequestParameter>();
                AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
                AddLoginParameter(parameters, request.Login);

                var endpoint = $"post/{request.Url}/comments";
                if (!string.IsNullOrWhiteSpace(request.Login))
                    endpoint = request.Login + "/" + endpoint;

                response = await Gateway.Get(endpoint, parameters, cts);
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
                var parameters = new List<RequestParameter>();
                AddLoginParameter(parameters, request.Login);

                var endpoint = $"user/{request.Username}/info";
                if (!string.IsNullOrWhiteSpace(request.Login))
                    endpoint = request.Login + "/" + endpoint;

                response = await Gateway.Get(endpoint, parameters, cts);
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
                var parameters = new List<RequestParameter>();
                AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
                AddLoginParameter(parameters, request.Login);

                var endpoint = $"user/{request.Username}/{request.Type.ToString().ToLowerInvariant()}";
                if (!string.IsNullOrWhiteSpace(request.Login))
                    endpoint = request.Login + "/" + endpoint;

                response = await Gateway.Get(endpoint, parameters, cts);
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
                response = await Gateway.Get("tos", new List<RequestParameter>(), cts);
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
                var parameters = new List<RequestParameter>();
                AddLoginParameter(parameters, request.Login);

                var endpoint = $"post/{request.Url}/info";
                if (!string.IsNullOrWhiteSpace(request.Login))
                    endpoint = request.Login + "/" + endpoint;

                response = await Gateway.Get(endpoint, parameters, cts);
                errorResult = CheckErrors(response);
            }
            return CreateResult<Post>(response?.Content, errorResult);
        }

        public async Task<OperationResult<SearchResponse<UserSearchResult>>> SearchUser(SearchWithQueryRequest request, CancellationTokenSource cts)
        {
            OperationResult errorResult = CheckInternetConnection();
            IRestResponse response = null;
            if (errorResult == null)
            {
                var parameters = new List<RequestParameter>();
                AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
                parameters.Add(new RequestParameter { Key = "query", Value = request.Query, Type = ParameterType.GetOrPost });

                response = await Gateway.Get("user/search", parameters, cts);
                errorResult = CheckErrors(response);
            }
            return CreateResult<SearchResponse<UserSearchResult>>(response?.Content, errorResult);
        }

        public async Task<OperationResult<UserExistsResponse>> UserExistsCheck(UserExistsRequests request, CancellationTokenSource cts)
        {
            OperationResult errorResult = CheckInternetConnection();
            IRestResponse response = null;
            if (errorResult == null)
            {
                response = await Gateway.Get($"user/{request.Username}/exists", new List<RequestParameter>(), cts);
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
                var parameters = new List<RequestParameter>();
                AddOffsetLimitParameters(parameters, request.Offset, request.Limit);

                response = await Gateway.Get("categories/top", parameters, cts);
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

                var parameters = new List<RequestParameter>();
                AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
                parameters.Add(new RequestParameter { Key = "query", Value = request.Query, Type = ParameterType.GetOrPost });

                response = await Gateway.Get("categories/search", parameters, cts);
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

        public async void Trace(string endpoint, string login, List<string> resultErrors, string target)
        {
            var parameters = new List<RequestParameter>();
            AddLoginParameter(parameters, login);
            parameters.Add(new RequestParameter { Key = "errors", Value = resultErrors == null ? string.Empty : string.Join(Environment.NewLine, resultErrors), Type = ParameterType.GetOrPost });
            if (!string.IsNullOrEmpty(target))
                parameters.Add(new RequestParameter { Key = "target", Value = target, Type = ParameterType.GetOrPost });
            var t = await Gateway.Post($@"log/{endpoint}", parameters, null);
        }

        #endregion Get requests

        private void AddOffsetLimitParameters(List<RequestParameter> parameters, string offset, int limit)
        {
            if (!string.IsNullOrWhiteSpace(offset))
                parameters.Add(new RequestParameter { Key = "offset", Value = offset, Type = ParameterType.GetOrPost });

            if (limit > 0)
                parameters.Add(new RequestParameter { Key = "limit", Value = limit, Type = ParameterType.GetOrPost });
        }

        private void AddLoginParameter(List<RequestParameter> parameters, string login)
        {
            if (!string.IsNullOrEmpty(login))
                parameters.Add(new RequestParameter { Key = "username", Value = login, Type = ParameterType.GetOrPost });
        }

        private void AddCensorParameters(List<RequestParameter> parameters, CensoredPostsRequests request)
        {
            parameters.Add(new RequestParameter { Key = "show_nsfw", Value = Convert.ToInt32(request.ShowNsfw), Type = ParameterType.GetOrPost });
            parameters.Add(new RequestParameter { Key = "show_low_rated", Value = Convert.ToInt32(request.ShowLowRated), Type = ParameterType.GetOrPost });
        }

        public async Task<OperationResult<UploadResponse>> UploadWithPrepare(UploadImageRequest request, string trx, CancellationTokenSource cts)
        {
            OperationResult errorResult = CheckInternetConnection();
            IRestResponse response = null;
            if (errorResult == null)
            {
                var parameters = CreateSessionParameter(request.SessionId);
                if (!request.IsNeedRewards)
                    parameters.Add(new RequestParameter { Key = "set_beneficiary", Value = "steepshot_no_rewards", Type = ParameterType.GetOrPost });
                response = await Gateway.Upload("post/prepare", request.Title, request.Photo, parameters, request.Tags, request.Login, trx, cts);
                errorResult = CheckErrors(response);
            }
            return CreateResult<UploadResponse>(response?.Content, errorResult);
        }

        protected OperationResult CheckInternetConnection()
        {
            var available = ConnectionService.IsConnectionAvailable();
            if (!available)
            {
                return new OperationResult() { Errors = new List<string>() { Localization.Errors.InternetUnavailable } };
            }
            return null;
        }
    }
}
