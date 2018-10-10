using Android.Media;

namespace Steepshot.CameraGL.Encoder
{
    public class AudioEncoder : BaseMediaEncoder
    {
        public override EncoderType Type => EncoderType.Audio;
        public sealed override MediaFormat Format { get; }

        public AudioEncoder(AudioEncoderConfig config)
        {
            MuxerWrapper = config.MuxerWrapper;

            Format = MediaFormat.CreateAudioFormat(config.MimeType, config.SampleRate, 1);
            Format.SetInteger(MediaFormat.KeyAacProfile, (int)MediaCodecProfileType.Aacobjectlc);
            Format.SetInteger(MediaFormat.KeyChannelMask, (int)ChannelIn.Mono);
            Format.SetInteger(MediaFormat.KeyBitRate, config.Bitrate);
            Format.SetInteger(MediaFormat.KeyChannelCount, 1);

            Codec = MediaCodec.CreateEncoderByType(config.MimeType);
            Codec.Configure(Format, null, null, MediaCodecConfigFlags.Encode);
            Codec.Start();
        }

        private long _prevFrameTime;
        public void Poll(byte[] buffer, long presentationTime)
        {
            var inputBufferIndex = Codec.DequeueInputBuffer(TimeoutUsec);
            if (inputBufferIndex >= 0)
            {
                var inputBuffer = Codec.GetInputBuffer(inputBufferIndex);
                inputBuffer.Clear();
                inputBuffer.Put(buffer);
                Codec.QueueInputBuffer(inputBufferIndex, 0, buffer.Length, presentationTime, MediaCodecBufferFlags.None);
                _prevFrameTime = presentationTime;
            }
        }

        protected override void SignalEndOfInputStream()
        {
            var inputBufferIndex = Codec.DequeueInputBuffer(TimeoutUsec);
            if (inputBufferIndex >= 0)
            {
                var inputBuffer = Codec.GetInputBuffer(inputBufferIndex);
                inputBuffer.Clear();
                Codec.QueueInputBuffer(inputBufferIndex, 0, 0, _prevFrameTime, MediaCodecBufferFlags.EndOfStream);
            }
        }
    }
}