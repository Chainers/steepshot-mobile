using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public class VotersPresenter : BasePresenter
    {
        public event Action VotersLoaded;
        public List<VotersResult> Users = new List<VotersResult>();
        public bool _hasItems = true;
        private string _offsetUrl = string.Empty;
        private readonly int _itemsLimit = 60;

        public async Task<List<string>> GetItems(string url)
        {
            List<string> errors = null;
            try
            {
                if (!CheckInternetConnection())
                    return errors;
                if (!_hasItems)
                    return errors;

                var request = new InfoRequest(url)
                {
                    Offset = _offsetUrl,
                    Limit = _itemsLimit
                };

                var response = await Api.GetPostVoters(request);
                errors = response.Errors;
                if (response.Success && response.Result?.Results != null && response.Result.Results.Count > 0)
                {
                    var lastItem = response.Result.Results.Last();
                    if (lastItem.Username != _offsetUrl)
                        response.Result.Results.Remove(lastItem);
                    else
                        _hasItems = false;

                    _offsetUrl = lastItem.Username;
                    Users.AddRange(response.Result.Results);
                }
                VotersLoaded?.Invoke();
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex, User.Login, AppVersion);
            }
            return errors;
        }
    }
}
