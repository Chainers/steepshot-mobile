using System;
using System.Linq;
using System.Threading.Tasks;
using Steepshot.Core.Clients;
using Steepshot.Core.Extensions;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public class TagsPresenter : ListPresenter<SearchResult>
    {
        private const int ItemsLimit = 40;

        private readonly SteepshotApiClient _steepshotApiClient;

        public TagsPresenter(IConnectionService connectionService, ILogService logService, SteepshotApiClient steepshotApiClient)
            : base(connectionService, logService)
        {
            _steepshotApiClient = steepshotApiClient;
        }

        public async Task<OperationResult<ListResponse<SearchResult>>> TryLoadNextAsync(string s, bool shouldClear = true, bool showUnknownTag = false)
        {
            if (shouldClear)
                Clear();

            var request = new SearchWithQueryModel(s.TagToEn())
            {
                Offset = OffsetUrl,
                Limit = ItemsLimit
            };

            var response = await RunAsSingleTaskAsync(_steepshotApiClient.SearchCategoriesAsync, request)
                .ConfigureAwait(false);

            if (response.IsSuccess)
            {
                if (shouldClear)
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
                else
                {
                    lock (Items)
                    {
                        if ((Items.Count == 0 || Items.Count == 1) && showUnknownTag)
                            Items.Add(new SearchResult { Name = s });
                    }
                }

                if (tags.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;

                NotifySourceChanged(nameof(TryLoadNextAsync), true);
            }
            return response;
        }

        public async Task<OperationResult<ListResponse<SearchResult>>> TryGetTopTagsAsync()
        {
            Clear();
            var request = new OffsetLimitModel()
            {
                Offset = OffsetUrl,
                Limit = ItemsLimit
            };


            var response = await RunAsSingleTaskAsync(_steepshotApiClient.GetCategoriesAsync, request)
                .ConfigureAwait(false);

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
            return response;
        }
    }
}
