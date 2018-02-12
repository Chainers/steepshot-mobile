using System.Collections.Generic;

namespace Steepshot.Core.Localization
{
    public class LocalizationModel
    {
        public string Lang { get; set; }

        public int Version { get; set; }

        public Dictionary<string, string> Map { get; set; }
    }
}
