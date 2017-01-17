namespace Sweetshot.Library.Models.Requests
{
    public class UserProfileRequest : UserPostsRequest
    {
        public UserProfileRequest(string username) : base(username)
        {
        }
    }
}