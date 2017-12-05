using System.Collections.Generic;

namespace Steepshot.Core.Models.Responses
{
    public class ListResponce<T>
    {
        public List<T> Results { get; set; }

        //addition fields (unused)
        public int Count { get; set; }
        public string Offset { get; set; }
        public int TotalCount { get; set; }
    }
}
