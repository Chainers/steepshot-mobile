using System.Collections.Generic;
using System.IO;
using Android.Content.Res;
using Newtonsoft.Json;
using Steepshot.Core.HttpClient;
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


        public LocalizationModel GetLocalization()
        {
            return TryReadAsset<LocalizationModel>("InstagramModuleOptions.txt");
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
            catch
            {
                //to do nothing
            }
            return hs;
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
            catch
            {
                //to do nothing
            }
            return new T();
        }

    }
}
