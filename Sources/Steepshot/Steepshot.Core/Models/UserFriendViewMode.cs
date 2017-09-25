using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Models
{
	public class UserFriendViewMode : UserFriend
	{
		public UserFriendViewMode(UserFriend item, bool isFollow)
		{
			Author = item.Author;
			Avatar = item.Avatar;
			Reputation = item.Reputation;
			IsFollow = isFollow;
		}

		public bool IsFollow { get; set; }
	}
}