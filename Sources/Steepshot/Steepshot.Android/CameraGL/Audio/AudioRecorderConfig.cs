using Android.Media;
using Java.Lang;
using Steepshot.CameraGL.Encoder;

namespace Steepshot.CameraGL.Audio
{
    public class AudioRecorderConfig : Object
    {
        public AudioEncoderConfig AudioEncoderConfig { get; }
        public int SampleRate { get; }
        public int SamplesPerFrame { get; }
        public ChannelIn ChanelConfig { get; }
        public Encoding AudioFormat { get; }

        public AudioRecorderConfig(AudioEncoderConfig encoderConfig, Encoding audioFormat)
        {
            AudioEncoderConfig = encoderConfig;
            SampleRate = encoderConfig.SampleRate;
            SamplesPerFrame = encoderConfig.SamplesPerFrame;
            ChanelConfig = encoderConfig.ChanelConfig;
            AudioFormat = audioFormat;
        }
    }
}