namespace Steepshot.Core.Models.Responses
{
    public class CommentResponse : VoidResponse
    {
        public string Permlink { get; set; }

        public CommentResponse(bool isSuccess) : base(isSuccess)
        {
        }
    }
}
