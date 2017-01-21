using Sweetshot.Library.Models.Requests.Common;

namespace Sweetshot.Library.Models.Requests
{
    public enum FriendsType
    {
        Followers,
        Following
    }

    public class UserFriendsRequest : OffsetLimitFields
    {
        public UserFriendsRequest(string username, FriendsType type)
        {
            Username = username;
            Type = type;
        }

        public string Username { get; private set; }
        public FriendsType Type { get; private set; }
    }
}