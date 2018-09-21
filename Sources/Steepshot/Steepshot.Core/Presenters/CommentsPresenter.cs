using System;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Core.JsonRpc;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public sealed class CommentsPresenter : BasePostPresenter
    {
        private const int ItemsLimit = 60;

        public async Task<Exception> TryLoadNextCommentsAsync(Post post)
        {
            return await RunAsSingleTaskAsync(LoadNextCommentsAsync, post).ConfigureAwait(false);
        }

        private async Task<Exception> LoadNextCommentsAsync(Post post, CancellationToken ct)
        {
            var request = new NamedInfoModel(post.Url)
            {
                Login = AppSettings.User.Login
            };

            Exception exception;
            var isNeedClearItems = true;
            bool isNeedRepeat;
            do
            {
                var response = await Api.GetCommentsAsync(request, ct).ConfigureAwait(false);
                isNeedRepeat = ResponseProcessing(response, ItemsLimit, out exception, nameof(TryLoadNextCommentsAsync), isNeedClearItems, true);
                isNeedClearItems = false;
            } while (isNeedRepeat);

            return exception;
        }

        public async Task<OperationResult<VoidResponse>> TryCreateCommentAsync(Post parentPost, string body)
        {
            var model = new CreateOrEditCommentModel(AppSettings.User.UserInfo, parentPost, body, AppSettings.AppInfo);
            return await TryRunTaskAsync<CreateOrEditCommentModel, VoidResponse>(CreateCommentAsync, OnDisposeCts.Token, model).ConfigureAwait(false);
        }

        private async Task<OperationResult<VoidResponse>> CreateCommentAsync(CreateOrEditCommentModel model, CancellationToken ct)
        {
            return await Api.CreateOrEditCommentAsync(model, ct).ConfigureAwait(false);
        }
    }
}
