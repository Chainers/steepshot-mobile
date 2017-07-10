using System;
using Sweetshot.Library.HttpClient;

namespace Steepshot
{
	public class BasePresenter
	{
		private static SteepshotApiClient _apiClient;

		protected static SteepshotApiClient Api
		{
			get
			{
				if (_apiClient == null)
					SwitchNetwork();
				return _apiClient;
			}
		}

		protected BaseView view;
		public BasePresenter(BaseView view)
		{
			this.view = view;
		}

		public static void SwitchNetwork()
		{
			if (UserPrincipal.Instance.CurrentNetwork == Constants.Steem)
			{
				if(UserPrincipal.Instance.IsDev)
					_apiClient = new SteepshotApiClient("https://qa.steepshot.org/api/v1/");
				else
					_apiClient = new SteepshotApiClient("https://steepshot.org/api/v1/");
			}
			else
			{
				if(UserPrincipal.Instance.IsDev)
					_apiClient = new SteepshotApiClient("https://qa.golos.steepshot.org/api/v1/");
				else
					_apiClient = new SteepshotApiClient("https://golos.steepshot.org/api/v1/");
			}
		}
	}
}
