using Sweetshot.Library.Models.Requests.Common;

namespace Sweetshot.Library.Models.Requests
{
    public class CategoriesRequest : OffsetLimitFields
    {
    }

    public class SearchCategoriesRequest : CategoriesRequest
    {
        public SearchCategoriesRequest(string query)
        {
            Query = query;
        }

        public string Query { get; private set; }
    }
}