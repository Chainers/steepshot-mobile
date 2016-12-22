namespace Steemix.Library.Models.Requests
{
    public class VoteRequest : TokenRequest
    {
        public VoteRequest(string token, string _identifier) : base(token)
        {
            identifier = _identifier;
        }

        public string identifier { get; private set; }
    }
}