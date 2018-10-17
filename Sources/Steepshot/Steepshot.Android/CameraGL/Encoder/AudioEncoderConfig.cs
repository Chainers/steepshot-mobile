using Android.Media;

namespace Steepshot.CameraGL.Encoder
{
    public class AudioEncoderConfig : BaseEncoderConfig
    {
        public string MimeType { get; }
        public int SampleRate { get; }
        public int SamplesPerFrame { get; }
        public int Bitrate { get; }
        public ChannelIn ChanelConfig { get; }

        public AudioEncoderConfig(MuxerWrapper muxerWrapper, string mimeType, int sampleRate, int samplesPerFrame, int bitrate, ChannelIn channelIn, int bufferSizeSec) : base(muxerWrapper, bufferSizeSec)
        {
            MimeType = mimeType;
            SampleRate = sampleRate;
            SamplesPerFrame = samplesPerFrame;
            Bitrate = bitrate;
            ChanelConfig = channelIn;
        }
    }
}