using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Preferences;
using SQLite;
using Sweetshot.Library.Models.Responses;

namespace Steepshot
{
	public class UserPrincipal
	{
		private static UserPrincipal _instance;
		public static UserPrincipal Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new UserPrincipal();
				}
				return _instance;
			}
		}

		private SQLiteConnection db;

		public string AppVersion;

		public string CurrentNetwork {
			get
			{
				ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
				var network = prefs.GetString ("network", Constants.Steem);
				return network;
			}
			set
			{
				ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
				ISharedPreferencesEditor editor = prefs.Edit();
				editor.PutString ("network", value);
				editor.Apply();  
			}
		}

		private UserPrincipal()
		{
			string folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			db = new SQLiteConnection(System.IO.Path.Combine(folder, "user.db"));
			db.CreateTable<UserInfo>();
		}

		public bool IsAuthenticated
		{

			get
			{
				try
				{
					if (CurrentSessionUser != null)
						return true;

					var items = db.Query<UserInfo>("SELECT * FROM UserInfo");
					if (items.Count == 1 && items[0].Network == null)
					{
						items[0].Network = Constants.Steem;
						db.Update(items[0]);
					}

					return (items != null && items.Count > 0) ? true : false;
				}
				catch (Exception e)
				{
					return false;
				}
			}
		}

		public void CreatePrincipal(LoginResponse result, string login, string pass, string network)
		{
			var userInfo = new UserInfo();
			userInfo.Login = login;
			userInfo.Network = network;
			userInfo.Password = pass;
			userInfo.SessionId = result.SessionId;
			db.Insert(userInfo);
			CurrentSessionUser = userInfo;
		}

        public string Cookie
        {
            get
            {
                if (CurrentUser != null)
                    return CurrentSessionUser.SessionId;
                else
                    return null;
            }
        }

        public void DeleteUser() {
            if (CurrentUser != null) {
                db.Delete(CurrentUser);
            }
        }

		public void DeleteUser(UserInfo user)
		{
			db.Delete(user);
		}

		private UserInfo CurrentSessionUser;

		public UserInfo CurrentUser
		{
			get
			{
				if (CurrentSessionUser == null)
				{
					try
					{
						//var user = db.Query<UserInfo>("SELECT * FROM UserInfo ORDER BY ROWID ASC LIMIT 1")[0];
						var user = db.Query<UserInfo>($"SELECT * FROM UserInfo WHERE Network=\"{CurrentNetwork}\"")[0];
						CurrentSessionUser = user;
						return CurrentSessionUser;
					}
					catch (Exception e)
					{
						return null;
					}
				}
				else
					return CurrentSessionUser;
			}
		}

		public void ClearUser()
		{
			CurrentSessionUser = null;
		}

		public List<UserInfo> GetAllAccounts()
		{
			var items = db.Query<UserInfo>("select * FROM UserInfo");
			return items;
		}
	}
}
