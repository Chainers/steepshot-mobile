using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Steepshot.Core.Clients;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Utils;

namespace Steepshot.iOS.Helpers
{
    public sealed class AssetHelper : IAssetHelper
    {
        public DebugInfo GetDebugInfo()
        {
            return TryReadAsset<DebugInfo>("DebugWif.txt");
        }

        public ConfigInfo GetConfigInfo()
        {
            return TryReadAsset<ConfigInfo>("Config.txt");
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


        public string GetLocalization(string lang)
        {
            var txt = string.Empty;
            try
            {
                txt = File.ReadAllText($@"Languages/{lang}/dic.xml");
            }
            catch (Exception ex)
            {
                AppDelegate.Logger.WarningAsync(ex);
            }
            return txt;
        }

        private T TryReadAsset<T>(string file) where T : new()
        {
            try
            {
                file = $"Assets/{file}";
                if (File.Exists(file))
                {
                    var json = File.ReadAllText(file);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        return JsonConvert.DeserializeObject<T>(json);
                    }
                }
            }
            catch (Exception ex)
            {
                AppDelegate.Logger.WarningAsync(ex);
            }
            return new T();
        }
    }
}
