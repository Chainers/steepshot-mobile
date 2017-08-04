namespace Steepshot.Core.Models.Responses
{
    ///{
    ///  "message": "Comment created"
    ///}
    public class CreateCommentResponse : MessageField
    {
        public bool IsCreated => Message.Equals("Comment created");
    }
}