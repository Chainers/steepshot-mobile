using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Steepshot.Core.Presenters
{
    public sealed class SearchPresenter : ListPresenter<object>
    {
        public UserFriendPresenter UserFriendPresenter { get; }
        public TagsPresenter TagsPresenter { get; }

        public override int Count => throw new NotSupportedException();

        public SearchPresenter()
        {
            UserFriendPresenter = new UserFriendPresenter();
            TagsPresenter = new TagsPresenter();
        }

        public async Task<List<string>> TrySearchCategories(string query, SearchType searchType)
        {
            if (!string.IsNullOrEmpty(query) && (query.Length == 1 || (query.Length == 2 && searchType == SearchType.People)) || string.IsNullOrEmpty(query) && searchType == SearchType.People)
            {
                if (searchType == SearchType.Tags)
                    TagsPresenter.NotifySourceChanged();
                else
                    UserFriendPresenter.NotifySourceChanged();

                return new List<string>();
            }

            if (string.IsNullOrEmpty(query))
                return await TagsPresenter.TryGetTopTags();

            if (searchType == SearchType.Tags)
                return await TagsPresenter.TryLoadNext(query);

            return await UserFriendPresenter.TryLoadNextSearchUser(query);
        }
    }
}
