using System.Collections.Generic;
using System.IO;
using Android.Content.Res;
using Newtonsoft.Json;
using Steepshot.Core.Clients;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;
using Steepshot.Core.Services;

namespace Steepshot.Utils
{
    public sealed class AssetHelper : IAssetHelper
    {
        private readonly AssetManager _assetManager;

        public AssetHelper(AssetManager assetManager)
        {
            _assetManager = assetManager;
        }

        public DebugInfo GetDebugInfo()
        {
            return TryReadJsonAsset<DebugInfo>("DebugWif.txt");
        }

        public ConfigInfo GetConfigInfo()
        {
            return TryReadJsonAsset<ConfigInfo>("Config.txt");
        }

        public LocalizationModel GetLocalization(string lang)
        {
            try
            {
                string txt;
                var stream = _assetManager.Open($@"Languages/{lang}/dic.xml");
                using (var sr = new StreamReader(stream))
                    txt = sr.ReadToEnd();

                stream.Dispose();
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
            var hs = new HashSet<string>();
            try
            {
                var stream = _assetManager.Open("CensoredWords.txt");
                using (var sr = new StreamReader(stream))
                {
                    while (!sr.EndOfStream)
                    {
                        var word = sr.ReadLine();
                        if (!string.IsNullOrEmpty(word) && !hs.Contains(word))
                            hs.Add(word.ToUpperInvariant());
                    }
                }
                stream.Dispose();
            }
            catch (System.Exception ex)
            {
                AppSettings.Logger.Warning(ex);
            }
            return hs;
        }

        public List<NodeConfig> SteemNodesConfig()
        {
            return TryReadJsonAsset<List<NodeConfig>>("SteemNodesConfig.txt");
        }

        public List<NodeConfig> GolosNodesConfig()
        {
            return TryReadJsonAsset<List<NodeConfig>>("GolosNodesConfig.txt");
        }

        public List<NodeConfig> EosNodesConfig()
        {
            return TryReadJsonAsset<List<NodeConfig>>("EosNodesConfig.txt");
        }

        public Dictionary<string, string> IntegrationModuleConfig()
        {
            return TryReadJsonAsset<Dictionary<string, string>>("IntegrationModuleConfig.txt");
        }


        public T TryReadJsonAsset<T>(string file)
            where T : new()
        {
            try
            {
                string txt;
                var stream = _assetManager.Open(file);
                using (var sr = new StreamReader(stream))
                {
                    txt = sr.ReadToEnd();
                }
                stream.Dispose();
                if (!string.IsNullOrWhiteSpace(txt))
                {
                    return JsonConvert.DeserializeObject<T>(txt);
                }
            }
            catch (System.Exception ex)
            {
                AppSettings.Logger.Warning(ex);
            }
            return new T();
        }
    }
}
