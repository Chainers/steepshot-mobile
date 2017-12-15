using Steepshot.Core.Utils;
using System.Collections.Generic;

namespace Steepshot.Core.Services
{
    public interface IAssetsHelper
    {
        HashSet<string> TryReadCensoredWords();

        ConfigInfo GetConfigInfo();

        DebugInfo GetDebugInfo();
    }
}