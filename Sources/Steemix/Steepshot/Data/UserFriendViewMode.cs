using System;
using Sweetshot.Library.Models.Responses;

namespace Steepshot
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
