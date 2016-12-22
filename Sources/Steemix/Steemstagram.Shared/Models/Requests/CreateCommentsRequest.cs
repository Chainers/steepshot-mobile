namespace Steemix.Library.Models.Requests
{

	public class CreateCommentsRequest : GetCommentsRequest
	{
		public CreateCommentsRequest(string token, string _url, string _body, string _title) : base(token, _url)
		{
			body = _body;
			title = _title;
		}

		public string body { get; private set; }
		public string title { get; private set; }
	}
}