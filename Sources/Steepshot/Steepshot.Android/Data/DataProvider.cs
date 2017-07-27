using System;
using System.Collections.Generic;
using Steepshot.Core;
using Steepshot.Core.Authority;
using Steepshot.SQLite;

namespace Steepshot.Data
{
    internal class DataProvider : IDataProvider
    {
        private SQLiteConnection _db;

        public DataProvider()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            _db = new SQLiteConnection(System.IO.Path.Combine(folder, "user.db"));
            _db.CreateTable<UserInfo>(CreateFlags.ImplicitPK | CreateFlags.AutoIncPK);
        }
        
        public void Delete(UserInfo userInfo)
        {
            _db.Delete(userInfo);
        }

        public void Insert(UserInfo userInfo)
        {
            _db.Insert(userInfo);
        }

        public List<UserInfo> Select()
        {
            return _db.Query<UserInfo>("select * FROM UserInfo");
        }

        public List<UserInfo> Select(KnownChains chain)
        {
            return _db.Query<UserInfo>($"SELECT * FROM UserInfo WHERE Chain=\"{(int)chain}\"");
        }

        public void Update(UserInfo currentUser)
        {
            _db.Update(currentUser);
        }
    }
}