using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Errors;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Presenters
{
    public class TagsPresenter : ListPresenter<SearchResult>
    {
        private const int ItemsLimit = 40;

        public async Task<ErrorBase> TryLoadNext(string s, bool shouldClear = true)
        {
            return await RunAsSingleTask(LoadNext, new Tuple<string, bool>(s, shouldClear));
        }

        private async Task<ErrorBase> LoadNext(Tuple<string, bool> queryParams, CancellationToken ct)
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
                else if (Items.Count == 0 || Items.Count == 1)
                    lock (Items)
                        Items.Add(new SearchResult() { Name = queryParams.Item1 });

                if (tags.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
                NotifySourceChanged(nameof(TryLoadNext), true);
            }
            return response.Error;
        }

        public async Task<ErrorBase> TryGetTopTags()
        {
            return await RunAsSingleTask(GetTopTags);
        }

        private async Task<ErrorBase> GetTopTags(CancellationToken ct)
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
            return response.Error;
        }
    }
}
