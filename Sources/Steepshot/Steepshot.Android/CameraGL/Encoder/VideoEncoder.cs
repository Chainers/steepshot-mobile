using Android.Media;
using Android.Views;

namespace Steepshot.CameraGL.Encoder
{
    public class VideoEncoder : BaseMediaEncoder
    {
        public override EncoderType Type => EncoderType.Video;

        public Surface InputSurface { get; }

        public VideoEncoder(VideoEncoderConfig config)
        {
            MuxerWrapper = config.MuxerWrapper;
            BufferInfo = new MediaCodec.BufferInfo();

            var format = MediaFormat.CreateVideoFormat(config.MimeType, config.Width, config.Height);

            format.SetInteger(MediaFormat.KeyColorFormat, (int)MediaCodecCapabilities.Formatsurface);
            format.SetInteger(MediaFormat.KeyBitRate, config.BitRate);
            format.SetInteger(MediaFormat.KeyFrameRate, config.FrameRate);
            format.SetInteger(MediaFormat.KeyIFrameInterval, config.IframeInterval);

            Codec = MediaCodec.CreateEncoderByType(config.MimeType);
            Codec.Configure(format, null, null, MediaCodecConfigFlags.Encode);
            InputSurface = Codec.CreateInputSurface();
            Codec.Start();
        }

        protected override void SignalEndOfInputStream()
        {
            Codec.SignalEndOfInputStream();
        }
    }
}