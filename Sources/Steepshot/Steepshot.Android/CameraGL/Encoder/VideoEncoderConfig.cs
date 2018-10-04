using Android.Opengl;
using Java.Lang;

namespace Steepshot.CameraGL.Encoder
{
    public class VideoEncoderConfig : Object
    {
        public MuxerWrapper MuxerWrapper { get; }
        public int Width { get; }
        public int Height { get; }
        public string MimeType { get; }
        public int FrameRate { get; }
        public int IframeInterval { get; }
        public int BitRate { get; }
        public EGLContext EglContext { get; set; }
        public float CameraAspectRatio { get; set; }

        public VideoEncoderConfig(MuxerWrapper muxerWrapper, int width, int height, string mimeType, int frameRate, int iframeInterval, int bitRate)
        {
            MuxerWrapper = muxerWrapper;
            Width = width;
            Height = height;
            MimeType = mimeType;
            FrameRate = frameRate;
            IframeInterval = iframeInterval;
            BitRate = bitRate;
            CameraAspectRatio = 1;
        }
    }
}