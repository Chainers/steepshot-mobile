using System;
using System.Collections.Generic;
using Steepshot.Core;
using Steepshot.Core.Authority;

namespace Steepshot.iOS.Data
{
    internal class DataProvider : IDataProvider
    {
        public List<UserInfo> Select()
        {
            throw new NotImplementedException();
        }

        public void Delete(UserInfo userInfo)
        {
            throw new NotImplementedException();
        }

        public void Insert(UserInfo currentUserInfo)
        {
            throw new NotImplementedException();
        }

        public List<UserInfo> Select(KnownChains chain)
        {
            throw new NotImplementedException();
        }

        public void Update(UserInfo currentUser)
        {
            throw new NotImplementedException();
        }
    }
}