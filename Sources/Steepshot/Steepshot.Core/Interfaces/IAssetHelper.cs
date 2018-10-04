using System.Collections.Generic;
using Steepshot.Core.Clients;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Interfaces
{
    public interface IAssetHelper
    {
        ConfigInfo GetConfigInfo();

        DebugInfo GetDebugInfo();

        string GetLocalization(string lang);

        List<NodeConfig> SteemNodesConfig();

        List<NodeConfig> GolosNodesConfig();

        Dictionary<string, string> IntegrationModuleConfig();
    }
}