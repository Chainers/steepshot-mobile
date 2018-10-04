using Java.Lang;

namespace Steepshot.CameraGL.Encoder
{
    public class AudioEncoderConfig : Object
    {
        public MuxerWrapper MuxerWrapper { get; }
        public string MimeType { get; }
        public int SampleRate { get; }
        public int SamplesPerFrame { get; }
        public int Bitrate { get; }

        public AudioEncoderConfig(MuxerWrapper muxerWrapper, string mimeType, int sampleRate, int samplesPerFrame, int bitrate)
        {
            MuxerWrapper = muxerWrapper;
            MimeType = mimeType;
            SampleRate = sampleRate;
            SamplesPerFrame = samplesPerFrame;
            Bitrate = bitrate;
        }
    }
}