using System;
using System.Collections.Generic;
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

        public async Task<ErrorBase> TryLoadNext(string s)
        {
            return await RunAsSingleTask(LoadNext, s);
        }

        private async Task<ErrorBase> LoadNext(string s, CancellationToken ct)
        {
            var request = new SearchWithQueryModel(s.TagToEn())
            {
                Offset = OffsetUrl,
                Limit = ItemsLimit
            };

            var response = await Api.SearchCategories(request, ct);

            if (response.IsSuccess)
            {
                var tags = response.Result.Results.Distinct(new SearchResultComparer()).ToList();
                if (tags.Count > 0)
                {
                    lock (Items)
                        Items.AddRange(Items.Count == 0 ? tags : tags.Skip(1));

                    OffsetUrl = tags.Last().Name;
                }

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
            var request = new OffsetLimitModel()
            {
                Offset = OffsetUrl,
                Limit = ItemsLimit
            };

            var response = await Api.GetCategories(request, ct);

            if (response.IsSuccess)
            {
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

    public class SearchResultComparer : IEqualityComparer<SearchResult>
    {
        public bool Equals(SearchResult x, SearchResult y)
        {
            return x.Name == y.Name;
        }

        public int GetHashCode(SearchResult obj)
        {
            return 0;
        }
    }
}
