namespace Steemix.Library.Models.Requests
{
    public class LoginRequest
    {
		public LoginRequest(string name, string pass)
		{
			username = name;
			password = pass;
		}

        public string username { get; set; }
        public string password { get; set; }
    }
}