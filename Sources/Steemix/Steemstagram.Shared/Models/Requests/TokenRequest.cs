namespace Steemix.Library.Models.Requests
{
    public class TokenRequest
    {
        public TokenRequest(string token)
        {
            Token = token;
        }

        public string Token { get; private set; }
    }
}