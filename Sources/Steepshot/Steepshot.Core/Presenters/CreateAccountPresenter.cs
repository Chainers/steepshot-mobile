using System.Threading.Tasks;
using Steepshot.Core.Clients;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public sealed class CreateAccountPresenter : ListPresenter<string>
    {
        private readonly SteepshotApiClient _steepshotApiClient;
        private readonly SteepshotClient _steepshotClient;

        public CreateAccountPresenter(IConnectionService connectionService, ILogService logService, SteepshotApiClient steepshotApiClient, SteepshotClient steepshotClient)
            : base(connectionService, logService)
        {
            _steepshotApiClient = steepshotApiClient;
            _steepshotClient = steepshotClient;
        }

        public async Task<OperationResult<UserProfileResponse>> TryGetAccountInfoAsync(string login)
        {
            var req = new UserProfileModel(login);
            return await RunAsSingleTaskAsync(_steepshotApiClient.GetUserProfileAsync, req)
                .ConfigureAwait(false);
        }

        public async Task<OperationResult<CreateAccountResponse>> TryCreateAccountAsync(CreateAccountModel account)
        {
            return await RunAsSingleTaskAsync(_steepshotClient.CreateAccountAsync, account).ConfigureAwait(false);
        }

        public async Task<OperationResult<CreateAccountResponse>> TryResendMailAsync(CreateAccountModel account)
        {
            return await RunAsSingleTaskAsync(_steepshotClient.ResendEmailAsync, account).ConfigureAwait(false);
        }
    }
}