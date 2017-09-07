using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public class FollowersPresenter : BasePresenter
    {
        public readonly List<UserFriend> Users = new List<UserFriend>();
        public bool _hasItems = true;
        private string _offsetUrl = string.Empty;
        private readonly int _itemsLimit = 60;

        public async Task<List<string>> GetItems(FriendsType followType, string username)
        {
            List<string> errors = null;
            try
            {
                if (!_hasItems)
                    return errors;
                var request = new UserFriendsRequest(username, followType)
                {
                    Login = User.Login,
                    Offset = _offsetUrl,
                    Limit = _itemsLimit
                };

                var response = await Api.GetUserFriends(request);
                errors = response.Errors;
                if (response.Success && response.Result?.Results != null && response.Result.Results.Count > 0)
                {
                    var lastItem = response.Result.Results.Last();
                    if (lastItem.Author != _offsetUrl)
                        response.Result.Results.Remove(lastItem);
                    else
                        _hasItems = false;

                    _offsetUrl = lastItem.Author;
                    Users.AddRange(response.Result.Results);
                }
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex);
            }
            return errors;
        }

        public async Task<OperationResult<FollowResponse>> Follow(UserFriend item)
        {
            var request = new FollowRequest(User.UserInfo, item.HasFollowed ? FollowType.UnFollow : FollowType.Follow, item.Author);
            return await Api.Follow(request);
        }
    }
}
