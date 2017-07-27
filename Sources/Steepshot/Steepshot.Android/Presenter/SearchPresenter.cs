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
        public SearchPresenter(SearchView view) : base(view) { }
        private CancellationTokenSource cts;

        public async Task<OperationResult> SearchCategories(string s, SearchType searchType)
        {
            using (cts = new CancellationTokenSource())
            {
                if (string.IsNullOrEmpty(s))
                {
                    var request = new SearchRequest() { };
                    return await Api.GetCategories(request, cts);
                }
                else
                {
                    var request = new SearchWithQueryRequest(s, User.CurrentUser);
                    if (searchType == SearchType.Tags)
                    {
                        return await Api.SearchCategories(request, cts);
                    }
                    return await Api.SearchUser(request, cts);
                }
            }
        }
    }
}
