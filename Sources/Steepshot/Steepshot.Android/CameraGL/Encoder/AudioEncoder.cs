using Android.Media;
using Java.Lang;

namespace Steepshot.CameraGL.Encoder
{
    public class AudioEncoder : BaseMediaEncoder
    {
        public override EncoderType Type => EncoderType.Audio;
        public override CircularEncoderBuffer CircularBuffer { get; protected set; }
        public sealed override MediaFormat Format { get; protected set; }

        public override void Configure(BaseEncoderConfig config)
        {
            if (!(config is AudioEncoderConfig))
                throw new IllegalArgumentException("You must provide audio config for this encoder");

            base.Configure(config);
        }

        protected override void InitCodec()
        {
            Release();

            var audioConfig = (AudioEncoderConfig)Config;
            Muxer = audioConfig.MuxerWrapper;
            CircularBuffer = new CircularEncoderBuffer(audioConfig.Bitrate, audioConfig.SampleRate * 2 / audioConfig.SamplesPerFrame, audioConfig.BufferSizeSec);

            Format = MediaFormat.CreateAudioFormat(audioConfig.MimeType, audioConfig.SampleRate, 1);
            Format.SetInteger(MediaFormat.KeyAacProfile, (int)MediaCodecProfileType.Aacobjectlc);
            Format.SetInteger(MediaFormat.KeyChannelMask, (int)audioConfig.ChanelConfig);
            Format.SetInteger(MediaFormat.KeyBitRate, audioConfig.Bitrate);

            Codec = MediaCodec.CreateEncoderByType(audioConfig.MimeType);
            Codec.Configure(Format, null, null, MediaCodecConfigFlags.Encode);
        }

        public override void Start()
        {
            InitCodec();
            Codec.Start();
        }

        public override void Stop(bool save = false)
        {
            if (save)
            {
                DrainEncoder(true);
                Muxer.MuxingFinished += MuxingFinished;
                Muxer.AddTrack(this);
                return;
            }

            Release();
        }

        public void Poll(byte[] buffer, long presentationTime)
        {
            var inputBufferIndex = Codec.DequeueInputBuffer(TimeoutUsec);
            if (inputBufferIndex >= 0)
            {
                var inputBuffer = Codec.GetInputBuffer(inputBufferIndex);
                inputBuffer.Clear();
                inputBuffer.Put(buffer);
                Codec.QueueInputBuffer(inputBufferIndex, 0, buffer.Length, presentationTime, MediaCodecBufferFlags.None);
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