using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Presenters
{
    public class VotersPresenter : BasePresenter
    {
        private const int ItemsLimit = 40;
        private string _offsetUrl = string.Empty;

        public readonly List<VotersResult> Voters;
        private bool IsLastReaded { get; set; }

        public VotersPresenter()
        {
            Voters = new List<VotersResult>();
        }

        public async Task<List<string>> LoadNext(string url, CancellationTokenSource cts)
        {
            if (IsLastReaded)
                return null;

            var request = new InfoRequest(url)
            {
                Offset = _offsetUrl,
                Limit = ItemsLimit
            };

            var response = await Api.GetPostVoters(request, cts);

            if (response.Success)
            {
                var voters = response.Result.Results;
                if (voters.Count > 0)
                {
                    Voters.AddRange(string.IsNullOrEmpty(_offsetUrl) ? voters : voters.Skip(1));
                    _offsetUrl = voters.Last().Username;
                }

                if (voters.Count < Math.Min(OffsetLimitFields.ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
            }
            return response.Errors;
        }
    }
}
