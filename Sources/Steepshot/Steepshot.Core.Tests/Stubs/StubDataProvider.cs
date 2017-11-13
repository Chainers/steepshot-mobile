using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Steepshot.Core.Authority;

namespace Steepshot.Core.Tests.Stubs
{
    public class StubDataProvider : IDataProvider
    {
        private readonly List<UserInfo> _set;

        public StubDataProvider()
        {
            _set = new List<UserInfo>
            {
                new UserInfo
                {
                    Chain = KnownChains.Golos,
                    Login = "joseph.kalu",
                    PostingKey = ConfigurationManager.AppSettings["GolosWif"]
                },
                new UserInfo
                {
                    Chain = KnownChains.Steem,
                    Login = "joseph.kalu",
                    PostingKey = ConfigurationManager.AppSettings["SteemWif"]
                },
                new UserInfo
                {
                    Chain = KnownChains.GolosTestNet,
                    Login = "joseph.kalu",
                    PostingKey = ConfigurationManager.AppSettings["GolosTestNetWif"]
                }
            };
        }

        public void Delete(UserInfo userInfo)
        {
            _set.Remove(userInfo);
        }

        public void Insert(UserInfo currentUserInfo)
        {
            _set.Add(currentUserInfo);
        }

        public List<UserInfo> Select()
        {
            return _set;
        }

        public List<UserInfo> Select(KnownChains chain)
        {
            return _set.Where(c => c.Chain == chain).ToList();
        }

        public void Update(UserInfo currentUser)
        {
            for (int i = 0; i < _set.Count; i++)
            {
                var itm = _set[i];
                if (itm.Login == currentUser.Login && itm.Chain == currentUser.Chain)
                {
                    _set[i] = currentUser;
                }
            }
        }
    }
}
