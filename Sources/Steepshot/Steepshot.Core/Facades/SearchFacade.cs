using System;
using System.Threading.Tasks;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;

namespace Steepshot.Core.Facades
{
    public sealed class SearchFacade
    {
        public readonly UserFriendPresenter UserFriendPresenter;
        public readonly TagsPresenter TagsPresenter;

        public SearchFacade(UserFriendPresenter userFriendPresenter, TagsPresenter tagsPresenter)
        {
            UserFriendPresenter = userFriendPresenter;
            TagsPresenter = tagsPresenter;
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
                {
                    var result = await TagsPresenter
                        .TryGetTopTagsAsync()
                        .ConfigureAwait(false);

                    return result.Exception;
                }

                if (searchType == SearchType.Tags)
                {
                    var result = await TagsPresenter
                        .TryLoadNextAsync(query)
                        .ConfigureAwait(false);

                    return result.Exception;
                }

                {
                    var result = await UserFriendPresenter
                        .TryLoadNextSearchUserAsync(query)
                        .ConfigureAwait(false);

                    return result.Exception;
                }
            }
            catch (Exception)
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
