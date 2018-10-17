namespace Steepshot.CameraGL.Encoder
{
    public abstract class BaseEncoderConfig
    {
        public MuxerWrapper MuxerWrapper { get; }
        public int BufferSizeSec { get; }

        public BaseEncoderConfig(MuxerWrapper muxerWrapper, int bufferSizeSec)
        {
            MuxerWrapper = muxerWrapper;
            BufferSizeSec = bufferSizeSec;
        }
    }
}