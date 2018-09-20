using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Presenters
{
    public class TagsPresenter : ListPresenter<SearchResult>
    {
        private const int ItemsLimit = 40;

        public async Task<Exception> TryLoadNextAsync(string s, bool shouldClear = true, bool showUnknownTag = false)
        {
            return await RunAsSingleTaskAsync(LoadNextAsync, new Tuple<string, bool, bool>(s, shouldClear, showUnknownTag)).ConfigureAwait(false);
        }

        private async Task<Exception> LoadNextAsync(Tuple<string, bool, bool> queryParams, CancellationToken ct)
        {
            if (queryParams.Item2)
                Clear();

            var request = new SearchWithQueryModel(queryParams.Item1.TagToEn())
            {
                Offset = OffsetUrl,
                Limit = ItemsLimit
            };

            var response = await Api.SearchCategoriesAsync(request, ct).ConfigureAwait(false);

            if (response.IsSuccess)
            {
                if (queryParams.Item2)
                    Clear();
                var tags = response.Result.Results;
                if (tags.Count > 0)
                {
                    lock (Items)
                    {
                        Items.AddRange(Items.Count == 0 ? tags : tags.Skip(1));
                    }
                    OffsetUrl = tags.Last().Name;
                }
                else if ((Items.Count == 0 || Items.Count == 1) && queryParams.Item3)
                    lock (Items)
                        Items.Add(new SearchResult() { Name = queryParams.Item1 });

                if (tags.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
                NotifySourceChanged(nameof(TryLoadNextAsync), true);
            }
            return response.Exception;
        }

        public async Task<Exception> TryGetTopTagsAsync()
        {
            return await RunAsSingleTaskAsync(GetTopTagsAsync).ConfigureAwait(false);
        }

        private async Task<Exception> GetTopTagsAsync(CancellationToken ct)
        {
            Clear();
            var request = new OffsetLimitModel()
            {
                Offset = OffsetUrl,
                Limit = ItemsLimit
            };

            var response = await Api.GetCategoriesAsync(request, ct).ConfigureAwait(false);

            if (response.IsSuccess)
            {
                Clear();
                var tags = response.Result.Results;
                if (tags.Count > 0)
                {
                    lock (Items)
                        Items.AddRange(Items.Count == 0 ? tags : tags.Skip(1));

                    OffsetUrl = tags.Last().Name;
                }

                if (tags.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
                NotifySourceChanged(nameof(TryGetTopTagsAsync), true);
            }
            return response.Exception;
        }
    }
}
