using Android.Media;
using Android.Views;

namespace Steepshot.CameraGL.Encoder
{
    public class VideoEncoder : BaseMediaEncoder
    {
        public override EncoderType Type => EncoderType.Video;
        public sealed override MediaFormat Format { get; }

        public Surface InputSurface { get; }

        public VideoEncoder(VideoEncoderConfig config)
        {
            MuxerWrapper = config.MuxerWrapper;

            Format = MediaFormat.CreateVideoFormat(config.MimeType, config.Width, config.Height);
            Format.SetInteger(MediaFormat.KeyColorFormat, (int)MediaCodecCapabilities.Formatsurface);
            Format.SetInteger(MediaFormat.KeyBitRate, config.BitRate);
            Format.SetInteger(MediaFormat.KeyFrameRate, config.FrameRate);
            Format.SetInteger(MediaFormat.KeyIFrameInterval, config.IframeInterval);

            Codec = MediaCodec.CreateEncoderByType(config.MimeType);
            Codec.Configure(Format, null, null, MediaCodecConfigFlags.Encode);
            InputSurface = Codec.CreateInputSurface();
            Codec.Start();
        }

        protected override void SignalEndOfInputStream()
        {
            Codec.SignalEndOfInputStream();
        }
    }
}