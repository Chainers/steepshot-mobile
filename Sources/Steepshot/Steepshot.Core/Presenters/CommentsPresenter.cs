using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public class CommentsPresenter : BasePostPresenter
    {
        private const int ItemsLimit = 60;

        public async Task<List<string>> TryLoadNextComments(string postUrl)
        {
            return await RunAsSingleTask(LoadNextComments, postUrl);
        }

        private async Task<List<string>> LoadNextComments(CancellationToken ct, string postUrl)
        {
            var request = new NamedInfoRequest(postUrl)
            {
                Login = User.Login
            };

            List<string> errors;
            OperationResult<UserPostResponse> response;
            bool isNeedRepeat;
            do
            {
                response = await Api.GetComments(request, ct);
                Clear();
                isNeedRepeat = ResponseProcessing(response, ItemsLimit, out errors);
            } while (isNeedRepeat);

            return errors;
        }

        public async Task<OperationResult<CommentResponse>> TryCreateComment(string comment, string url)
        {
            return await TryRunTask<string, string, CommentResponse>(CreateComment, OnDisposeCts.Token, comment, url);
        }

        private async Task<OperationResult<CommentResponse>> CreateComment(CancellationToken ct, string comment, string url)
        {
            var reqv = new CommentRequest(User.UserInfo, url, comment, AppSettings.AppInfo);
            return await Api.CreateComment(reqv, ct);
        }
    }
}
