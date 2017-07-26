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
					var request = new SearchWithQueryRequest(s) { SessionId = User.SessionId };
					if (searchType == SearchType.Tags)
					{
						return await Api.SearchCategories(request, cts);
					}
					else
					{
						return await Api.SearchUser(request, cts);
					}
				}
			}
		}
	}
}
