using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Preferences;
using Steepshot.SQLite;
using Sweetshot.Library.Models.Responses;

namespace Steepshot
{
    public static class User
    {
        private static readonly SQLiteConnection Db;
        public static string AppVersion;
        public static bool ShouldUpdateProfile;

        private static UserInfo CurrentUser { get; set; }

        public static bool IsDev
        {
            get
            {
                ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
                var isdev = prefs.GetBoolean("isdev", false);
                return isdev;
            }
            set
            {
                ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
                ISharedPreferencesEditor editor = prefs.Edit();
                editor.PutBoolean("isdev", value);
                editor.Apply();
            }
        }

        public static KnownChains Chain
        {
            get
            {
                ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
                var chain = prefs.GetInt("chain", (int)KnownChains.Steem);
                return (KnownChains)chain;
            }
            set
            {
                ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
                ISharedPreferencesEditor editor = prefs.Edit();
                editor.PutInt("chain", (int)value);
                editor.Apply();
            }
        }

        public static bool IsAuthenticated
        {
            get
            {
                return CurrentUser != null;
            }
        }

        public static string Login => CurrentUser?.Login;

        public static bool IsNsfw
        {
            get { return CurrentUser.IsNsfw; }
            set
            {
                CurrentUser.IsNsfw = value;
                SaveChanges();
            }
        }

        public static bool IsLowRated
        {
            get { return CurrentUser.IsLowRated; }
            set
            {
                CurrentUser.IsLowRated = value;
                SaveChanges();
            }
        }

        public static string SessionId
        {
            get { return CurrentUser.SessionId; }
            set
            {
                CurrentUser.SessionId = value;
                SaveChanges();
            }
        }

        static User()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            Db = new SQLiteConnection(System.IO.Path.Combine(folder, "user.db"));
            Db.CreateTable<UserInfo>();
            UserInfo user;
            if (TryLoadFromDb(out user))
            {
                CurrentUser = user;
            }
        }

        private static bool TryLoadFromDb(out UserInfo userInfo)
        {
            try
            {
                //var user = db.Query<UserInfo>("SELECT * FROM UserInfo ORDER BY ROWID ASC LIMIT 1")[0];
                var users = Db.Query<UserInfo>($"SELECT * FROM UserInfo WHERE Chain=\"{(int)Chain}\"");
                if (users != null && users.Count > 0)
                {
                    userInfo = users[0];
                    return true;
                }
            }
            catch (Exception e)
            {
            }
            userInfo = null;
            return false;
        }

        public static void UpdateAndSave(LoginResponse result, string login, string pass, KnownChains chain)
        {
            var userInfo = new UserInfo
            {
                Login = login,
                Chain = chain,
                Password = pass,
                SessionId = result.SessionId
            };

            CurrentUser = userInfo;
            SaveChanges();
        }

        private static void SaveChanges()
        {
            Db.Insert(CurrentUser);
        }

        public static void Delete()
        {
            if (CurrentUser != null)
            {
                Db.Delete(CurrentUser);
            }
        }

        public static void Delete(UserInfo user)
        {
            Db.Delete(user);
        }

        public static List<UserInfo> GetAllAccounts()
        {
            var items = Db.Query<UserInfo>("select * FROM UserInfo");
            return items;
        }
    }
}
