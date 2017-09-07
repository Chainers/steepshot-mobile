using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public class SearchPresenter : BasePresenter
    {
        private CancellationTokenSource _cts;
        public List<SearchResult> Tags = new List<SearchResult>();
        public List<UserSearchResult> Users = new List<UserSearchResult>();
        private readonly Dictionary<SearchType, string> _prevQuery = new Dictionary<SearchType, string> { { SearchType.People, null }, { SearchType.Tags, null } };

        public async Task<List<string>> SearchCategories(string query, SearchType searchType)
        {
            List<string> errors = null;

            if (_prevQuery[searchType] == query)
                return errors;

            if (!string.IsNullOrEmpty(query) && (query.Length == 1 || (query.Length == 2 && searchType == SearchType.People))
                || string.IsNullOrEmpty(query) && searchType == SearchType.People)
                return errors;

            _prevQuery[searchType] = query;

            try
            {
                _cts?.Cancel();
            }
            catch (ObjectDisposedException) { }

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
                        var request = new SearchWithQueryRequest(query);
                        if (searchType == SearchType.Tags)
                            response = await Api.SearchCategories(request, _cts);
                        else
                            response = await Api.SearchUser(request, _cts);
                    }
                    if (response.Success)
                    {
                        if (searchType == SearchType.Tags)
                        {
                            Tags.Clear();
                            Tags.AddRange(((OperationResult<SearchResponse<SearchResult>>)response).Result?.Results);
                        }
                        else
                        {
                            Users.Clear();
                            Users.AddRange(((OperationResult<SearchResponse<UserSearchResult>>)response).Result?.Results);
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
                Reporter.SendCrash(ex);
            }
            return errors;
        }
    }
}
