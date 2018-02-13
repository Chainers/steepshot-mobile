using System;
using System.Threading.Tasks;
using Steepshot.Core.Errors;
using Steepshot.Core.Models.Enums;

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

        public async Task<ErrorBase> TrySearchCategories(string query, SearchType searchType)
        {
            if (!string.IsNullOrEmpty(query) && (query.Length == 1 || (query.Length == 2 && searchType == SearchType.People)) || string.IsNullOrEmpty(query) && searchType == SearchType.People)
            {
                if (searchType == SearchType.Tags)
                    TagsPresenter.NotifySourceChanged(nameof(TrySearchCategories), true);
                else
                    UserFriendPresenter.NotifySourceChanged(nameof(TrySearchCategories), true);

                return null;
            }

            if (string.IsNullOrEmpty(query))
                return await TagsPresenter.TryGetTopTags();

            if (searchType == SearchType.Tags)
                return await TagsPresenter.TryLoadNext(query);

            return await UserFriendPresenter.TryLoadNextSearchUser(query);
        }
    }
}
