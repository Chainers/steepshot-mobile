using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;

namespace Steepshot
{
	public class FollowersPresenter : BasePresenter
	{
		public FollowersPresenter(FollowersView view):base(view)
		{
		}

		public readonly ObservableCollection<UserFriendViewMode> Collection = new ObservableCollection<UserFriendViewMode>();

		public void ViewLoad(FollowType friendsType)
		{
			if (Collection.Count == 0)
				Task.Run(() => GetItems(string.Empty, 10, friendsType));
		}

		public async Task GetItems(string offset, int limit, FollowType followType)
		{
			var request = new UserFriendsRequest(UserPrincipal.Instance.CurrentUser.Login,
                followType == FollowType.Follow ? FriendsType.Followers : FriendsType.Following)
			{
                SessionId = UserPrincipal.Instance.Cookie,
                Offset = offset,
				Limit = limit
			};

			var responce = await Api.GetUserFriends(request);
			//TODO:KOA -- Errors not processed
			if (responce.Success)
			{
				foreach (var item in responce.Result.Results)
				{
					Collection.Add(new UserFriendViewMode(item, followType == FollowType.Follow));
				}
			}
		}

		public async Task<OperationResult<FollowResponse>> Follow(UserFriendViewMode item)
		{
			var request = new FollowRequest(UserPrincipal.Instance.CurrentUser.SessionId, item.IsFollow ? FollowType.Follow : FollowType.UnFollow, item.Author);
			return await Api.Follow(request);
		}
	}
}
