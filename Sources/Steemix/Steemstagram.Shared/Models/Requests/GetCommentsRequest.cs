namespace Steemix.Library.Models.Requests
{
	public class GetCommentsRequest : TokenRequest
	{
		public GetCommentsRequest(string token, string _url) : base(token)
		{
			url = _url;
		}

		public string url { get; private set; }
	}
}