using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Core;
using Ditch.Core.JsonRpc;
using Steepshot.Core.Authorization;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Clients
{
    [Obsolete]
    public abstract class BaseServerClient
    {
        protected readonly ILogService LogService;
        protected readonly ExtendedHttpClient HttpClient;
        protected readonly string BaseUrl;

        protected BaseServerClient(ExtendedHttpClient httpClient, ILogService logService, string baseUrl)
        {
            LogService = logService;
            HttpClient = httpClient;
            BaseUrl = baseUrl;
        }

        #region Get requests

        public async Task<OperationResult<ListResponse<Post>>> GetUserPostsAsync(UserPostsModel model, CancellationToken token)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<ListResponse<Post>>(results);

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, model.Offset, model.Limit);
            AddLoginParameter(parameters, model.Login);
            AddCensorParameters(parameters, model);

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/user/{model.Username}/posts";
            return await HttpClient.GetAsync<ListResponse<Post>>(endpoint, parameters, token).ConfigureAwait(false);
        }

        public async Task<OperationResult<ListResponse<Post>>> GetUserRecentPostsAsync(CensoredNamedRequestWithOffsetLimitModel request, CancellationToken token)
        {
            var results = Validate(request);
            if (results != null)
                return new OperationResult<ListResponse<Post>>(results);

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            AddLoginParameter(parameters, request.Login);
            AddCensorParameters(parameters, request);

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/recent";
            return await HttpClient.GetAsync<ListResponse<Post>>(endpoint, parameters, token).ConfigureAwait(false);
        }

        public async Task<OperationResult<ListResponse<Post>>> GetPostsAsync(PostsModel model, CancellationToken token)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<ListResponse<Post>>(results);

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, model.Offset, model.Limit);
            AddLoginParameter(parameters, model.Login);
            AddCensorParameters(parameters, model);

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/posts/{model.Type.ToString().ToLowerInvariant()}";
            return await HttpClient.GetAsync<ListResponse<Post>>(endpoint, parameters, token).ConfigureAwait(false);
        }

        public async Task<OperationResult<ListResponse<Post>>> GetPostsByCategoryAsync(PostsByCategoryModel model, CancellationToken token)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<ListResponse<Post>>(results);

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, model.Offset, model.Limit);
            AddLoginParameter(parameters, model.Login);
            AddCensorParameters(parameters, model);

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/posts/{model.Category}/{model.Type.ToString().ToLowerInvariant()}";
            return await HttpClient.GetAsync<ListResponse<Post>>(endpoint, parameters, token).ConfigureAwait(false);
        }

        public async Task<OperationResult<ListResponse<UserFriend>>> GetPostVotersAsync(VotersModel model, CancellationToken token)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<ListResponse<UserFriend>>(results);

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, model.Offset, model.Limit);
            AddVotersTypeParameters(parameters, model.Type);
            if (!string.IsNullOrEmpty(model.Login))
                AddLoginParameter(parameters, model.Login);

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/post/{model.Url}/voters";
            return await HttpClient.GetAsync<ListResponse<UserFriend>>(endpoint, parameters, token).ConfigureAwait(false);
        }

        public async Task<OperationResult<ListResponse<Post>>> GetCommentsAsync(NamedInfoModel model, CancellationToken token)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<ListResponse<Post>>(results);

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, model.Offset, model.Limit);
            AddLoginParameter(parameters, model.Login);

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/post/{model.Url}/comments";
            var resp = await HttpClient.GetAsync<ListResponse<Post>>(endpoint, parameters, token).ConfigureAwait(false);
            if (resp.IsSuccess)
                resp.Result.Results.ForEach(p => p.IsComment = true);

            return resp;
        }

        public async Task<OperationResult<UserProfileResponse>> GetUserProfileAsync(UserProfileModel model, CancellationToken token)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<UserProfileResponse>(results);

            var parameters = new Dictionary<string, object>();
            AddLoginParameter(parameters, model.Login);
            parameters.Add("show_nsfw", Convert.ToInt32(model.ShowNsfw));
            parameters.Add("show_low_rated", Convert.ToInt32(model.ShowLowRated));

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/user/{model.Username}/info";
            return await HttpClient.GetAsync<UserProfileResponse>(endpoint, parameters, token).ConfigureAwait(false);
        }

        public async Task<OperationResult<ListResponse<UserFriend>>> GetUserFriendsAsync(UserFriendsModel model, CancellationToken token)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<ListResponse<UserFriend>>(results);

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, model.Offset, model.Limit);
            AddLoginParameter(parameters, model.Login);

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/user/{model.Username}/{model.Type.ToString().ToLowerInvariant()}";
            return await HttpClient.GetAsync<ListResponse<UserFriend>>(endpoint, parameters, token).ConfigureAwait(false);
        }

        public async Task<OperationResult<Post>> GetPostInfoAsync(NamedInfoModel model, CancellationToken token)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<Post>(results);

            var parameters = new Dictionary<string, object>();
            AddLoginParameter(parameters, model.Login);
            AddCensorParameters(parameters, model);

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/post/{model.Url}/info";
            return await HttpClient.GetAsync<Post>(endpoint, parameters, token).ConfigureAwait(false);
        }

        public async Task<OperationResult<ListResponse<UserFriend>>> SearchUserAsync(SearchWithQueryModel model, CancellationToken token)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<ListResponse<UserFriend>>(results);

            var parameters = new Dictionary<string, object>();
            AddLoginParameter(parameters, model.Login);
            AddOffsetLimitParameters(parameters, model.Offset, model.Limit);
            parameters.Add("query", model.Query);

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/user/search";
            return await HttpClient.GetAsync<ListResponse<UserFriend>>(endpoint, parameters, token).ConfigureAwait(false);
        }

        public async Task<OperationResult<UserExistsResponse>> UserExistsCheckAsync(UserExistsModel model, CancellationToken token)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<UserExistsResponse>(results);

            var parameters = new Dictionary<string, object>();
            var endpoint = $"{BaseUrl}/{GatewayVersion.V1}/user/{model.Username}/exists";
            return await HttpClient.GetAsync<UserExistsResponse>(endpoint, parameters, token).ConfigureAwait(false);
        }

        public async Task<OperationResult<ListResponse<SearchResult>>> GetCategoriesAsync(OffsetLimitModel request, CancellationToken token)
        {
            var results = Validate(request);
            if (results != null)
                return new OperationResult<ListResponse<SearchResult>>(results);

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            var endpoint = $"{BaseUrl}/{GatewayVersion.V1}/categories/top";
            var result = await HttpClient.GetAsync<ListResponse<SearchResult>>(endpoint, parameters, token).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                foreach (var category in result.Result.Results)
                {
                    category.Name = Transliteration.ToRus(category.Name);
                }
            }
            return result;
        }

        public async Task<OperationResult<ListResponse<SearchResult>>> SearchCategoriesAsync(SearchWithQueryModel model, CancellationToken token)
        {
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
            var result = await HttpClient.GetAsync<ListResponse<SearchResult>>(endpoint, parameters, token).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                foreach (var categories in result.Result.Results)
                {
                    categories.Name = Transliteration.ToRus(categories.Name);
                }
            }

            return result;
        }

        public async Task<OperationResult<VoidResponse>> TraceAsync(string endpoint, string login, Exception resultException, string target, CancellationToken token)
        {
            try
            {
                var parameters = new Dictionary<string, object>();
                AddLoginParameter(parameters, login);
                parameters.Add("error", resultException == null ? string.Empty : resultException.Message);
                if (!string.IsNullOrEmpty(target))
                    parameters.Add("target", target);

                endpoint = $"{BaseUrl}/{GatewayVersion.V1}/log/{endpoint}";
                var result = await HttpClient.PutAsync<VoidResponse, Dictionary<string, object>>(endpoint, parameters, token).ConfigureAwait(false);
                if (result.IsSuccess)
                    result.Result = new VoidResponse();
                return result;
            }
            catch (Exception ex)
            {
                await LogService.WarningAsync(ex).ConfigureAwait(false);
            }
            return null;
        }


        private Beneficiary[] _beneficiariesCash;

        public async Task<Beneficiary[]> GetBeneficiariesAsync(CancellationToken token)
        {
            if (_beneficiariesCash == null)
            {
                var endpoint = $"{BaseUrl}/{GatewayVersion.V1}/beneficiaries";
                var result = await HttpClient.GetAsync<BeneficiariesResponse>(endpoint, token).ConfigureAwait(false);
                if (result.IsSuccess)
                    _beneficiariesCash = result.Result.Beneficiaries;
            }

            return _beneficiariesCash;
        }

        public async Task<OperationResult<SpamResponse>> CheckForSpamAsync(string username, CancellationToken token)
        {
            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/user/{username}/spam";
            var result = await HttpClient.GetAsync<SpamResponse>(endpoint, token).ConfigureAwait(false);
            return result;
        }

        public async Task<OperationResult<CurrencyRate[]>> GetCurrencyRatesAsync(CancellationToken token)
        {
            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/currency/rates";
            var result = await HttpClient.GetAsync<CurrencyRate[]>(endpoint, token).ConfigureAwait(false);
            return result;
        }

        #endregion Get requests

        public async Task<OperationResult<PreparePostResponse>> PreparePostAsync(PreparePostModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<PreparePostResponse>(results);

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/post/prepare";
            return await HttpClient.PutAsync<PreparePostResponse, PreparePostModel>(endpoint, model, ct).ConfigureAwait(false);
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

        protected ValidationException Validate<T>(T request)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(request);
            Validator.TryValidateObject(request, context, results, true);
            if (results.Any())
            {
                var msg = results.Select(m => m.ErrorMessage).First();
                return new ValidationException(msg);
            }
            return null;
        }

        public async Task<OperationResult<SubscriptionsModel>> CheckSubscriptionsAsync(User user, CancellationToken ct)
        {
            if (!user.HasPostingPermission || string.IsNullOrEmpty(user.PushesPlayerId))
                return new OperationResult<SubscriptionsModel>(new NullReferenceException(nameof(user.PushesPlayerId)));

            var endpoint = $"{BaseUrl}/{GatewayVersion.V1P1}/subscriptions/{user.Login}/{user.PushesPlayerId}";
            return await HttpClient.GetAsync<SubscriptionsModel>(endpoint, ct).ConfigureAwait(false);
        }

        public async Task<OperationResult<PromoteResponse>> FindPromoteBotAsync(PromoteRequest promoteModel, CancellationToken ct)
        {
            var botsResponse = await HttpClient.GetAsync<List<BidBot>>("https://steembottracker.net/bid_bots", ct).ConfigureAwait(false);
            if (!botsResponse.IsSuccess)
                return new OperationResult<PromoteResponse>(botsResponse.Exception);

            var priceResponse = await HttpClient.GetAsync<Price>("https://postpromoter.net/api/prices", ct).ConfigureAwait(false);
            if (!priceResponse.IsSuccess)
                return new OperationResult<PromoteResponse>(priceResponse.Exception);

            var steemToUsd = priceResponse.Result.SteemPrice;
            var sbdToUsd = priceResponse.Result.SbdPrice;

            var votersModel = new VotersModel(promoteModel.PostToPromote.Url, VotersType.Likes);
            var usersResult = await GetPostVotersAsync(votersModel, ct).ConfigureAwait(false);
            if (!usersResult.IsSuccess)
                return new OperationResult<PromoteResponse>(usersResult.Exception);


            var postAge = (DateTime.Now - promoteModel.PostToPromote.Created).TotalDays;

            var suitableBot = botsResponse.Result
                                          .Where(x => CheckBot(x, postAge, promoteModel, steemToUsd, sbdToUsd, usersResult.Result.Results))
                                          .OrderBy(x => x.Next)
                                          .FirstOrDefault();

            if (suitableBot == null)
                return new OperationResult<PromoteResponse>(new ValidationException());

            var response = await SearchUserAsync(new SearchWithQueryModel(suitableBot.Name), ct).ConfigureAwait(false);

            if (!response.IsSuccess)
                return new OperationResult<PromoteResponse>(response.Exception);

            var promoteResponse = new PromoteResponse(response.Result.Results.First(), TimeSpan.FromMilliseconds(suitableBot.Next ?? 0));
            return new OperationResult<PromoteResponse>(promoteResponse);
        }

        private bool CheckBot(BidBot bot, double postAge, PromoteRequest promoteModel, double steemToUsd, double sbdToUsd, List<UserFriend> users)
        {
            return !bot.IsDisabled &&
                   Constants.SupportedListBots.Contains(bot.Name) &&
                  (!bot.MaxPostAge.HasValue || postAge < TimeSpan.FromDays(bot.MaxPostAge.Value).TotalDays) &&
                  (!bot.MinPostAge.HasValue || postAge > TimeSpan.FromMinutes(bot.MinPostAge.Value).TotalDays) &&
                  CheckAmount(promoteModel.Amount, steemToUsd, sbdToUsd, promoteModel.CurrencyType, bot) &&
                  !users.Any(r => r.Author.Equals(bot.Name)) &&
                  (promoteModel.CurrencyType == CurrencyType.Sbd
                   ? (bot.MinBid.HasValue && bot.MinBid <= promoteModel.Amount)
                   : (bot.MinBidSteem.HasValue && bot.AcceptsSteem && bot.MinBidSteem <= promoteModel.Amount));
        }


        private bool CheckAmount(double promoteAmount, double steemToUsd, double sbdToUsd, CurrencyType token, BidBot botInfo)
        {
            var amountLimit = botInfo.VoteUsd;
            var bidsAmountInBot = botInfo.TotalUsd;
            double userBidInUsd;
            switch (token)
            {
                case CurrencyType.Steem:
                    userBidInUsd = promoteAmount * steemToUsd;
                    break;
                case CurrencyType.Sbd:
                    userBidInUsd = promoteAmount * sbdToUsd;
                    break;
                default:
                    return false;
            }
            return (userBidInUsd + bidsAmountInBot) < amountLimit - (amountLimit * 0.25);
        }
    }
}
