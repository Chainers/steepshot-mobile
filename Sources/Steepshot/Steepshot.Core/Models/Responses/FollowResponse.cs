namespace Steepshot.Core.Models.Responses
{
    public class FollowResponse : MessageField
    {
        private const string ServerPositiveResponceMsg = "User is followed";
        private const string ServerPositiveResponceMsg2 = "User is unfollowed";
        private readonly bool _isFollowed;

        public bool IsSuccess => _isFollowed || Message.Equals(ServerPositiveResponceMsg) || Message.Equals(ServerPositiveResponceMsg2);

        public FollowResponse(bool isFollowed)
        {
            _isFollowed = isFollowed;
        }
    }
}