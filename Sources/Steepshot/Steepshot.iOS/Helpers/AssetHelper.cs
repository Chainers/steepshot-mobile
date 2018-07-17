using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Steepshot.Core.Clients;
using Steepshot.Core.Localization;
using Steepshot.Core.Services;
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

        public LocalizationModel GetLocalization(string lang)
        {
            return TryReadAsset<LocalizationModel>($"Localization.{lang}.txt");
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

        public void SetLocalization(LocalizationModel model)
        {
            TryWriteAsset($"Localization.{model.Lang}.txt", model);
        }

        public HashSet<string> TryReadCensoredWords()
        {
            var file = "CensoredWords.txt";
            var hs = new HashSet<string>();
            try
            {
                if (File.Exists(file))
                {
                    var lines = File.ReadAllLines(file);
                    foreach (var word in lines)
                    {
                        if (!string.IsNullOrEmpty(word) && !hs.Contains(word))
                            hs.Add(word.ToUpperInvariant());
                    }
                }
            }
            catch
            {
                //to do nothing
            }
            return hs;
        }

        private T TryReadAsset<T>(string file) where T : new()
        {
            try
            {
                if (File.Exists(file))
                {
                    var json = File.ReadAllText(file);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        return JsonConvert.DeserializeObject<T>(json);
                    }
                }
            }
            catch
            {
                //to do nothing
            }
            return new T();
        }

        private void TryWriteAsset<T>(string file, T data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                if (File.Exists(file))
                {
                    File.WriteAllText(file, json);
                }
            }
            catch
            {
                //to do nothing
            }
        }
    }
}
