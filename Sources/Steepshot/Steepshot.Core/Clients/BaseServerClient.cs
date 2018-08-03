using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Core;
using Ditch.Core.JsonRpc;
using Steepshot.Core.Errors;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Clients
{
    public abstract class BaseServerClient
    {
        public bool EnableRead;
        public ExtendedHttpClient HttpClient;
        protected string BaseUrl;

        #region Get requests

        public async Task<OperationResult<ListResponse<Post>>> GetUserPosts(UserPostsModel model, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var results = Validate(model);
            if (results != null)
                return new OperationResult<ListResponse<Post>>(results);

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, model.Offset, model.Limit);
            AddLoginParameter(parameters, model.Login);
            AddCensorParameters(parameters, model);

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/user/{model.Username}/posts";
            return await HttpClient.Get<ListResponse<Post>>(endpoint, parameters, token);
        }

        public async Task<OperationResult<ListResponse<Post>>> GetUserRecentPosts(CensoredNamedRequestWithOffsetLimitModel request, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var results = Validate(request);
            if (results != null)
                return new OperationResult<ListResponse<Post>>(results);

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            AddLoginParameter(parameters, request.Login);
            AddCensorParameters(parameters, request);

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/recent";
            return await HttpClient.Get<ListResponse<Post>>(endpoint, parameters, token);
        }

        public async Task<OperationResult<ListResponse<Post>>> GetPosts(PostsModel model, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var results = Validate(model);
            if (results != null)
                return new OperationResult<ListResponse<Post>>(results);

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, model.Offset, model.Limit);
            AddLoginParameter(parameters, model.Login);
            AddCensorParameters(parameters, model);

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/posts/{model.Type.ToString().ToLowerInvariant()}";
            return await HttpClient.Get<ListResponse<Post>>(endpoint, parameters, token);
        }

        public async Task<OperationResult<ListResponse<Post>>> GetPostsByCategory(PostsByCategoryModel model, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var results = Validate(model);
            if (results != null)
                return new OperationResult<ListResponse<Post>>(results);

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, model.Offset, model.Limit);
            AddLoginParameter(parameters, model.Login);
            AddCensorParameters(parameters, model);

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/posts/{model.Category}/{model.Type.ToString().ToLowerInvariant()}";
            return await HttpClient.Get<ListResponse<Post>>(endpoint, parameters, token);
        }

        public async Task<OperationResult<ListResponse<UserFriend>>> GetPostVoters(VotersModel model, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var results = Validate(model);
            if (results != null)
                return new OperationResult<ListResponse<UserFriend>>(results);

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, model.Offset, model.Limit);
            AddVotersTypeParameters(parameters, model.Type);
            if (!string.IsNullOrEmpty(model.Login))
                AddLoginParameter(parameters, model.Login);

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/post/{model.Url}/voters";
            return await HttpClient.Get<ListResponse<UserFriend>>(endpoint, parameters, token);
        }

        public async Task<OperationResult<ListResponse<Post>>> GetComments(NamedInfoModel model, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var results = Validate(model);
            if (results != null)
                return new OperationResult<ListResponse<Post>>(results);

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, model.Offset, model.Limit);
            AddLoginParameter(parameters, model.Login);

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/post/{model.Url}/comments";
            var resp = await HttpClient.Get<ListResponse<Post>>(endpoint, parameters, token);
            if (resp.IsSuccess)
                resp.Result.Results.ForEach(p => p.IsComment = true);

            return resp;
        }

        public async Task<OperationResult<UserProfileResponse>> GetUserProfile(UserProfileModel model, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var results = Validate(model);
            if (results != null)
                return new OperationResult<UserProfileResponse>(results);

            var parameters = new Dictionary<string, object>();
            AddLoginParameter(parameters, model.Login);
            parameters.Add("show_nsfw", Convert.ToInt32(model.ShowNsfw));
            parameters.Add("show_low_rated", Convert.ToInt32(model.ShowLowRated));

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/user/{model.Username}/info";
            return await HttpClient.Get<UserProfileResponse>(endpoint, parameters, token);
        }

        public async Task<OperationResult<ListResponse<UserFriend>>> GetUserFriends(UserFriendsModel model, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var results = Validate(model);
            if (results != null)
                return new OperationResult<ListResponse<UserFriend>>(results);

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, model.Offset, model.Limit);
            AddLoginParameter(parameters, model.Login);

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/user/{model.Username}/{model.Type.ToString().ToLowerInvariant()}";
            return await HttpClient.Get<ListResponse<UserFriend>>(endpoint, parameters, token);
        }

        public async Task<OperationResult<Post>> GetPostInfo(NamedInfoModel model, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var results = Validate(model);
            if (results != null)
                return new OperationResult<Post>(results);

            var parameters = new Dictionary<string, object>();
            AddLoginParameter(parameters, model.Login);
            AddCensorParameters(parameters, model);

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/post/{model.Url}/info";
            return await HttpClient.Get<Post>(endpoint, parameters, token);
        }

        public async Task<OperationResult<ListResponse<UserFriend>>> SearchUser(SearchWithQueryModel model, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var results = Validate(model);
            if (results != null)
                return new OperationResult<ListResponse<UserFriend>>(results);

            var parameters = new Dictionary<string, object>();
            AddLoginParameter(parameters, model.Login);
            AddOffsetLimitParameters(parameters, model.Offset, model.Limit);
            parameters.Add("query", model.Query);

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/user/search";
            return await HttpClient.Get<ListResponse<UserFriend>>(endpoint, parameters, token);
        }

        public async Task<OperationResult<UserExistsResponse>> UserExistsCheck(UserExistsModel model, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var results = Validate(model);
            if (results != null)
                return new OperationResult<UserExistsResponse>(results);

            var parameters = new Dictionary<string, object>();
            var endpoint = $"{BaseUrl}/{GatewayVersion.V1}/user/{model.Username}/exists";
            return await HttpClient.Get<UserExistsResponse>(endpoint, parameters, token);
        }

        public async Task<OperationResult<ListResponse<SearchResult>>> GetCategories(OffsetLimitModel request, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var results = Validate(request);
            if (results != null)
                return new OperationResult<ListResponse<SearchResult>>(results);

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            var endpoint = $"{BaseUrl}/{GatewayVersion.V1}/categories/top";
            var result = await HttpClient.Get<ListResponse<SearchResult>>(endpoint, parameters, token);

            if (result.IsSuccess)
            {
                foreach (var category in result.Result.Results)
                {
                    category.Name = Transliteration.ToRus(category.Name);
                }
            }
            return result;
        }

        public async Task<OperationResult<ListResponse<SearchResult>>> SearchCategories(SearchWithQueryModel model, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var results = Validate(model);
            if (results != null)
                return new OperationResult<ListResponse<SearchResult>>(results);

            var query = Transliteration.ToEng(model.Query);
            if (query != model.Query)
            {
                query = $"ru--{query}";
            }
            model.Query = query;

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, model.Offset, model.Limit);
            parameters.Add("query", model.Query);
            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/categories/search";
            var result = await HttpClient.Get<ListResponse<SearchResult>>(endpoint, parameters, token);

            if (result.IsSuccess)
            {
                foreach (var categories in result.Result.Results)
                {
                    categories.Name = Transliteration.ToRus(categories.Name);
                }
            }

            return result;
        }

        protected async Task<OperationResult<VoidResponse>> Trace(string endpoint, string login, Exception resultErrors, string target, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            try
            {
                var parameters = new Dictionary<string, object>();
                AddLoginParameter(parameters, login);
                parameters.Add("error", resultErrors == null ? string.Empty : resultErrors.Message);
                if (!string.IsNullOrEmpty(target))
                    parameters.Add("target", target);

                endpoint = $"{BaseUrl}/{GatewayVersion.V1}/log/{endpoint}";
                var result = await HttpClient.Post<VoidResponse>(endpoint, parameters, token);
                if (result.IsSuccess)
                    result.Result = new VoidResponse();
                return result;
            }
            catch (Exception ex)
            {
                await AppSettings.Logger.Warning(ex);
            }
            return null;
        }

        public async Task<OperationResult<BeneficiariesResponse>> GetBeneficiaries(CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1}/beneficiaries";
            return await HttpClient.Get<BeneficiariesResponse>(endpoint, token);
        }

        public async Task<OperationResult<SpamResponse>> CheckForSpam(string username, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/user/{username}/spam";
            var result = await HttpClient.Get<SpamResponse>(endpoint, token);
            return result;
        }

        public async Task<OperationResult<CurrencyRate[]>> GetCurrencyRates(CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/currency/rates";
            var result = await HttpClient.Get<CurrencyRate[]>(endpoint, token);
            return result;
        }

        #endregion Get requests

        public async Task<OperationResult<PreparePostResponse>> PreparePost(PreparePostModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<PreparePostResponse>(results);

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/post/prepare";
            return await HttpClient.Post<PreparePostResponse, PreparePostModel>(endpoint, model, ct);
        }

        public async Task<OperationResult<NsfwRate>> NsfwCheck(Stream stream, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            return await HttpClient.NsfwCheck(stream, token);
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

        private void AddCensorParameters(Dictionary<string, object> parameters, CensoredNamedRequestWithOffsetLimitModel request)
        {
            parameters.Add("show_nsfw", Convert.ToInt32(request.ShowNsfw));
            parameters.Add("show_low_rated", Convert.ToInt32(request.ShowLowRated));
        }

        protected ValidationError Validate<T>(T request)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(request);
            Validator.TryValidateObject(request, context, results, true);
            if (results.Any())
            {
                var msg = results.Select(m => m.ErrorMessage).First();
                return new ValidationError(msg);
            }
            return null;
        }
    }
}
