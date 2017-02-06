namespace Sweetshot.Library.Models.Requests
{
    public class SessionIdField
    {
        public string SessionId { get; set; }
    }

    public class OffsetLimitFields
    {
        public string Offset { get; set; }
        public int Limit { get; set; }
    }

    public class SessionIdOffsetLimitFields : OffsetLimitFields
    {
        public string SessionId { get; set; }
    }
}