namespace Steepshot.Core.Models.Responses
{
    public class CreateCommentResponse : MessageField
    {
        private const string ServerPositiveResponceMsg = "Comment created";
        private readonly bool _isCreated;

        public bool IsCreated => _isCreated || Message.Equals(ServerPositiveResponceMsg);

        public CreateCommentResponse(bool isCreated)
        {
            _isCreated = isCreated;
        }

        public string Permlink { get; set; }
    }
}