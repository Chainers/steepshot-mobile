using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Errors;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Presenters
{
    public class SinglePostPresenter : BasePostPresenter
    {
        public Post PostInfo { get; private set; }

        public async Task<ErrorBase> TryLoadPostInfo(string url) => await TryRunTask(LoadPostInfo, OnDisposeCts.Token, url);

        private async Task<ErrorBase> LoadPostInfo(string url, CancellationToken ct)
        {
            var request = new NamedInfoModel(url) { Login = User.Login };

            var response = await Api.GetPostInfo(request, ct);

            if (response.IsSuccess)
            {
                PostInfo = response.Result;
                PostInfo = CashPresenterManager.Add(PostInfo);
                NotifySourceChanged(nameof(TryLoadPostInfo), true);
            }
            return response.Error;
        }
    }
}
