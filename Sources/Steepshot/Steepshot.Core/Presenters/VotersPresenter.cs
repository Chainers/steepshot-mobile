using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Presenters
{
    public sealed class VotersPresenter : ListPresenter<VotersResult>
    {
        private const int ItemsLimit = 40;

        public async Task<List<string>> TryLoadNext(string url)
        {
            if (IsLastReaded)
                return null;
            return await RunAsSingleTask(LoadNext, url);
        }

        private async Task<List<string>> LoadNext(CancellationToken ct, string url)
        {
            var request = new InfoRequest(url)
            {
                Offset = OffsetUrl,
                Limit = ItemsLimit
            };

            var response = await Api.GetPostVoters(request, ct);

            if (response.Success)
            {
                var voters = response.Result.Results;
                if (voters.Count > 0)
                {
                    lock (Items)
                        Items.AddRange(string.IsNullOrEmpty(OffsetUrl) ? voters : voters.Skip(1));

                    OffsetUrl = voters.Last().Username;
                }

                if (voters.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
            }
            return response.Errors;
        }
    }
}
