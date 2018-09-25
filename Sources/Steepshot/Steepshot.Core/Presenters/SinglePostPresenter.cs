using System;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public class SinglePostPresenter : BasePostPresenter
    {
        public Post PostInfo { get; private set; }

        public async Task<Exception> TryLoadPostInfoAsync(string url)
        {
            return await TryRunTaskAsync(LoadPostInfoAsync, OnDisposeCts.Token, url).ConfigureAwait(false);
        }

        private async Task<Exception> LoadPostInfoAsync(string url, CancellationToken ct)
        {
            var request = new NamedInfoModel(url)
            {
                ShowNsfw = AppSettings.User.IsNsfw,
                ShowLowRated = AppSettings.User.IsLowRated,
                Login = AppSettings.User.Login
            };

            var response = await Api.GetPostInfoAsync(request, ct).ConfigureAwait(false);
            var exception = ResponseProcessing(response, nameof(TryLoadPostInfoAsync));

            return exception;
        }

        protected Exception ResponseProcessing(OperationResult<Post> response, string sender)
        {
            if (response == null)
                return null;

            if (response.IsSuccess)
            {
                var isAdded = false;
                var item = response.Result;
                if (IsValidMedia(item))
                {
                    CashPresenterManager.Add(item);
                    PostInfo = item;
                    isAdded = true;
                }

                NotifySourceChanged(sender, isAdded);
            }
            return response.Exception;
        }
    }
}
