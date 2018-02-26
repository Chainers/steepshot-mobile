using System.Collections.Generic;

namespace Steepshot.Utils.GifDecoder
{
    public class GifHeader
    {
        public int[] Gct { get; set; } = null;
        public int Status { get; set; } = GifDecoder.STATUS_OK;
        public int FrameCount { get; set; } = 0;

        public GifFrame CurrentFrame { get; set; }
        public List<GifFrame> Frames { get; set; } = new List<GifFrame>();

        public int Width { get; set; }
        public int Height { get; set; }

        public bool GctFlag { get; set; }
        public int GctSize { get; set; }
        public int BgIndex { get; set; }
        public int PixelAspect { get; set; }
        public int BgColor { get; set; }
        public int LoopCount { get; set; } = 0;
    }
}