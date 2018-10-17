namespace Steepshot.CameraGL.Encoder
{
    public class VideoEncoderConfig : BaseEncoderConfig
    {
        public int Width { get; }
        public int Height { get; }
        public string MimeType { get; }
        public int FrameRate { get; }
        public int IframeInterval { get; }
        public int Bitrate { get; }

        public VideoEncoderConfig(MuxerWrapper muxerWrapper, int width, int height, string mimeType, int frameRate, int iframeInterval, int bitrate, int bufferSizeSec) : base(muxerWrapper, bufferSizeSec)
        {
            Width = width;
            Height = height;
            MimeType = mimeType;
            FrameRate = frameRate;
            IframeInterval = iframeInterval;
            Bitrate = bitrate;
        }
    }
}