using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Steepshot.Base;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;
using Steepshot.Fragment;
using Steepshot.View;

namespace Steepshot.Presenter
{
	public class VotersPresenter : BasePresenter
	{
		public VotersPresenter(IFollowersView view):base(view)
		{
		}

		public event VoidDelegate VotersLoaded;
		public List<VotersResult> Users = new List<VotersResult>();
		private bool _hasItems = true;
		private string _offsetUrl = string.Empty;
		private int _itemsLimit = 60;

		public void ViewLoad(string url)
		{
			if (Users.Count == 0)
				Task.Run(() => GetItems(url));
		}

		public async Task GetItems(string url)
		{
			try
			{
				if (!_hasItems)
					return;
				var request = new GetVotesRequest(url, User.CurrentUser)
				{
					Offset = _offsetUrl,
					Limit = _itemsLimit
				};

				var responce = await Api.GetPostVoters(request);
				if (responce.Success && responce?.Result?.Results != null && responce.Result.Results.Count > 0)
				{
					var lastItem = responce.Result.Results.Last();
					if (lastItem.Name != _offsetUrl)
						responce.Result.Results.Remove(lastItem);
					else
						_hasItems = false;

					_offsetUrl = lastItem.Username;
					Users.AddRange(responce.Result.Results);
				}
				VotersLoaded?.Invoke();
			}
			catch (Exception ex)
			{
				Reporter.SendCrash(ex, BasePresenter.User.Login, BasePresenter.AppVersion);
			}
		}
	}
}
