using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Steepshot.Core.Presenters
{
    public sealed class SearchPresenter : ListPresenter<object>
    {
        public FollowersPresenter FollowersPresenter { get; }
        public TagsPresenter TagsPresenter { get; }

        public override int Count => throw new NotSupportedException();

        public SearchPresenter()
        {
            FollowersPresenter = new FollowersPresenter();
            TagsPresenter = new TagsPresenter();
        }

        public async Task<List<string>> TrySearchCategories(string query, SearchType searchType, bool clear)
        {
            if (!string.IsNullOrEmpty(query) && (query.Length == 1 || (query.Length == 2 && searchType == SearchType.People)) || string.IsNullOrEmpty(query) && searchType == SearchType.People)
                return null;

            if (clear)
            {
                FollowersPresenter.Clear();
                TagsPresenter.Clear();
            }
            return await RunAsSingleTask(SearchCategories, query, searchType);
        }

        private async Task<List<string>> SearchCategories(CancellationTokenSource cts, string query, SearchType searchType)
        {
            if (string.IsNullOrEmpty(query))
                return await TagsPresenter.TryGetTopTags();

            if (searchType == SearchType.Tags)
                return await TagsPresenter.TryLoadNext(query);

            return await FollowersPresenter.TryLoadNextSearchUser(cts, query);
        }
    }
}
