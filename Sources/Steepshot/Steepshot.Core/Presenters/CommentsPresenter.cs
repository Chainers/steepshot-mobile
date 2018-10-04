using System;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Core.JsonRpc;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Presenters
{
    public sealed class CommentsPresenter : BasePostPresenter
    {
        private const int ItemsLimit = 60;
        private readonly IAppInfo _appInfo;

        public CommentsPresenter(IConnectionService connectionService, ILogService logService, BaseDitchClient ditchClient, SteepshotApiClient steepshotApiClient, User user, SteepshotClient steepshotClient, IAppInfo appInfo)
            : base(connectionService, logService, ditchClient, steepshotApiClient, user, steepshotClient)
        {
            _appInfo = appInfo;
        }

        public async Task<Exception> TryLoadNextCommentsAsync(Post post)
        {
            var request = new NamedInfoModel(post.Url)
            {
                Login = User.Login
            };

            Exception exception;
            var isNeedClearItems = true;
            bool isNeedRepeat;
            do
            {
                var response = await RunAsSingleTaskAsync(SteepshotApiClient.GetCommentsAsync, request)
                    .ConfigureAwait(false);
                isNeedRepeat = ResponseProcessing(response, ItemsLimit, out exception, nameof(TryLoadNextCommentsAsync), isNeedClearItems, true);
                isNeedClearItems = false;
            } while (isNeedRepeat);

            return exception;
        }

        public async Task<OperationResult<VoidResponse>> TryCreateCommentAsync(Post parentPost, string body)
        {
            var model = new CreateOrEditCommentModel(User.UserInfo, parentPost, body, _appInfo);
            return await TaskHelper.TryRunTaskAsync(CreateOrEditCommentAsync, model, OnDisposeCts.Token).ConfigureAwait(false);
        }

        public async Task<OperationResult<VoidResponse>> TryEditCommentAsync(UserInfo userInfo, Post parentPost, Post post, string body, IAppInfo appInfo)
        {
            if (string.IsNullOrEmpty(body) || parentPost == null || post == null)
                return null;

            var model = new CreateOrEditCommentModel(userInfo, parentPost, post, body, appInfo);
            var response = await TaskHelper.TryRunTaskAsync(CreateOrEditCommentAsync, model, OnDisposeCts.Token).ConfigureAwait(false);
            if (response.IsSuccess)
                post.Body = model.Body;

            NotifySourceChanged(nameof(TryEditCommentAsync), true);
            return response;
        }

        private async Task<OperationResult<VoidResponse>> CreateOrEditCommentAsync(CreateOrEditCommentModel model, CancellationToken ct)
        {
            if (!model.IsEditMode)
                model.Beneficiaries = await SteepshotApiClient.GetBeneficiariesAsync(ct).ConfigureAwait(false);

            var result = await DitchClient.CreateOrEditAsync(model, ct).ConfigureAwait(false);
            //log parent post to perform update
            await SteepshotApiClient.TraceAsync($"post/@{model.ParentAuthor}/{model.ParentPermlink}/comment", model.Login, result.Exception, $"@{model.ParentAuthor}/{model.ParentPermlink}", ct).ConfigureAwait(false);
            return result;
        }
    }
}
