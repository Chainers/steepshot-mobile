namespace Steemix.Library.Models.Requests
{
    public class UserInfoRequest : TokenRequest
    {
        public UserInfoRequest(string token, string login)
            : base(token)
        {
            Login = login;
        }

        public string Login { get; private set; }
    }
}