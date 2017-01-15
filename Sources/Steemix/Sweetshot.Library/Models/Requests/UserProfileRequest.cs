namespace Sweetshot.Library.Models.Requests
{
    public class UserProfileRequest : UserPostsRequest
    {
        public UserProfileRequest(string sessionId, string username) : base(sessionId, username)
        {
        }
    }
}