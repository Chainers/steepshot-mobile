using System;
using System.Collections.Generic;
using Steepshot.Core;
using Steepshot.Core.Authority;
using Steepshot.SQLite;

namespace Steepshot.Data
{
    internal class DataProvider : IDataProvider
    {
        private SQLiteConnection Db;

        public DataProvider()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            Db = new SQLiteConnection(System.IO.Path.Combine(folder, "user.db"));
            Db.CreateTable<UserInfo>(CreateFlags.ImplicitPK | CreateFlags.AutoIncPK);
        }
        
        public void Delete(UserInfo userInfo)
        {
            Db.Delete(userInfo);
        }

        public void Insert(UserInfo userInfo)
        {
            Db.Insert(userInfo);
        }

        public List<UserInfo> Select()
        {
            return Db.Query<UserInfo>("select * FROM UserInfo");
        }

        public List<UserInfo> Select(KnownChains chain)
        {
            return Db.Query<UserInfo>($"SELECT * FROM UserInfo WHERE Chain=\"{(int)chain}\"");
        }

        public void Update(UserInfo currentUser)
        {
            Db.Update(currentUser);
        }
    }
}