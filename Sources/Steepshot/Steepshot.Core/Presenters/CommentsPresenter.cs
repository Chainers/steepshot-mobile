using System.Threading;
using System.Threading.Tasks;
using Ditch.Core.JsonRpc;
using Steepshot.Core.Errors;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public sealed class CommentsPresenter : BasePostPresenter
    {
        private const int ItemsLimit = 60;

        public async Task<ErrorBase> TryLoadNextComments(Post post)
        {
            return await RunAsSingleTask(LoadNextComments, post);
        }

        private async Task<ErrorBase> LoadNextComments(Post post, CancellationToken ct)
        {
            var request = new NamedInfoModel(post.Url)
            {
                Login = AppSettings.User.Login
            };

            ErrorBase error;
            var isNeedClearItems = true;
            bool isNeedRepeat;
            do
            {
                var response = await Api.GetComments(request, ct);
                isNeedRepeat = ResponseProcessing(response, ItemsLimit, out error, nameof(TryLoadNextComments), isNeedClearItems, true);
                isNeedClearItems = false;
            } while (isNeedRepeat);

            return error;
        }

        public async Task<OperationResult<VoidResponse>> TryCreateComment(Post parentPost, string body)
        {
            var model = new CreateOrEditCommentModel(AppSettings.User.UserInfo, parentPost, body, AppSettings.AppInfo);
            return await TryRunTask<CreateOrEditCommentModel, VoidResponse>(CreateComment, OnDisposeCts.Token, model);
        }

        private async Task<OperationResult<VoidResponse>> CreateComment(CreateOrEditCommentModel model, CancellationToken ct)
        {
            return await Api.CreateOrEditComment(model, ct);
        }
    }
}
