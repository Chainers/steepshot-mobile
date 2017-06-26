using Sweetshot.Library.Models.Common;
using System.Threading.Tasks;
using Sweetshot.Library.Models.Responses;
using Sweetshot.Library.Models.Requests;
using System.Threading;

namespace Steepshot
{
	public class SearchPresenter : BasePresenter
	{
		public SearchPresenter(SearchView view):base(view) { }
		private CancellationTokenSource cts;
		private string _prevQuery;

		public async Task<OperationResult<SearchResponse>> SearchCategories(string s)
		{
			if (_prevQuery == s)
				return new OperationResult<SearchResponse>();
				
			_prevQuery = s;
			using (cts = new CancellationTokenSource())
			{
				var request = new SearchWithQueryRequest(s);
				if(!string.IsNullOrEmpty(s))
					return await Api.SearchCategories(request, cts);
				return await Api.GetCategories(request, cts);
			}
		}
	}
}
