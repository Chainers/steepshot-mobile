using System;

namespace Sweetshot.Library.Models.Requests
{
    public class CategoriesRequest
    {
        public string Offset { get; set; }
        public int Limit { get; set; }
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