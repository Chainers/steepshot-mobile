namespace Steepshot.Core.Models.Common
{
    public class FrameSize
    {
        public int Width { get; set; }

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
