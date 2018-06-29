using System.Threading.Tasks;
using Steepshot.Core.Errors;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;

namespace Steepshot.Core.Facades
{
    public sealed class TransferFacade
    {
        public UserFriendPresenter UserFriendPresenter { get; }
        private readonly PreSignInPresenter _preSignInPresenter;

        public TransferFacade()
        {
            UserFriendPresenter = new UserFriendPresenter();
            _preSignInPresenter = new PreSignInPresenter();
        }

        public async Task<ErrorBase> TryLoadNextSearchUser(string query) => await UserFriendPresenter.TryLoadNextSearchUser(query);
        public async Task<OperationResult<AccountInfoResponse>> TryGetAccountInfo(string login) => await _preSignInPresenter.TryGetAccountInfo(login);

        public void TasksCancel(bool andDispose = false)
        {
            _preSignInPresenter.TasksCancel(andDispose);
            UserFriendPresenter.TasksCancel(andDispose);
        }
    }
}
