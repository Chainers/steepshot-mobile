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

        public async Task<Exception> TryLoadNext(string s, bool shouldClear = true, bool showUnknownTag = false)
        {
            return await RunAsSingleTask(LoadNext, new Tuple<string, bool, bool>(s, shouldClear, showUnknownTag));
        }

        private async Task<Exception> LoadNext(Tuple<string, bool, bool> queryParams, CancellationToken ct)
        {
            if (queryParams.Item2)
                Clear();

            var request = new SearchWithQueryModel(queryParams.Item1.TagToEn())
            {
                Offset = OffsetUrl,
                Limit = ItemsLimit
            };

            var response = await Api.SearchCategories(request, ct);

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
                NotifySourceChanged(nameof(TryLoadNext), true);
            }
            return response.Exception;
        }

        public async Task<Exception> TryGetTopTags()
        {
            return await RunAsSingleTask(GetTopTags);
        }

        private async Task<Exception> GetTopTags(CancellationToken ct)
        {
            Clear();
            var request = new OffsetLimitModel()
            {
                Offset = OffsetUrl,
                Limit = ItemsLimit
            };

            var response = await Api.GetCategories(request, ct);

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
                NotifySourceChanged(nameof(TryGetTopTags), true);
            }
            return response.Exception;
        }
    }
}
