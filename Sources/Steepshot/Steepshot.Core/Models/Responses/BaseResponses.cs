namespace Steepshot.Core.Models.Responses
{
    public class MessageField
    {
        protected string Message { get; set; }
    }

    public class OffsetCountFields
    {
        public string Offset { get; set; }
        public int Count { get; set; }
    }
}
