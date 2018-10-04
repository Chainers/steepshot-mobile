using Android.Media;
using Java.Lang;
using Steepshot.CameraGL.Encoder;

namespace Steepshot.CameraGL.Audio
{
    public class AudioRecorderConfig : Object
    {
        public AudioEncoderWrapper AudioEncoderWrapper { get; }
        public int SampleRate { get; }
        public int SamplesPerFrame { get; }
        public ChannelIn ChanelConfig { get; }
        public Encoding AudioFormat { get; }

        public AudioRecorderConfig(AudioEncoderWrapper audioEncoderWrapper, ChannelIn chanelConfig, Encoding audioFormat)
        {
            AudioEncoderWrapper = audioEncoderWrapper;
            SampleRate = audioEncoderWrapper.Config.SampleRate;
            SamplesPerFrame = audioEncoderWrapper.Config.SamplesPerFrame;
            ChanelConfig = chanelConfig;
            AudioFormat = audioFormat;
        }
    }
}