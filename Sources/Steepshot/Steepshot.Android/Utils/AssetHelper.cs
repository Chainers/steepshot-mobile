using System.Collections.Generic;
using System.IO;
using Android.Content.Res;
using Newtonsoft.Json;
using Steepshot.Core.Clients;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Utils;

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

        public string GetLocalization(string lang)
        {
            var txt = string.Empty;
            Stream stream = null;
            StreamReader sr = null;
            try
            {
                stream = _assetManager.Open($@"Languages/{lang}/dic.xml");
                sr = new StreamReader(stream);
                txt = sr.ReadToEnd();
            }
            catch (System.Exception ex)
            {
                AppSettings.Logger.WarningAsync(ex);
            }
            finally
            {
                sr?.Dispose();
                stream?.Dispose();
            }
            return txt;
        }

        public List<NodeConfig> SteemNodesConfig()
        {
            return TryReadJsonAsset<List<NodeConfig>>("SteemNodesConfig.txt");
        }

        public List<NodeConfig> GolosNodesConfig()
        {
            return TryReadJsonAsset<List<NodeConfig>>("GolosNodesConfig.txt");
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
                AppSettings.Logger.WarningAsync(ex);
            }
            return new T();
        }
    }
}
