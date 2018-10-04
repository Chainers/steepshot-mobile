using Android.Media;
using Java.Lang;

namespace Steepshot.CameraGL.Encoder
{
    public class AudioEncoder : BaseMediaEncoder
    {
        public override EncoderType Type => EncoderType.Audio;

        public AudioEncoder(AudioEncoderConfig config)
        {
            MuxerWrapper = config.MuxerWrapper;
            BufferInfo = new MediaCodec.BufferInfo();

            var format = MediaFormat.CreateAudioFormat(config.MimeType, config.SampleRate, 1);
            format.SetInteger(MediaFormat.KeyAacProfile, (int)MediaCodecProfileType.Aacobjectlc);
            format.SetInteger(MediaFormat.KeyChannelMask, (int)ChannelIn.Mono);
            format.SetInteger(MediaFormat.KeyBitRate, config.Bitrate);
            format.SetInteger(MediaFormat.KeyChannelCount, 1);

            Codec = MediaCodec.CreateEncoderByType(config.MimeType);
            Codec.Configure(format, null, null, MediaCodecConfigFlags.Encode);
            Codec.Start();
        }

        public void Poll(byte[] buffer, long presentationTime)
        {
            var inputBufferIndex = Codec.DequeueInputBuffer(TimeoutUsec);
            if (inputBufferIndex >= 0)
            {
                var inputBuffer = Codec.GetInputBuffer(inputBufferIndex);
                inputBuffer.Clear();
                inputBuffer.Put(buffer);
                Codec.QueueInputBuffer(inputBufferIndex, 0, buffer.Length, presentationTime, 0);
            }
        }

        protected override void SignalEndOfInputStream()
        {
            var inputBufferIndex = Codec.DequeueInputBuffer(TimeoutUsec);
            if (inputBufferIndex >= 0)
            {
                var inputBuffer = Codec.GetInputBuffer(inputBufferIndex);
                inputBuffer.Clear();
                Codec.QueueInputBuffer(inputBufferIndex, 0, 0, JavaSystem.NanoTime() / 1000L, MediaCodecBufferFlags.EndOfStream);
            }
        }
    }
}