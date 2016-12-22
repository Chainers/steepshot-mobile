namespace Steemix.Library.Models.Requests
{
    public class UserPostRequest : TokenRequest
    {
        public UserPostRequest(string token, string username) : base(token)
        {
            Username = username;
        }

        public string Username { get; private set; }
    }
}