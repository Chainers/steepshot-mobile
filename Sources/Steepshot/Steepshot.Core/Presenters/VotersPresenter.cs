using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public class VotersPresenter : BasePresenter
    {
        private CancellationTokenSource _cancellationTokenSource;
        private const int ItemsLimit = 40;
        private string _offsetUrl = string.Empty;

        public readonly List<VotersResult> Voters;
        private bool IsLastReaded { get; set; }

        public VotersPresenter()
        {
            Voters = new List<VotersResult>();
        }

        public async Task<List<string>> TryLoadNext(string url)
        {
            if (IsLastReaded)
                return null;

            try
            {
                var request = new InfoRequest(url)
                {
                    Offset = _offsetUrl,
                    Limit = ItemsLimit
                };

                if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
                    _cancellationTokenSource = new CancellationTokenSource();

                var response = await Api.GetPostVoters(request, _cancellationTokenSource);

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
            catch (OperationCanceledException)
            {
                // to do nothing
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex);
            }
            return null;
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}