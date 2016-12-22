namespace Steemix.Library.Models.Requests
{
    public class TopPostRequest : TokenRequest
    {
        public TopPostRequest(string token, int offset, int limit) : base(token)
        {
            Offset = offset;
            Limit = limit;
        }

        public int Offset { get; private set; }
        public int Limit { get; private set; }
    }
}