using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Errors;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public class SinglePostPresenter : BasePostPresenter
    {
        public Post PostInfo { get; private set; }

        public async Task<ErrorBase> TryLoadPostInfo(string url) => await TryRunTask(LoadPostInfo, OnDisposeCts.Token, url);

        private async Task<ErrorBase> LoadPostInfo(string url, CancellationToken ct)
        {
            var request = new NamedInfoModel(url) { Login = AppSettings.User.Login };

            var response = await Api.GetPostInfo(request, ct);
            var error = ResponseProcessing(response, nameof(TryLoadPostInfo));

            return error;
        }

        protected ErrorBase ResponseProcessing(OperationResult<Post> response, string sender)
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
            return response.Error;
        }
    }
}
