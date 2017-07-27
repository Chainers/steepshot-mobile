using System.Collections.Generic;

namespace Steepshot.Core.Models.Responses
{
	///{
	///  "total_count": -1,
	///  "count": 1,
	///  "results": [
	///    {
	///      "name": "life"
	///    }
	///  ]
	///}
	public class SearchResponse<T>
	{
		public int TotalCount { get; set; }
		public int Count { get; set; }
		public List<T> Results { get; set; }
	}

	public class SearchResult
	{
		public string Name { get; set; }
	}

	public class UserSearchResponse : SearchResponse<UserSearchResult>
	{
		
	}

	public class UserSearchResult
	{
		public string Name { get; set; }
		public string Username { get; set; }
		public string ProfileImage { get; set; }
	}

	public class GetVotersResponse : SearchResponse<VotersResult>
	{
		
	}

	public class VotersResult : UserSearchResult
	{
		public double Percent { get; set; }
	}
}