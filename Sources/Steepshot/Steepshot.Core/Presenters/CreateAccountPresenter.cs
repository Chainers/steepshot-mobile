using System;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Presenters
{
    public sealed class CreateAccountPresenter : ListPresenter<string>
    {
        public async Task<Exception> TryGetAccountInfo(string login)
        {
            return await RunAsSingleTask(GetAccountInfo, login);
        }

        private async Task<Exception> GetAccountInfo(string login, CancellationToken ct)
        {
            var req = new UserProfileModel(login);
            var response = await Api.GetUserProfile(req, ct);
            return response.Error;
        }

        public async Task<Exception> TryCreateAccount(CreateAccountModel account)
        {
            return await RunAsSingleTask(CreateAccount, account);
        }

        private async Task<Exception> CreateAccount(CreateAccountModel account, CancellationToken ct)
        {
            var response = await Api.CreateAccount(account, ct);
            return response.Error;
        }

        public async Task<Exception> TryResendMail(CreateAccountModel account)
        {
            return await RunAsSingleTask(ResendMail, account);
        }

        private async Task<Exception> ResendMail(CreateAccountModel account, CancellationToken ct)
        {
            var response = await Api.ResendEmail(account, ct);
            return response.Error;
        }
    }
}
