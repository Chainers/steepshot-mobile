namespace Steepshot.CameraGL.Encoder
{
    public class VideoEditorEncoder : VideoEncoder
    {
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
    }
}