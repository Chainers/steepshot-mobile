using Android.Media;
using Android.Views;
using Java.Lang;

namespace Steepshot.CameraGL.Encoder
{
    public class VideoEncoder : BaseMediaEncoder
    {
        public sealed override MediaFormat Format { get; protected set; }
        public Surface InputSurface { get; private set; }

        public override void Configure(BaseEncoderConfig config)
        {
            if (!(config is VideoEncoderConfig))
                throw new IllegalArgumentException("You must provide video config for this encoder");

            base.Configure(config);
        }

        protected override void InitCodec()
        {
            Release();

            Type = EncoderType.Video;
            var videoConfig = (VideoEncoderConfig)Config;
            Muxer = videoConfig.MuxerWrapper;
            CircularBuffer = new CircularEncoderBuffer(videoConfig.Bitrate, videoConfig.FrameRate, Config.BufferSizeSec);

            Format = MediaFormat.CreateVideoFormat(videoConfig.MimeType, videoConfig.Width, videoConfig.Height);
            Format.SetInteger(MediaFormat.KeyColorFormat, (int)MediaCodecCapabilities.Formatsurface);
            Format.SetInteger(MediaFormat.KeyBitRate, videoConfig.Bitrate);
            Format.SetInteger(MediaFormat.KeyFrameRate, videoConfig.FrameRate);
            Format.SetInteger(MediaFormat.KeyIFrameInterval, videoConfig.IframeInterval);
            Format.SetInteger(MediaFormat.KeyProfile, 0x08);
            Format.SetInteger(MediaFormat.KeyLevel, 0x100);

            Codec = MediaCodec.CreateEncoderByType(videoConfig.MimeType);
            Codec.Configure(Format, null, null, MediaCodecConfigFlags.Encode);

            InputSurface = Codec.CreateInputSurface();
        }

        protected override void SignalEndOfInputStream()
        {
            Codec.SignalEndOfInputStream();
        }

        protected override void Release()
        {
            base.Release();
            InputSurface?.Release();
            InputSurface = null;
        }
    }
}