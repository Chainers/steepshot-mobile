using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Presenters
{
    public sealed class TagsPresenter : ListPresenter
    {
        private const int ItemsLimit = 40;
        private readonly List<SearchResult> _tags;

        public override int Count => _tags.Count;

        public SearchResult this[int position]
        {
            get
            {
                lock (_tags)
                {
                    if (position > -1 && position < _tags.Count)
                        return _tags[position];
                }
                return null;
            }
        }

        public TagsPresenter()
        {
            _tags = new List<SearchResult>();
        }



        public async Task<List<string>> TryLoadNext(string s)
        {
            return await RunAsSingleTask(LoadNext, s);
        }

        private async Task<List<string>> LoadNext(CancellationTokenSource cts, string s)
        {
            var request = new SearchWithQueryRequest(s)
            {
                Offset = OffsetUrl,
                Limit = ItemsLimit
            };

            var response = await Api.SearchCategories(request, cts);

            if (response.Success)
            {
                var tags = response.Result.Results;
                if (tags.Count > 0)
                {
                    lock (_tags)
                        _tags.AddRange(string.IsNullOrEmpty(OffsetUrl) ? tags : tags.Skip(1));

                    OffsetUrl = tags.Last().Name;
                }

                if (tags.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
            }
            return response.Errors;
        }


        public async Task<List<string>> TryGetTopTags()
        {
            return await RunAsSingleTask(GetTopTags);
        }

        private async Task<List<string>> GetTopTags(CancellationTokenSource cts)
        {
            var request = new OffsetLimitFields()
            {
                Offset = OffsetUrl,
                Limit = ItemsLimit
            };

            var response = await Api.GetCategories(request, cts);

            if (response.Success)
            {
                var tags = response.Result.Results;
                if (tags.Count > 0)
                {
                    lock (_tags)
                        _tags.AddRange(string.IsNullOrEmpty(OffsetUrl) ? tags : tags.Skip(1));

                    OffsetUrl = tags.Last().Name;
                }

                if (tags.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
            }
            return response.Errors;
        }

        public void Clear()
        {
            lock (_tags)
                _tags.Clear();
            OffsetUrl = string.Empty;
            IsLastReaded = false;
        }
    }
}
