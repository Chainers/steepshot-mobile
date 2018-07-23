using System.Collections.Generic;

namespace Steepshot.Core.Localization
{
    public class LocalizationModel
    {
        public string Lang { get; set; } = string.Empty;

        public int Version { get; set; }

        public Dictionary<string, string> Map { get; set; } = new Dictionary<string, string>();
    }
}
