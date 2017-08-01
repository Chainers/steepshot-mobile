using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Akavache;
using System.Reactive.Linq;
using System.Threading;

namespace Steepshot.Core.Authority
{
	public class DataProvider : IDataProvider
	{
		private List<UserInfo> _set;

        ManualResetEvent mre = new ManualResetEvent(false);

		public DataProvider()
		{
            BlobCache.ApplicationName = "Steepshot";
			BlobCache.Secure.GetObject<List<UserInfo>>(Constants.UserContextKey)
                     .Subscribe((obj) =>
                     {
                         _set = obj;
                         mre.Set();
                     }, ex =>
                     {
                        _set = new List<UserInfo>();
						 mre.Set();
                     });
            mre.WaitOne();
		}

        public List<UserInfo> Select()
		{
			return _set;
		}

		public void Delete(UserInfo userInfo)
		{
			for (int i = 0; i < _set.Count; i++)
			{
				if (_set[i].Id == userInfo.Id)
				{
					_set.RemoveAt(i);
					break;
				}
			}
			Save();
		}

		public void Insert(UserInfo currentUserInfo)
		{
			if (currentUserInfo.Id == 0)
				currentUserInfo.Id = _set.Any() ? _set.Max(i => i.Id) + 1 : 1;
			_set.Add(currentUserInfo);
			Save();
		}

		public List<UserInfo> Select(KnownChains chain)
		{
			return _set.Where(i => i.Chain == chain).ToList();
		}

		public void Update(UserInfo userInfo)
		{
			for (int i = 0; i < _set.Count; i++)
			{
				if (_set[i].Id == userInfo.Id)
				{
					_set[i] = userInfo;
					break;
				}
			}
			Save();
		}

        private void Save()
        {
            BlobCache.Secure.InsertObject(Constants.UserContextKey, _set);
        }			
	}
}
