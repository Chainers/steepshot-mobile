using System;
using Sweetshot.Library.Models.Common;
using System.Threading.Tasks;
using Sweetshot.Library.Models.Responses;
using Sweetshot.Library.Models.Requests;

namespace Steepshot
{
	public class SearchPresenter : BasePresenter
	{
		public SearchPresenter(SearchView view):base(view)
		{
		}

		public async Task<OperationResult<SearchResponse>> SearchCategories(string s)
		{
			var request = new SearchWithQueryRequest(s);
			return await Api.SearchCategories(request);
		}
	}
}
