namespace Steemix.Library.Models.Requests
{
    public class TopPostRequest
    {
        public TopPostRequest(string offset, int limit)
        {
            Offset = offset;
            Limit = limit;
        }

        public string Offset { get; private set; }
        public int Limit { get; private set; }
    }
}