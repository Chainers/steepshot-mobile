using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;

namespace Steemix.Droid.ViewModels
{
    public class FollowersViewModel : MvvmViewModelBase
    {
        public readonly ObservableCollection<UserFriendViewMode> Collection = new ObservableCollection<UserFriendViewMode>();

        public void ViewLoad(FollowType friendsType)
        {
            base.ViewLoad();
            if (Collection.Count == 0)
                Task.Run(() => GetItems(string.Empty, 10, friendsType));
        }

        public async Task GetItems(string offset, int limit, FollowType followType)
        {
            var request = new UserFriendsRequest(UserPrincipal.CurrentUser.SessionId, UserPrincipal.CurrentUser.Login, followType == FollowType.Follow ? FriendsType.Followers : FriendsType.Following, offset, limit);
            var responce = await ViewModelLocator.Api.GetUserFriends(request);
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
            var request = new FollowRequest(UserPrincipal.CurrentUser.SessionId, item.FollowUnfollow ? FollowType.Follow : FollowType.UnFollow, item.Author);
            return await ViewModelLocator.Api.Follow(request);
        }
    }
}