using System;
using System.Threading.Tasks;
using Steepshot.Core.Clients;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Localization;
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

        public void SetClient(SteepshotApiClient client)
        {
            UserFriendPresenter.SetClient(client);
            TagsPresenter.SetClient(client);
        }

        public async Task<Exception> TrySearchCategoriesAsync(string query, SearchType searchType)
        {
            try
            {
                if (!string.IsNullOrEmpty(query) && (query.Length == 1 || (query.Length == 2 && searchType == SearchType.People)) || string.IsNullOrEmpty(query) && searchType == SearchType.People)
                {
                    if (searchType == SearchType.Tags)
                    {
                        TagsPresenter.NotifySourceChanged(nameof(TrySearchCategoriesAsync), true);
                        TagsPresenter.TasksCancel();
                    }
                    else
                    {
                        UserFriendPresenter.NotifySourceChanged(nameof(TrySearchCategoriesAsync), true);
                        UserFriendPresenter.TasksCancel();
                    }

                    return new ValidationException(LocalizationKeys.TagSearchWarning);
                }

                if (string.IsNullOrEmpty(query))
                    return await TagsPresenter.TryGetTopTagsAsync().ConfigureAwait(false);

                if (searchType == SearchType.Tags)
                    return await TagsPresenter.TryLoadNextAsync(query).ConfigureAwait(false);

                return await UserFriendPresenter.TryLoadNextSearchUserAsync(query).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void TasksCancel()
        {
            UserFriendPresenter.TasksCancel();
            TagsPresenter.TasksCancel();
        }
    }
}
