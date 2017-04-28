using System;
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

					var items = db.Query<UserInfo>("select * FROM UserInfo");
					return (items != null && items.Count > 0) ? true : false;
				}
				catch (Exception e)
				{
					return false;
				}
			}
		}

		public void CreatePrincipal(LoginResponse result, string login, string pass)
		{
			var userInfo = new UserInfo();
			userInfo.Login = login;
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

		private UserInfo CurrentSessionUser;

		public UserInfo CurrentUser
		{
			get
			{
				if (CurrentSessionUser == null)
				{
					try
					{
						var user = db.Query<UserInfo>("SELECT * FROM UserInfo ORDER BY ROWID ASC LIMIT 1")[0];
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
	}
}
