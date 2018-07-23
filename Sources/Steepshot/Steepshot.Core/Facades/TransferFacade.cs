using System;
using System.Threading.Tasks;
using Steepshot.Core.Clients;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;

namespace Steepshot.Core.Facades
{
    public sealed class TransferFacade
    {
        private readonly PreSignInPresenter _preSignInPresenter;
        public UserFriendPresenter UserFriendPresenter { get; }
        public TransferPresenter TransferPresenter { get; }
        public Action OnUserBalanceChanged;
        public Action OnRecipientChanged;

        private BalanceModel _userBalance;
        public BalanceModel UserBalance
        {
            get => _userBalance;
            set
            {
                _userBalance = value;
                OnUserBalanceChanged?.Invoke();
            }
        }

        private UserFriend _recipient;
        public UserFriend Recipient
        {
            get => _recipient;
            set
            {
                _recipient = value;
                OnRecipientChanged?.Invoke();
            }
        }

        public TransferFacade()
        {
            UserFriendPresenter = new UserFriendPresenter();
            _preSignInPresenter = new PreSignInPresenter();
            TransferPresenter = new TransferPresenter();
        }

        public void SetClient(SteepshotApiClient client)
        {
            UserFriendPresenter.SetClient(client);
            TransferPresenter.SetClient(client);
            _preSignInPresenter.SetClient(client);
        }



        public async Task<Exception> TryLoadNextSearchUser(string query) => await UserFriendPresenter.TryLoadNextSearchUser(query);
        public async Task<OperationResult<AccountInfoResponse>> TryGetAccountInfo(string login)
        {
            return await _preSignInPresenter.TryGetAccountInfo(login);
        }

        public void TasksCancel()
        {
            _preSignInPresenter.TasksCancel();
            UserFriendPresenter.TasksCancel();
        }
    }
}
