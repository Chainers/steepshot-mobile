using System.Threading;
using System.Threading.Tasks;
using Steepshot.Base;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Fragment;
using Steepshot.View;

namespace Steepshot.Presenter
{
    public class SearchPresenter : BasePresenter
    {
        public SearchPresenter(ISearchView view) : base(view) { }
        private CancellationTokenSource _cts;

        public async Task<OperationResult> SearchCategories(string s, SearchType searchType)
        {
            using (_cts = new CancellationTokenSource())
            {
                if (string.IsNullOrEmpty(s))
                {
                    var request = new SearchRequest() { };
                    return await Api.GetCategories(request, _cts);
                }
                else
                {
                    var request = new SearchWithQueryRequest(s)
                    {
                        SessionId = User.CurrentUser.SessionId,
                        Login = User.CurrentUser.Login
                    };
                    if (searchType == SearchType.Tags)
                    {
                        return await Api.SearchCategories(request, _cts);
                    }
                    return await Api.SearchUser(request, _cts);
                }
            }
        }
    }
}
