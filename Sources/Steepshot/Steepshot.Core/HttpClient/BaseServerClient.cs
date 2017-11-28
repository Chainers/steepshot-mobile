using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Core.Helpers;
using RestSharp.Portable;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Serializing;

namespace Steepshot.Core.HttpClient
{
    public class BaseServerClient
    {
        public volatile bool EnableRead;
        public ApiGateway Gateway;

        protected readonly JsonNetConverter JsonConverter;

        public BaseServerClient(JsonNetConverter converter)
        {
            JsonConverter = converter;
        }

        #region Get requests

        public async Task<OperationResult<ListResponce<Post>>> GetUserPosts(UserPostsRequest request, CancellationToken ct)
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

            return CreateResult<ListResponce<Post>>(response?.Content, errorResult);
        }

        public async Task<OperationResult<ListResponce<Post>>> GetUserRecentPosts(CensoredNamedRequestWithOffsetLimitFields request, CancellationToken ct)
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
            return CreateResult<ListResponce<Post>>(response?.Content, errorResult);
        }

        public async Task<OperationResult<ListResponce<Post>>> GetPosts(PostsRequest request, CancellationToken ct)
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

            return CreateResult<ListResponce<Post>>(response?.Content, errorResult);
        }

        public async Task<OperationResult<ListResponce<Post>>> GetPostsByCategory(PostsByCategoryRequest request, CancellationToken ct)
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

            return CreateResult<ListResponce<Post>>(response?.Content, errorResult);
        }

        public async Task<OperationResult<ListResponce<UserFriend>>> GetPostVoters(VotersRequest request, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            AddVotersTypeParameters(parameters, request.Type);
            if (!string.IsNullOrEmpty(request.Login))
                AddLoginParameter(parameters, request.Login);

            var endpoint = $"post/{request.Url}/voters";

            var response = await Gateway.Get(GatewayVersion.V1P1, endpoint, parameters, ct);
            var errorResult = CheckErrors(response);

            return CreateResult<ListResponce<UserFriend>>(response?.Content, errorResult);
        }

        public async Task<OperationResult<ListResponce<Post>>> GetComments(NamedInfoRequest request, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            AddLoginParameter(parameters, request.Login);

            var endpoint = $"post/{request.Url}/comments";

            var response = await Gateway.Get(GatewayVersion.V1P1, endpoint, parameters, ct);
            var errorResult = CheckErrors(response);

            return CreateResult<ListResponce<Post>>(response?.Content, errorResult);
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

        public async Task<OperationResult<ListResponce<UserFriend>>> GetUserFriends(UserFriendsRequest request, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            AddLoginParameter(parameters, request.Login);

            var endpoint = $"user/{request.Username}/{request.Type.ToString().ToLowerInvariant()}";

            var response = await Gateway.Get(GatewayVersion.V1P1, endpoint, parameters, ct);
            var errorResult = CheckErrors(response);

            return CreateResult<ListResponce<UserFriend>>(response?.Content, errorResult);
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

        public async Task<OperationResult<ListResponce<UserFriend>>> SearchUser(SearchWithQueryRequest request, CancellationToken ct)
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

            return CreateResult<ListResponce<UserFriend>>(response?.Content, errorResult);
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

        public async Task<OperationResult<ListResponce<SearchResult>>> GetCategories(OffsetLimitFields request, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            var endpoint = "categories/top";

            var response = await Gateway.Get(GatewayVersion.V1, endpoint, parameters, ct);
            var errorResult = CheckErrors(response);

            var result = CreateResult<ListResponce<SearchResult>>(response?.Content, errorResult);
            if (result.Success)
            {
                foreach (var category in result.Result.Results)
                {
                    category.Name = Transliteration.ToRus(category.Name);
                }
            }

            return result;
        }

        public async Task<OperationResult<ListResponce<SearchResult>>> SearchCategories(SearchWithQueryRequest request, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var query = Transliteration.ToEng(request.Query);
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

            var result = CreateResult<ListResponce<SearchResult>>(response?.Content, errorResult);

            if (result.Success)
            {
                foreach (var categories in result.Result.Results)
                {
                    categories.Name = Transliteration.ToRus(categories.Name);
                }
            }

            return result;
        }

        public async Task Trace(string endpoint, string login, List<string> resultErrors, string target, CancellationToken ct)
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


        public async Task<OperationResult<UploadResponse>> UploadWithPrepare(UploadImageRequest request, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            return await Task.Run(async () =>
            {
                Transliteration.PrepareTags(request.Tags);

                var response = await Gateway.Upload(GatewayVersion.V1, "post/prepare", request, ct);
                var errorResult = CheckErrors(response);
                return CreateResult<UploadResponse>(response?.Content, errorResult);

            }, ct);
        }

        private void AddOffsetLimitParameters(Dictionary<string, object> parameters, string offset, int limit)
        {
            if (!string.IsNullOrWhiteSpace(offset))
                parameters.Add("offset", offset);

            if (limit > 0)
                parameters.Add("limit", limit);
        }

        private void AddVotersTypeParameters(Dictionary<string, object> parameters, VotersType type)
        {
            if (type != VotersType.All)
                parameters.Add(type == VotersType.Likes ? "likes" : "flags", 1);
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
                var dic = JsonConverter.Deserialize<Dictionary<string, List<string>>>(content);
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
                result.Result = JsonConverter.Deserialize<T>(json);
            }
            else
            {
                result.Errors.AddRange(error.Errors);
            }

            return result;
        }
    }
}
