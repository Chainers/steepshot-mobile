using System.Collections.Generic;
using System.Configuration;
using Steepshot.Core.Authorization;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Tests.Stubs
{
    public class StubSaverService : ISaverService
    {
        private readonly Dictionary<string, object> _container;

        public StubSaverService()
        {
            _container = new Dictionary<string, object>
            {
                {
                    UserManager.UserContextKey, new List<UserInfo>
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
                        }
                    }
                },
                {
                    LocalizationManager.Localization,
                    new Dictionary<string, LocalizationModel>()
                }
            };
        }

        public void Save<T>(string key, T obj)
        {
            if (_container.ContainsKey(key))
                _container[key] = obj;
            else
                _container.Add(key, obj);
        }

        public T Get<T>(string key) where T : new()
        {
            if (_container.ContainsKey(key))
                return (T)_container[key];
            return default(T);
        }
    }
}