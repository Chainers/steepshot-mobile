using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Steepshot.Core.HttpClient;
using Steepshot.Core.Localization;
using Steepshot.Core.Services;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Tests.Stubs
{
    public class AssetsHelperStub : IAssetsHelper
    {
        public ConfigInfo GetConfigInfo()
        {
            throw new NotImplementedException();
        }

        public DebugInfo GetDebugInfo()
        {
            throw new NotImplementedException();
        }

        public LocalizationModel GetLocalization(string lang)
        {
            var jsonLocalization = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}\\Localization.en-us.txt");
            return JsonConvert.DeserializeObject<LocalizationModel>(jsonLocalization);
        }

        public List<NodeConfig> SteemNodesConfig()
        {
            throw new NotImplementedException();
        }

        public List<NodeConfig> GolosNodesConfig()
        {
            throw new NotImplementedException();
        }

        public HashSet<string> TryReadCensoredWords()
        {
            throw new NotImplementedException();
        }
    }
}
