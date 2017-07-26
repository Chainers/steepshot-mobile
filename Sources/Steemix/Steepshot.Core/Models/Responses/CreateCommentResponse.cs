namespace Sweetshot.Library.Models.Responses
{
    ///{
    ///  "message": "Comment created"
    ///}
    public class CreateCommentResponse : MessageField
    {
        private bool _isCreated;

        public bool IsCreated
        {
            get => _isCreated || Message.Equals("Comment created");
            set => _isCreated = value;
        }

        public CreateCommentResponse(string msg)
        {
            Message = msg;
        }

        public CreateCommentResponse(bool isCreated)
        {
            _isCreated = isCreated;
        }
    }
}