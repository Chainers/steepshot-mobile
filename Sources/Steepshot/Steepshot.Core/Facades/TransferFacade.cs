using System;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Facades
{
    public sealed class TransferFacade
    {
        public readonly UserFriendPresenter UserFriendPresenter;
        public readonly TransferPresenter TransferPresenter;

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

        public TransferFacade(UserFriendPresenter userFriendPresenter, TransferPresenter transferPresenter)
        {
            UserFriendPresenter = userFriendPresenter;
            TransferPresenter = transferPresenter;
        }


        public async Task<OperationResult<ListResponse<UserFriend>>> TryLoadNextSearchUserAsync(string query)
        {
            return await UserFriendPresenter.TryLoadNextSearchUserAsync(query).ConfigureAwait(false);
        }

        public async Task<OperationResult<AccountInfoResponse>> TryGetAccountInfoAsync(string login)
        {
            return await TransferPresenter.TryGetAccountInfoAsync(login).ConfigureAwait(false);
        }

        public void TasksCancel()
        {
            TransferPresenter.TasksCancel();
            UserFriendPresenter.TasksCancel();
        }
    }
}
