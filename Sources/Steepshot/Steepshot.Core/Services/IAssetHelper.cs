using Steepshot.Core.Localization;
using Steepshot.Core.Utils;
using System.Collections.Generic;
using Steepshot.Core.Clients;

namespace Steepshot.Core.Services
{
    public interface IAssetHelper
    {
        HashSet<string> TryReadCensoredWords();

        ConfigInfo GetConfigInfo();

        DebugInfo GetDebugInfo();

        LocalizationModel GetLocalization(string lang);

        List<NodeConfig> SteemNodesConfig();

        List<NodeConfig> GolosNodesConfig();

        List<NodeConfig> EosNodesConfig();

        Dictionary<string, string> IntegrationModuleConfig();
    }
}