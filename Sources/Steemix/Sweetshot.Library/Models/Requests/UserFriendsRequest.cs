using Sweetshot.Library.Models.Requests.Common;

namespace Sweetshot.Library.Models.Requests
{
    public enum FriendsType
    {
        Followers,
        Following
    }

    public class UserFriendsRequest : OffsetLimitSessionRequest
    {
        public UserFriendsRequest(string sessionId, string username, FriendsType type, string offset = "", int limit = 0)
            : base(sessionId, offset, limit)
        {
            Username = username;
            Type = type;
        }

        public string Username { get; private set; }
        public FriendsType Type { get; private set; }
    }
}