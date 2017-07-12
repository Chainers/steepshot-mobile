using System;
using System.Threading.Tasks;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;

namespace Steepshot
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
