namespace Steepshot.Core.Models.Responses
{
    public class ChangePasswordResponse : MessageField
    {
        private const string ServerPositiveResponceMsg = "PostingKey was changed";

        private readonly bool _isChanged;

        public bool IsChanged => _isChanged || Message.Equals(ServerPositiveResponceMsg);

        public ChangePasswordResponse(bool isChanged)
        {
            _isChanged = isChanged;
            Message = ServerPositiveResponceMsg;
        }
    }
}