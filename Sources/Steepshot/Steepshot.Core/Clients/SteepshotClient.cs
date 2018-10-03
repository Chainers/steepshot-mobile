using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Clients
{
    public sealed class SteepshotClient
    {
        private readonly ExtendedHttpClient _httpClient;


        public SteepshotClient(ExtendedHttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        public async Task<OperationResult<CreateAccountResponse>> CreateAccountAsync(CreateAccountModel model, CancellationToken token)
        {
            var endpoint = "https://createacc.steepshot.org/api/v1/account";
            return await _httpClient.PostAsync<CreateAccountResponse, CreateAccountModel>(endpoint, model, token).ConfigureAwait(false);
        }

        public async Task<OperationResult<CreateAccountResponse>> ResendEmailAsync(CreateAccountModel model, CancellationToken token)
        {
            var endpoint = "https://createacc.steepshot.org/api/v1/resend-mail";
            return await _httpClient.PostAsync<CreateAccountResponse, CreateAccountModel>(endpoint, model, token).ConfigureAwait(false);
        }

        public async Task<OperationResult<string>> CheckRegistrationServiceStatusAsync(CancellationToken token)
        {
            return await _httpClient.GetAsync<string>("https://createacc.steepshot.org/api/v1/active", token).ConfigureAwait(false);
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

        public async Task<OperationResult<UUIDModel>> UploadMediaAsync(UploadMediaModel model, CancellationToken ct)
        {
            var results = Validate(model);
            if (results != null)
                return new OperationResult<UUIDModel>(results);

            var endpoint = "https://media.steepshot.org/api/v1/upload";
            return await _httpClient.UploadMediaAsync(endpoint, model, ct).ConfigureAwait(false);
        }

    }
}