using Android.Media;
using Java.Lang;
using Steepshot.CameraGL.Encoder;

namespace Steepshot.CameraGL
{
    public class MuxerTrack : Object
    {
        public MediaFormat OutputFormat { get; protected set; }
        public CircularEncoderBuffer CircularBuffer { get; protected set; }
        public EncoderType Type { get; protected set; }

        protected MuxerTrack()
        {
        }

        public MuxerTrack(EncoderType type, MediaFormat outputFormat, CircularEncoderBuffer circularBuffer)
        {
            Type = type;
            OutputFormat = outputFormat;
            CircularBuffer = circularBuffer;
        }
    }
}