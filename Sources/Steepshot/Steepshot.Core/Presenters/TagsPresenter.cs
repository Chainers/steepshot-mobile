using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public class TagsPresenter : BasePresenter
    {
        public async Task<OperationResult<SearchResponse<SearchResult>>> TrySearchTags(string s, CancellationTokenSource cts = null)
        {
            if (cts == null)
                cts = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None);
            return await TryRunTask(SearchTags, cts, s);
        }

        private async Task<OperationResult<SearchResponse<SearchResult>>> SearchTags(CancellationTokenSource cts, string s)
        {
            var request = new SearchWithQueryRequest(s);
            return await Api.SearchCategories(request, cts);
        }

        public async Task<OperationResult<SearchResponse<SearchResult>>> TryGetTopTags(CancellationTokenSource cts = null)
        {
            if (cts == null)
                cts = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None);
            return await TryRunTask(GetTopTags, cts);
        }

        private async Task<OperationResult<SearchResponse<SearchResult>>> GetTopTags(CancellationTokenSource cts)
        {
            var request = new OffsetLimitFields();
            return await Api.GetCategories(request, cts);
        }
    }
}