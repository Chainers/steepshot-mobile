using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Steepshot.Base;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;
using Steepshot.Data;

namespace Steepshot.Presenter
{
    public class FollowersPresenter : BasePresenter
    {
        public FollowersPresenter(IBaseView view) : base(view)
        {
        }

        public readonly ObservableCollection<UserFriendViewMode> Collection = new ObservableCollection<UserFriendViewMode>();
        private bool _hasItems = true;
        private string _offsetUrl = string.Empty;
        private int _itemsLimit = 60;

        public void ViewLoad(FollowType friendsType, string username)
        {
            if (Collection.Count == 0)
                Task.Run(() => GetItems(friendsType, username));
        }

        public async Task GetItems(FollowType followType, string username)
        {
            try
            {
                if (!_hasItems)
                    return;
                var request = new UserFriendsRequest(username, followType == FollowType.Follow ? FriendsType.Followers : FriendsType.Following)
                {
                    SessionId = User.CurrentUser.SessionId,
                    Login = User.CurrentUser.Login,
                    Offset = _offsetUrl,
                    Limit = _itemsLimit
                };

                var responce = await Api.GetUserFriends(request);
                //TODO:KOA -- Errors not processed
                if (responce.Success && responce?.Result?.Results != null && responce.Result.Results.Count > 0)
                {
                    var lastItem = responce.Result.Results.Last();
                    if (lastItem.Author != _offsetUrl)
                        responce.Result.Results.Remove(lastItem);
                    else
                        _hasItems = false;

                    _offsetUrl = lastItem.Author;
                    foreach (var item in responce.Result.Results)
                        Collection.Add(new UserFriendViewMode(item, item.HasFollowed));
                }
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex, User.Login, AppVersion);
            }
        }

        public async Task<OperationResult<FollowResponse>> Follow(UserFriendViewMode item)
        {
            var request = new FollowRequest(User.CurrentUser.SessionId,
                item.IsFollow ? FollowType.UnFollow : FollowType.Follow, item.Author)
            {
                SessionId = User.CurrentUser.SessionId,
                Login = User.CurrentUser.Login
            };
            return await Api.Follow(request);
        }
    }
}
