namespace Steepshot.Core.Models.Responses
{
    public class FollowResponse : MessageField
    {
        private const string ServerPositiveResponceMsg = "User is followed";
        private readonly bool _isFollowed;

        public bool IsFollowed => _isFollowed || Message.Equals(ServerPositiveResponceMsg);

        public FollowResponse(bool isFollowed)
        {
            _isFollowed = isFollowed;
            Message = ServerPositiveResponceMsg;
        }
    }
}