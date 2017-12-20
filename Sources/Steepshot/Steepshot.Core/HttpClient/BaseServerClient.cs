using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Core.Helpers;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Serializing;
using Steepshot.Core.Errors;

namespace Steepshot.Core.HttpClient
{
    public class BaseServerClient
    {
        public volatile bool EnableRead;
        public readonly ApiGateway Gateway;

        protected readonly JsonNetConverter JsonConverter;

        public BaseServerClient(JsonNetConverter converter)
        {
            Gateway = new ApiGateway();
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
            return await CreateResult<ListResponce<Post>>(response);
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
            return await CreateResult<ListResponce<Post>>(response);
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
            return await CreateResult<ListResponce<Post>>(response);
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
            return await CreateResult<ListResponce<Post>>(response);
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
            return await CreateResult<ListResponce<UserFriend>>(response);
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
            return await CreateResult<ListResponce<Post>>(response);
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
            return await CreateResult<UserProfileResponse>(response);
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
            return await CreateResult<ListResponce<UserFriend>>(response);
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
            return await CreateResult<Post>(response);
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
            return await CreateResult<ListResponce<UserFriend>>(response);
        }

        public async Task<OperationResult<UserExistsResponse>> UserExistsCheck(UserExistsRequests request, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var parameters = new Dictionary<string, object>();
            var endpoint = $"user/{request.Username}/exists";
            var response = await Gateway.Get(GatewayVersion.V1, endpoint, parameters, ct);
            return await CreateResult<UserExistsResponse>(response);
        }

        public async Task<OperationResult<ListResponce<SearchResult>>> GetCategories(OffsetLimitFields request, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            var endpoint = "categories/top";
            var response = await Gateway.Get(GatewayVersion.V1, endpoint, parameters, ct);

            var result = await CreateResult<ListResponce<SearchResult>>(response);
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
            var result = await CreateResult<ListResponce<SearchResult>>(response);

            if (result.Success)
            {
                foreach (var categories in result.Result.Results)
                {
                    categories.Name = Transliteration.ToRus(categories.Name);
                }
            }

            return result;
        }

        public async Task Trace(string endpoint, string login, ErrorBase resultErrors, string target, CancellationToken ct)
        {
            if (!EnableRead)
                return;

            try
            {
                var parameters = new Dictionary<string, object>();
                AddLoginParameter(parameters, login);
                parameters.Add("error", resultErrors == null ? string.Empty : resultErrors.Message);
                if (!string.IsNullOrEmpty(target))
                    parameters.Add("target", target);
                await Gateway.Post(GatewayVersion.V1, $@"log/{endpoint}", parameters, ct);
            }
            catch
            {
                //todo nothing
            }
        }

        public async Task<OperationResult<BeneficiariesResponse>> GetBeneficiaries(bool isNeedRewards, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            var parameters = new Dictionary<string, object>();
            SetBeneficiaryParameters(parameters, isNeedRewards);

            var endpoint = "beneficiaries";
            var response = await Gateway.Get(GatewayVersion.V1, endpoint, parameters, ct);
            return await CreateResult<BeneficiariesResponse>(response);
        }

        #endregion Get requests


        public Task<OperationResult<UploadResponse>> UploadWithPrepare(UploadImageRequest request, CancellationToken ct)
        {
            if (!EnableRead)
                return null;

            return Task.Run(async () =>
            {
                OperationHelper.PrepareTags(request.Tags);
                var response = await Gateway.Upload(GatewayVersion.V1, "post/prepare", request, ct);
                return await CreateResult<UploadResponse>(response);
            }, ct);
        }

        private void AddOffsetLimitParameters(Dictionary<string, object> parameters, string offset, int limit)
        {
            if (!string.IsNullOrWhiteSpace(offset))
                parameters.Add("offset", offset);

            if (limit > 0)
                parameters.Add("limit", limit);
        }

        private void SetBeneficiaryParameters(Dictionary<string, object> parameters, bool isNeedRewards)
        {
            if (!isNeedRewards)
                parameters.Add("set_beneficiary", "steepshot_no_rewards");
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


        protected virtual async Task<OperationResult<T>> CreateResult<T>(HttpResponseMessage response)
        {
            var result = new OperationResult<T>();
            var content = await response.Content.ReadAsStringAsync();

            // HTTP error
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                result.Error = new HttpError((int)response.StatusCode, Localization.Errors.StatusCodeToMessage(response.StatusCode));
            }

            if (!result.Success)
            {
                // Checking content
                if (string.IsNullOrWhiteSpace(content))
                {
                    result.Error = new ServerError(Localization.Errors.EmptyResponseContent);
                }
                else if (new Regex(@"<[^>]+>").IsMatch(content))
                {
                    result.Error = new ServerError(Localization.Errors.ResponseContentContainsHtml + content);
                }
            }
            else
            {
                result.Result = JsonConverter.Deserialize<T>(content);
            }

            return result;
        }
    }
}
