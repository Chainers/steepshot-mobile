using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Steepshot.Core.Clients;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Tests.Stubs
{
    public class AssetsHelperStub : IAssetHelper
    {
        public DebugInfo GetDebugInfo()
        {
            return TryReadAsset<DebugInfo>("DebugWif.txt");
        }

        public ConfigInfo GetConfigInfo()
        {
            return TryReadAsset<ConfigInfo>("Config.txt");
        }

        public string GetLocalization(string lang)
        {
            return File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}\\Data\\dic.xml");
        }

        public List<NodeConfig> SteemNodesConfig()
        {
            return TryReadAsset<List<NodeConfig>>("SteemNodesConfig.txt");
        }

        public List<NodeConfig> GolosNodesConfig()
        {
            return TryReadAsset<List<NodeConfig>>("GolosNodesConfig.txt");
        }

        public Dictionary<string, string> IntegrationModuleConfig()
        {
            return TryReadAsset<Dictionary<string, string>>("IntegrationModuleConfig.txt");
        }

        public T TryReadAsset<T>(string file)
            where T : new()
        {
            try
            {
                var json = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}\\{file}");
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch
            {
                //to do nothing
            }
            return new T();
        }
    }
}
