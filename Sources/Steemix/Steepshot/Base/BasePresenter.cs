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
					_apiClient = new SteepshotApiClient("https://steepshot.org/api/v1/");
				return _apiClient;
			}
		}

		protected BaseView view;
		public BasePresenter(BaseView view)
		{
			this.view = view;
		}

	}
}
