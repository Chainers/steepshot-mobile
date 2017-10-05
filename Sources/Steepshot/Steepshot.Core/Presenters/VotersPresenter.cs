using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Presenters
{
    public class VotersPresenter : ListPresenter
    {
        private const int ItemsLimit = 40;
        private readonly List<VotersResult> _voters;

        public override int Count => _voters.Count;

        public VotersResult this[int position]
        {
            get
            {
                lock (_voters)
                {
                    if (position > -1 && position < _voters.Count)
                        return _voters[position];
                }
                return null;
            }
        }

        public VotersPresenter()
        {
            _voters = new List<VotersResult>();
        }


        public async Task<List<string>> TryLoadNext(string url)
        {
            if (IsLastReaded)
                return null;
            return await RunAsSingleTask(LoadNext, url);
        }

        private async Task<List<string>> LoadNext(string url, CancellationTokenSource cts)
        {
            var request = new InfoRequest(url)
            {
                Offset = OffsetUrl,
                Limit = ItemsLimit
            };

            var response = await Api.GetPostVoters(request, cts);

            if (response.Success)
            {
                var voters = response.Result.Results;
                if (voters.Count > 0)
                {
                    lock (_voters)
                        _voters.AddRange(string.IsNullOrEmpty(OffsetUrl) ? voters : voters.Skip(1));

                    OffsetUrl = voters.Last().Username;
                }

                if (voters.Count < Math.Min(ServerMaxCount, ItemsLimit))
                    IsLastReaded = true;
            }
            return response.Errors;
        }
    }
}