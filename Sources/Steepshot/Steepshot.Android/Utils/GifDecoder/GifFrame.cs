namespace Steepshot.Utils.GifDecoder
{
    public class GifFrame
    {
        public int Ix { get; set; }
        public int Iy { get; set; }
        public int Iw { get; set; }
        public int Ih { get; set; }
        public bool Interlace { get; set; }
        public bool Transparency { get; set; }
        public int Dispose { get; set; }
        public int TransIndex { get; set; }
        public int Delay { get; set; }
        public int BufferFrameStart { get; set; }
        public int[] Lct { get; set; }
    }
}