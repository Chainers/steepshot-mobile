namespace Steemix.Library.Models.Requests
{

	public class RegisterRequest : LoginRequest
	{
		public RegisterRequest(string key, string username, string password) : base(username, password)
		{
			posting_key = key;
		}
		
		public string posting_key { get; set; }
	}
}