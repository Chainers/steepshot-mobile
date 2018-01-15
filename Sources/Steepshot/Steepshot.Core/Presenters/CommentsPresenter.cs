using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;
using Steepshot.Core.Errors;

namespace Steepshot.Core.Presenters
{
    public class CommentsPresenter : BasePostPresenter
    {
        private const int ItemsLimit = 60;

        public async Task<ErrorBase> TryLoadNextComments(string postUrl)
        {
            return await RunAsSingleTask(LoadNextComments, postUrl);
        }

        private async Task<ErrorBase> LoadNextComments(CancellationToken ct, string postUrl)
        {
            var request = new NamedInfoModel(postUrl)
            {
                Login = User.Login
            };

            ErrorBase error;
            var isNeedClearItems = true;
            bool isNeedRepeat;
            do
            {
                var response = await Api.GetComments(request, ct);
                isNeedRepeat = ResponseProcessing(response, ItemsLimit, out error, nameof(TryLoadNextComments), isNeedClearItems);
                isNeedClearItems = false;
            } while (isNeedRepeat);

            return error;
        }

        public async Task<OperationResult<CommentResponse>> TryCreateComment(string comment, string url)
        {
            return await TryRunTask<string, string, CommentResponse>(CreateComment, OnDisposeCts.Token, comment, url);
        }

        private async Task<OperationResult<CommentResponse>> CreateComment(CancellationToken ct, string comment, string url)
        {
            var reqv = new CommentModel(User.UserInfo, url, comment, AppSettings.AppInfo);
            return await Api.CreateComment(reqv, ct);
        }
    }
}
