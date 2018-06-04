using Steepshot.Core.Localization;
using Steepshot.Core.Utils;
using System.Collections.Generic;
using Steepshot.Core.HttpClient;

namespace Steepshot.Core.Services
{
    public interface IAssetsHelper
    {
        HashSet<string> TryReadCensoredWords();

        ConfigInfo GetConfigInfo();

        DebugInfo GetDebugInfo();

        LocalizationModel GetLocalization(string lang);

        List<NodeConfig> SteemNodesConfig();

        List<NodeConfig> GolosNodesConfig();
    }
}