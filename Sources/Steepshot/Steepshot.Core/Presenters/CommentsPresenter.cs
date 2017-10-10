using System;
using System.Collections.Generic;
using System.Linq;
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

        private async Task<List<string>> LoadNextComments(CancellationTokenSource cts, string postUrl)
        {
            var request = new NamedInfoRequest(postUrl)
            {
                Login = User.Login
            };

            var response = await Api.GetComments(request);
            if (response.Success)
            {
                var results = response.Result.Results;
                if (results.Count > 0)
                {
                    lock (Posts)
                        Posts.AddRange(string.IsNullOrEmpty(OffsetUrl) ? results : results.Skip(1));

                    OffsetUrl = results.Last().Url;
                }
                if (results.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
            }
            return response.Errors;
        }

        public async Task<OperationResult<CreateCommentResponse>> TryCreateComment(string comment, string url)
        {
            return await TryRunTask(CreateComment, CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None), comment, url);
        }

        private async Task<OperationResult<CreateCommentResponse>> CreateComment(CancellationTokenSource cts, string comment, string url)
        {
            var reqv = new CreateCommentRequest(User.UserInfo, url, comment, AppSettings.AppInfo);
            return await Api.CreateComment(reqv, cts);
        }
    }
}