using Android.Media;
using Java.Lang;

namespace Steepshot.CameraGL.Encoder
{
    public abstract class BaseMediaEncoder
    {
        public virtual EncoderType Type { get; }
        protected virtual int TimeoutUsec => 10000;
        protected MediaCodec Codec;
        protected MediaCodec.BufferInfo BufferInfo;
        protected MuxerWrapper MuxerWrapper;
        protected int TrackIndex;

        protected abstract void SignalEndOfInputStream();

        public virtual void DrainEncoder(bool endOfStream)
        {
            if (endOfStream)
            {
                SignalEndOfInputStream();
            }

            while (true)
            {
                var encoderStatus = Codec.DequeueOutputBuffer(BufferInfo, TimeoutUsec);
                if (encoderStatus == (int)MediaCodecInfoState.TryAgainLater)
                {
                    if (!endOfStream)
                    {
                        break;
                    }
                }
                else if (encoderStatus == (int)MediaCodecInfoState.OutputFormatChanged)
                {
                    if (MuxerWrapper.IsMuxerStarted)
                    {
                        throw new RuntimeException("format changed twice");
                    }

                    var newFormat = Codec.OutputFormat;

                    TrackIndex = MuxerWrapper.AddTrack(newFormat);
                    MuxerWrapper.RegisterEncoder(this);
                }
                else if (encoderStatus >= 0)
                {
                    var encodedData = Codec.GetOutputBuffer(encoderStatus);
                    if (encodedData == null)
                    {
                        throw new RuntimeException("encoderOutputBuffer " + encoderStatus +
                                                   " was null");
                    }

                    if ((BufferInfo.Flags & MediaCodecBufferFlags.CodecConfig) != 0)
                    {
                        BufferInfo.Size = 0;
                    }

                    if (BufferInfo.Size != 0)
                    {
                        if (!MuxerWrapper.IsMuxerStarted)
                        {
                            continue;
                        }

                        encodedData.Position(BufferInfo.Offset);
                        encodedData.Limit(BufferInfo.Offset + BufferInfo.Size);
                        MuxerWrapper.WriteSampleData(TrackIndex, encodedData, BufferInfo);
                    }

                    Codec.ReleaseOutputBuffer(encoderStatus, false);

                    if ((BufferInfo.Flags & MediaCodecBufferFlags.EndOfStream) != 0)
                    {
                        break;
                    }
                }
            }
        }

        public virtual void Release()
        {
            if (Codec != null)
            {
                Codec.Stop();
                Codec.Release();
                Codec = null;
            }

            MuxerWrapper.UnRegisterEncoder(this);
        }
    }
}