namespace Steemix.Library.Models.Requests
{
    public class FollowRequest : TokenRequest
    {
        public FollowRequest(string token, string _username) : base(token)
        {
            username = _username;
        }

        public string username { get; private set; }
    }
}