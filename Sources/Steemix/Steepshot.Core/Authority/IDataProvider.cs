using System.Collections.Generic;

namespace Steepshot.Core.Authority
{
    public interface IDataProvider
    {
        List<UserInfo> Select();
        void Delete(UserInfo userInfo);
        void Insert(UserInfo currentUserInfo);
        List<UserInfo> Select(KnownChains chain);
        void Update(UserInfo currentUser);
    }
}