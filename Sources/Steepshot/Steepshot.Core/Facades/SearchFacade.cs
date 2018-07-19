using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Steepshot.Core.Errors;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;

namespace Steepshot.Core.Facades
{
    public sealed class SearchFacade
    {
        public UserFriendPresenter UserFriendPresenter { get; }
        public TagsPresenter TagsPresenter { get; }

        public SearchFacade()
        {
            UserFriendPresenter = new UserFriendPresenter();
            TagsPresenter = new TagsPresenter();
        }

        public async Task<ErrorBase> TrySearchCategories(string query, SearchType searchType)
        {
            if (!string.IsNullOrEmpty(query) && (query.Length == 1 || (query.Length == 2 && searchType == SearchType.People)) || string.IsNullOrEmpty(query) && searchType == SearchType.People)
            {
                if (searchType == SearchType.Tags)
                {
                    TagsPresenter.NotifySourceChanged(nameof(TrySearchCategories), true);
                    TagsPresenter.TasksCancel(false);
                }
                else
                {
                    UserFriendPresenter.NotifySourceChanged(nameof(TrySearchCategories), true);
                    UserFriendPresenter.TasksCancel(false);
                }
                
                return new ValidationError();
            }

            if (string.IsNullOrEmpty(query))
                return await TagsPresenter.TryGetTopTags();

            if (searchType == SearchType.Tags)
                return await TagsPresenter.TryLoadNext(query);

            return await UserFriendPresenter.TryLoadNextSearchUser(query);
        }

        public void TasksCancel(bool andDispose = false)
        {
            UserFriendPresenter.TasksCancel(andDispose);
            TagsPresenter.TasksCancel(andDispose);
        }
    }
}
