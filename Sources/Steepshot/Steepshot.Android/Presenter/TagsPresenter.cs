using System.Threading.Tasks;
using Steepshot.Base;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.View;

namespace Steepshot.Presenter
{
	public class TagsPresenter : BasePresenter
	{
		public TagsPresenter(TagsView view):base(view)
		{
		}

		public async Task<OperationResult<SearchResponse<SearchResult>>> SearchTags(string s)
		{

			var request = new SearchWithQueryRequest(s);

			return await Api.SearchCategories(request, null);
		}

		public async Task<OperationResult<SearchResponse<SearchResult>>> GetTopTags()
		{
			var request = new SearchRequest();
			return await Api.GetCategories(request, null);
		}
	}
}
