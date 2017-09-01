using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public class TagsPresenter : BasePresenter
    {
        public async Task<OperationResult<SearchResponse<SearchResult>>> SearchTags(string s)
        {
            var request = new SearchWithQueryRequest(s);
            return await Api.SearchCategories(request);
        }

        public async Task<OperationResult<SearchResponse<SearchResult>>> GetTopTags()
        {
            var request = new OffsetLimitFields();
            return await Api.GetCategories(request);
        }
    }
}