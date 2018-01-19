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

        private async Task<ErrorBase> LoadNextComments(string postUrl, CancellationToken ct)
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

        public async Task<OperationResult<VoidResponse>> TryCreateComment(string comment, string url)
        {
            return await TryRunTask<string, string, VoidResponse>(CreateComment, OnDisposeCts.Token, comment, url);
        }

        private async Task<OperationResult<VoidResponse>> CreateComment(string comment, string url, CancellationToken ct)
        {
            var reqv = new CreateCommentModel(User.UserInfo, url, comment, AppSettings.AppInfo);
            return await Api.CreateComment(reqv, ct);
        }
    }
}
