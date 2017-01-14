namespace Sweetshot.Library.Models.Requests
{
    public enum FriendsType
    {
        Followers,
        Following
    }

    public class UserFriendsRequest : UserRequest
    {
        public UserFriendsRequest(string sessionId, string username, FriendsType type, string offset = "") : base(sessionId, username)
        {
            Type = type;
            Offset = offset;
        }

        public FriendsType Type { get; private set; }

        public string Offset { get; private set; }
    }
}