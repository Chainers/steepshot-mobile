using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Presenters
{
    public class SearchPresenter : BasePresenter
    {
        private CancellationTokenSource _cts;

        public async Task<OperationResult> SearchCategories(string s, SearchType searchType)
        {
            using (_cts = new CancellationTokenSource())
            {
                if (string.IsNullOrEmpty(s))
                {
                    var request = new OffsetLimitFields();
                    return await Api.GetCategories(request, _cts);
                }
                else
                {
                    var request = new SearchWithQueryRequest(s);
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