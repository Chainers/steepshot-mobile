using System.Threading.Tasks;
using Steepshot.Base;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Presenter
{
	public class TagsPresenter : BasePresenter
	{
		public TagsPresenter(IBaseView view):base(view)
		{
		}

		public async Task<OperationResult<SearchResponse<SearchResult>>> SearchTags(string s)
		{
		    var request = new SearchWithQueryRequest(s)
		    {
		        SessionId = User.CurrentUser.SessionId,
		        Login = User.CurrentUser.Login
		    };

            return await Api.SearchCategories(request, null);
		}

		public async Task<OperationResult<SearchResponse<SearchResult>>> GetTopTags()
		{
			var request = new SearchRequest();
			return await Api.GetCategories(request, null);
		}
	}
}
