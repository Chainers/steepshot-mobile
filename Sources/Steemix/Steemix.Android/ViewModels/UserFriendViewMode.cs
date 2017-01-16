using Sweetshot.Library.Models.Responses;

namespace Steemix.Droid.ViewModels
{
    public class UserFriendViewMode : UserFriend
    {
        public UserFriendViewMode(UserFriend item, bool followUnfollow)
        {
            Author = item.Author;
            Avatar = item.Avatar;
            Reputation = item.Reputation;
            FollowUnfollow = followUnfollow;
        }

        public bool FollowUnfollow { get; set; }
    }
}