using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Core.Helpers;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Serializing;
using Steepshot.Core.Errors;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Steepshot.Core.Models.Enums;

namespace Steepshot.Core.HttpClient
{
    public abstract class BaseServerClient
    {
        public volatile bool EnableRead;
        protected ApiGateway Gateway;
        protected JsonNetConverter JsonConverter;

        #region Get requests

        public async Task<OperationResult<ListResponse<Post>>> GetUserPosts(UserPostsModel model, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var results = Validate(model);
            if (results.Any())
                return new OperationResult<ListResponse<Post>>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, model.Offset, model.Limit);
            AddLoginParameter(parameters, model.Login);
            AddCensorParameters(parameters, model);

            var endpoint = $"user/{model.Username}/posts";
            return await Gateway.Get<ListResponse<Post>>(GatewayVersion.V1P1, endpoint, parameters, token);
        }

        public async Task<OperationResult<ListResponse<Post>>> GetUserRecentPosts(CensoredNamedRequestWithOffsetLimitModel request, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var results = Validate(request);
            if (results.Any())
                return new OperationResult<ListResponse<Post>>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            AddLoginParameter(parameters, request.Login);
            AddCensorParameters(parameters, request);

            var endpoint = "recent";
            return await Gateway.Get<ListResponse<Post>>(GatewayVersion.V1P1, endpoint, parameters, token);
        }

        public async Task<OperationResult<ListResponse<Post>>> GetPosts(PostsModel model, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var results = Validate(model);
            if (results.Any())
                return new OperationResult<ListResponse<Post>>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, model.Offset, model.Limit);
            AddLoginParameter(parameters, model.Login);
            AddCensorParameters(parameters, model);

            var endpoint = $"posts/{model.Type.ToString().ToLowerInvariant()}";
            return await Gateway.Get<ListResponse<Post>>(GatewayVersion.V1P1, endpoint, parameters, token);
        }

        public async Task<OperationResult<ListResponse<Post>>> GetPostsByCategory(PostsByCategoryModel model, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var results = Validate(model);
            if (results.Any())
                return new OperationResult<ListResponse<Post>>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, model.Offset, model.Limit);
            AddLoginParameter(parameters, model.Login);
            AddCensorParameters(parameters, model);

            var endpoint = $"posts/{model.Category}/{model.Type.ToString().ToLowerInvariant()}";
            return await Gateway.Get<ListResponse<Post>>(GatewayVersion.V1P1, endpoint, parameters, token);
        }

        public async Task<OperationResult<ListResponse<UserFriend>>> GetPostVoters(VotersModel model, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var results = Validate(model);
            if (results.Any())
                return new OperationResult<ListResponse<UserFriend>>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, model.Offset, model.Limit);
            AddVotersTypeParameters(parameters, model.Type);
            if (!string.IsNullOrEmpty(model.Login))
                AddLoginParameter(parameters, model.Login);

            var endpoint = $"post/{model.Url}/voters";
            return await Gateway.Get<ListResponse<UserFriend>>(GatewayVersion.V1P1, endpoint, parameters, token);
        }

        public async Task<OperationResult<ListResponse<Post>>> GetComments(NamedInfoModel model, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var results = Validate(model);
            if (results.Any())
                return new OperationResult<ListResponse<Post>>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, model.Offset, model.Limit);
            AddLoginParameter(parameters, model.Login);

            var endpoint = $"post/{model.Url}/comments";
            return await Gateway.Get<ListResponse<Post>>(GatewayVersion.V1P1, endpoint, parameters, token);
        }

        public async Task<OperationResult<UserProfileResponse>> GetUserProfile(UserProfileModel model, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var results = Validate(model);
            if (results.Any())
                return new OperationResult<UserProfileResponse>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var parameters = new Dictionary<string, object>();
            AddLoginParameter(parameters, model.Login);
            parameters.Add("show_nsfw", Convert.ToInt32(model.ShowNsfw));
            parameters.Add("show_low_rated", Convert.ToInt32(model.ShowLowRated));

            var endpoint = $"user/{model.Username}/info";
            return await Gateway.Get<UserProfileResponse>(GatewayVersion.V1P1, endpoint, parameters, token);
        }

        public async Task<OperationResult<ListResponse<UserFriend>>> GetUserFriends(UserFriendsModel model, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var results = Validate(model);
            if (results.Any())
                return new OperationResult<ListResponse<UserFriend>>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, model.Offset, model.Limit);
            AddLoginParameter(parameters, model.Login);

            var endpoint = $"user/{model.Username}/{model.Type.ToString().ToLowerInvariant()}";
            return await Gateway.Get<ListResponse<UserFriend>>(GatewayVersion.V1P1, endpoint, parameters, token);
        }

        public async Task<OperationResult<Post>> GetPostInfo(NamedInfoModel model, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var results = Validate(model);
            if (results.Any())
                return new OperationResult<Post>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var parameters = new Dictionary<string, object>();
            AddLoginParameter(parameters, model.Login);
            AddCensorParameters(parameters, model);

            var endpoint = $"post/{model.Url}/info";
            return await Gateway.Get<Post>(GatewayVersion.V1P1, endpoint, parameters, token);
        }

        public async Task<OperationResult<ListResponse<UserFriend>>> SearchUser(SearchWithQueryModel model, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var results = Validate(model);
            if (results.Any())
                return new OperationResult<ListResponse<UserFriend>>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var parameters = new Dictionary<string, object>();
            AddLoginParameter(parameters, model.Login);
            AddOffsetLimitParameters(parameters, model.Offset, model.Limit);
            parameters.Add("query", model.Query);

            var endpoint = "user/search";
            return await Gateway.Get<ListResponse<UserFriend>>(GatewayVersion.V1P1, endpoint, parameters, token);
        }

        public async Task<OperationResult<UserExistsResponse>> UserExistsCheck(UserExistsModel model, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var results = Validate(model);
            if (results.Any())
                return new OperationResult<UserExistsResponse>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var parameters = new Dictionary<string, object>();
            var endpoint = $"user/{model.Username}/exists";
            return await Gateway.Get<UserExistsResponse>(GatewayVersion.V1, endpoint, parameters, token);
        }

        public async Task<OperationResult<ListResponse<SearchResult>>> GetCategories(OffsetLimitModel request, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var results = Validate(request);
            if (results.Any())
                return new OperationResult<ListResponse<SearchResult>>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            var endpoint = "categories/top";
            var result = await Gateway.Get<ListResponse<SearchResult>>(GatewayVersion.V1, endpoint, parameters, token);

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
            if (results.Any())
                return new OperationResult<ListResponse<SearchResult>>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var query = Transliteration.ToEng(model.Query);
            if (query != model.Query)
            {
                query = $"ru--{query}";
            }
            model.Query = query;

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, model.Offset, model.Limit);
            parameters.Add("query", model.Query);
            var endpoint = "categories/search";
            var result = await Gateway.Get<ListResponse<SearchResult>>(GatewayVersion.V1, endpoint, parameters, token);

            if (result.IsSuccess)
            {
                foreach (var categories in result.Result.Results)
                {
                    categories.Name = Transliteration.ToRus(categories.Name);
                }
            }

            return result;
        }

        protected async Task<OperationResult<VoidResponse>> Trace(string endpoint, string login, ErrorBase resultErrors, string target, CancellationToken token)
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
                var result = await Gateway.Post<VoidResponse>(GatewayVersion.V1, $@"log/{endpoint}", parameters, token);
                if (result.IsSuccess)
                    result.Result = new VoidResponse(true);
                return result;
            }
            catch
            {
                //todo nothing
            }
            return null;
        }

        public async Task<OperationResult<BeneficiariesResponse>> GetBeneficiaries(bool isNeedRewards, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            var parameters = new Dictionary<string, object>();
            SetBeneficiaryParameters(parameters, isNeedRewards);

            var endpoint = "beneficiaries";
            return await Gateway.Get<BeneficiariesResponse>(GatewayVersion.V1, endpoint, parameters, token);
        }

        #endregion Get requests

        public async Task<OperationResult<PreparePostResponce>> PreparePost(PreparePostModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results.Any())
                return new OperationResult<PreparePostResponce>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            return await Gateway.Post<PreparePostResponce, PreparePostModel>(GatewayVersion.V1P1, "post/prepare", model, ct);
        }


        public async Task<OperationResult<NsfwRate>> NsfwCheck(Stream stream, CancellationToken token)
        {
            if (!EnableRead)
                return null;

            return await Gateway.NsfwCheck(stream, token);
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

        private void AddCensorParameters(Dictionary<string, object> parameters, CensoredNamedRequestWithOffsetLimitModel request)
        {
            parameters.Add("show_nsfw", Convert.ToInt32(request.ShowNsfw));
            parameters.Add("show_low_rated", Convert.ToInt32(request.ShowLowRated));
        }

        protected List<ValidationResult> Validate<T>(T request)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(request);
            Validator.TryValidateObject(request, context, results, true);
            return results;
        }
    }
}
