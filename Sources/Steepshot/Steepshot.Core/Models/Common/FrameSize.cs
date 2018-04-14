using Newtonsoft.Json;

namespace Steepshot.Core.Models.Common
{
    [JsonObject(MemberSerialization.OptIn)]
    public class FrameSize
    {
        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }


        public FrameSize() { }

        public FrameSize(int height, int width)
        {
            Height = height;
            Width = width;
        }

        public override string ToString()
        {
            return $"{Height}x{Width}";
        }
    }
}
