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

        private async Task<List<string>> LoadNextComments(CancellationToken ct, string postUrl)
        {
            var request = new NamedInfoRequest(postUrl)
            {
                Login = User.Login
            };

            var response = await Api.GetComments(request, ct);
            if (response == null)
                return null;

            if (response.Success)
            {
                var results = response.Result.Results;
                if (results.Count > 0)
                {
                    lock (Items)
                    {
                        //Comments work wrong so...
                        //Items.AddRange(string.IsNullOrEmpty(OffsetUrl) ? results : results.Skip(1));
                        if (Items.Any())
                        {
                            var range = Items.Union(results);
                            Items.Clear();
                            Items.AddRange(range);
                        }
                        else
                            Items.AddRange(results);
                    }

                    OffsetUrl = results.Last().Url;
                }
                if (results.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
                Items.RemoveAll(i => User.PostBlackList.Contains(i.Url));
                NotifySourceChanged();
            }
            return response.Errors;
        }

        public async Task<OperationResult<CreateCommentResponse>> TryCreateComment(string comment, string url)
        {
            return await TryRunTask<string, string, CreateCommentResponse>(CreateComment, OnDisposeCts.Token, comment, url);
        }

        private async Task<OperationResult<CreateCommentResponse>> CreateComment(CancellationToken ct, string comment, string url)
        {
            var reqv = new CreateCommentRequest(User.UserInfo, url, comment, AppSettings.AppInfo);
            return await Api.CreateComment(reqv, ct);
        }
    }
}
