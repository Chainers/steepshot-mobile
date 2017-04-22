using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Newtonsoft.Json;

namespace Steepshot.iOS
{
    public class UserContext
    {
        [JsonIgnore]
        public static UserContext Instanse { get; } = new UserContext();
        [JsonIgnore]
        private static readonly object _lock = new object();
		[JsonIgnore]
		public List<string> TagsList { get; set; } = new List<string>();
		[JsonIgnore]
		public bool ShouldProfileUpdate { get; set; }
		[JsonIgnore]
		public bool NetworkChanged { get; set; }

		public bool IsDev { get; set; }

		public List<Account> Accounts { get; set; } = new List<Account>();

		[JsonIgnore]
		public string Token
		{
			get
			{
				return Accounts.FirstOrDefault(a => a.Network == Network)?.Token;
			}
		}

		[JsonIgnore]
		public string Username
		{
			get
			{
				return Accounts.FirstOrDefault(a => a.Network == Network)?.Login;
			}
		}

		public string Network { get; set; }

        public static void Load()
        {
            lock (_lock)
            {
                var appSettings = NSUserDefaults.StandardUserDefaults.StringForKey(Constants.UserContextKey);
				if (appSettings != null)
				{
					var loadedInstance = JsonConvert.DeserializeObject<UserContext>(appSettings);
					Instanse.Accounts = loadedInstance.Accounts;
					//Instanse.Username = loadedInstance.Username;
					Instanse.Network = loadedInstance.Network;
				}
				else
				{
					//Set default
					Instanse.Network = Constants.Steem;
				}
            }
        }

        public static void Save()
        {
            lock (_lock)
            {
                var context = JsonConvert.SerializeObject(Instanse);
                NSUserDefaults.StandardUserDefaults.SetString(context, Constants.UserContextKey);
                NSUserDefaults.StandardUserDefaults.Synchronize();
            }
        }
    }
}
