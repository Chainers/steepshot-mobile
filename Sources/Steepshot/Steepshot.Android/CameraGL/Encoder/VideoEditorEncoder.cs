namespace Steepshot.CameraGL.Encoder
{
    public class VideoEditorEncoder : VideoEncoder
    {
        public override void Configure(BaseEncoderConfig config)
        {
            Config = config;
        }

        public override void Start()
        {
            InitCodec();
            Codec.Start();
        }

        public override void Stop(bool save = false)
        {
            Release();
        }
    }
}