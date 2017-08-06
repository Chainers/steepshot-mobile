namespace Steepshot.Core.Models.Responses
{
    ///{
    ///  "message": "PostingKey was changed"
    ///}
    public class ChangePasswordResponse : MessageField
    {
        public bool IsChanged => Message.Equals("PostingKey was changed");
    }
}