using System.IO;
using Android.Content.Res;
using Newtonsoft.Json;

namespace Steepshot.Utils
{
    public class DebugInfo
    {
        public string SteemTestLogin { get; set; }

        public string SteemTestWif { get; set; }

        public string GolosTestLogin { get; set; }

        public string GolosTestWif { get; set; }
    }

    public class ConfigInfo
    {
        public string RavenClientDSN { get; set; }
    }

    public class AssetsHelper
    {
        public static DebugInfo GetDebugInfo(AssetManager assetManager)
        {
            return TryReadAsset<DebugInfo>(assetManager, "DebugWif.txt");
        }

        public static ConfigInfo GetConfigInfo(AssetManager assetManager)
        {
            return TryReadAsset<ConfigInfo>(assetManager, "Config.txt");
        }

        private static T TryReadAsset<T>(AssetManager assetManager, string file) where T : new()
        {
            try
            {
                string txt;
                var stream = assetManager.Open(file);
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