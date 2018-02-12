using System.Collections.Generic;

namespace Steepshot.Core.Models.Common
{
    public class MediaModel
    {
        public Dictionary<int, string> Thumbnails { get; set; }

        public string Url { get; set; }

        public string IpfsHash { get; set; }

        public FrameSize Size { get; set; }
    }
}
