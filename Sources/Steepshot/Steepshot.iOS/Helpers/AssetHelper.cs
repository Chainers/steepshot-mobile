using System;
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


        public LocalizationModel GetLocalization(string lang)
        {
            try
            {
                var txt = File.ReadAllText($@"Languages/{lang}/dic.xml");

                if (!string.IsNullOrWhiteSpace(txt))
                {
                    var model = new LocalizationModel();
                    LocalizationManager.Update(txt, model);
                    return model;
                }
            }
            catch (System.Exception ex)
            {
                AppSettings.Logger.Warning(ex);
            }
            return new LocalizationModel();
        }

        public HashSet<string> TryReadCensoredWords()
        {
            var file = "Assets/CensoredWords.txt";
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
            catch (Exception ex)
            {
                AppSettings.Logger.Warning(ex);
            }
            return hs;
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
                AppSettings.Logger.Warning(ex);
            }
            return new T();
        }
    }
}
