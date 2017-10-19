using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Presenters
{
    public sealed class VotersPresenter : ListPresenter<UserFriend>
    {
        private const int ItemsLimit = 40;

        public async Task<List<string>> TryLoadNext(string url)
        {
            if (IsLastReaded)
                return null;
            return await RunAsSingleTask(LoadNext, url);
        }

        private async Task<List<string>> LoadNext(CancellationToken ct, string url)
        {
            var request = new InfoRequest(url)
            {
                Offset = OffsetUrl,
                Limit = ItemsLimit,
                Login = User.Login
            };

            var response = await Api.GetPostVoters(request, ct);

            if (response.Success)
            {
                var voters = response.Result.Results;
                if (voters.Count > 0)
                {
                    lock (Items)
                        Items.AddRange(string.IsNullOrEmpty(OffsetUrl) ? voters : voters.Skip(1));

                    OffsetUrl = voters.Last().Author;
                }

                if (voters.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
            }
            return response.Errors;
        }

        public async Task<List<string>> TryFollow(UserFriend item)
        {
            return await TryRunTask(Follow, CancellationToken.None, item);
        }

        private async Task<List<string>> Follow(CancellationToken ct, UserFriend item)
        {
            var request = new FollowRequest(User.UserInfo, (bool)item.HasFollowed ? Models.Requests.FollowType.UnFollow : Models.Requests.FollowType.Follow, item.Author);
            item.HasFollowed = null;
            var response = await Api.Follow(request, ct);
            if (response.Success)
                item.HasFollowed = request.Type == FollowType.Follow;
            else
                item.HasFollowed = request.Type != FollowType.Follow;
            return response.Errors;
        }
    }
}
