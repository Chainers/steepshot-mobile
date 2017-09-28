using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public class SearchPresenter : ListPresenter
    {
        private CancellationTokenSource _cts;
        public List<SearchResult> Tags = new List<SearchResult>();
        public List<UserFriend> Users = new List<UserFriend>();
        private const int ItemsLimit = 40;

        public async Task<List<string>> SearchCategories(string query, SearchType searchType, bool clear)
        {
            List<string> errors = null;

            if (!string.IsNullOrEmpty(query) && (query.Length == 1 || (query.Length == 2 && searchType == SearchType.People))
                || string.IsNullOrEmpty(query) && searchType == SearchType.People)
                return errors;

            try
            {
                _cts?.Cancel();
            }
            catch (ObjectDisposedException) { }

            if (clear)
            {
                OffsetUrl = string.Empty;
                IsLastReaded = false;
            }

            try
            {
                OperationResult response;
                using (_cts = new CancellationTokenSource())
                {
                    if (string.IsNullOrEmpty(query))
                    {
                        var request = new OffsetLimitFields();
                        response = await Api.GetCategories(request, _cts);
                    }
                    else
                    {
                        var request = new SearchWithQueryRequest(query)
                        {
                            Limit = ItemsLimit
                        };
                        if (searchType == SearchType.Tags)
                            response = await Api.SearchCategories(request, _cts);
                        else
                        {
                            request.Offset = OffsetUrl;
                            request.Login = User.Login;
                            response = await Api.SearchUser(request, _cts);
                        }
                    }
                    if (response.Success)
                    {
                        if (searchType == SearchType.Tags)
                        {
                            Tags.AddRange(((OperationResult<SearchResponse<SearchResult>>)response).Result?.Results);
                        }
                        else
                        {
                            var users = ((OperationResult<SearchResponse<UserFriend>>)response).Result?.Results;
                            if (users.Count > 0)
                            {
                                Users.AddRange(string.IsNullOrEmpty(OffsetUrl) ? users : users.Skip(1));
                                OffsetUrl = users.Last().Author;
                            }

                            if (users.Count < Math.Min(ServerMaxCount, ItemsLimit))
                                IsLastReaded = true;
                        }
                    }
                    else
                        errors = response.Errors;
                }
            }
            catch (TaskCanceledException e)
            {
                throw e;
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
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
