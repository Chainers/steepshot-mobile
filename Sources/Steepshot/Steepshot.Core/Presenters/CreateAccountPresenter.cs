using System;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Presenters
{
    public sealed class CreateAccountPresenter : ListPresenter<string>
    {
        public async Task<Exception> TryGetAccountInfoAsync(string login)
        {
            return await RunAsSingleTaskAsync(GetAccountInfoAsync, login).ConfigureAwait(false);
        }

        private async Task<Exception> GetAccountInfoAsync(string login, CancellationToken ct)
        {
            var req = new UserProfileModel(login);
            var response = await Api.GetUserProfileAsync(req, ct).ConfigureAwait(false);
            return response.Exception;
        }

        public async Task<Exception> TryCreateAccountAsync(CreateAccountModel account)
        {
            return await RunAsSingleTaskAsync(CreateAccountAsync, account).ConfigureAwait(false);
        }

        private async Task<Exception> CreateAccountAsync(CreateAccountModel account, CancellationToken ct)
        {
            var response = await Api.CreateAccountAsync(account, ct).ConfigureAwait(false);
            return response.Exception;
        }

        public async Task<Exception> TryResendMailAsync(CreateAccountModel account)
        {
            return await RunAsSingleTaskAsync(ResendMailAsync, account).ConfigureAwait(false);
        }

        private async Task<Exception> ResendMailAsync(CreateAccountModel account, CancellationToken ct)
        {
            var response = await Api.ResendEmailAsync(account, ct).ConfigureAwait(false);
            return response.Exception;
        }
    }
}
