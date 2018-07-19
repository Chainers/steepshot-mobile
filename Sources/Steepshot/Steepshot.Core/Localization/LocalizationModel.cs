using System.Collections.Generic;

namespace Steepshot.Core.Localization
{
    public class LocalizationModel
    {
        public string Lang { get; set; } = "en";

        public int Version { get; set; } = 0;

        public Dictionary<string, string> Map { get; set; } = new Dictionary<string, string>();
    }
}
