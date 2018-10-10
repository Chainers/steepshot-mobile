using System.Diagnostics;
using Android.Media;
using Java.Lang;

namespace Steepshot.CameraGL.Encoder
{
    public abstract class BaseMediaEncoder
    {
        public abstract MediaFormat Format { get; }
        public abstract EncoderType Type { get; }
        protected int TimeoutUsec => 0;
        protected MediaCodec Codec;
        protected MuxerWrapper MuxerWrapper;
        public int TrackIndex;

        protected abstract void SignalEndOfInputStream();

        public void DrainEncoder(bool endOfStream)
        {
            if (endOfStream)
            {
                SignalEndOfInputStream();
            }

            while (true)
            {
                var bufferInfo = new MediaCodec.BufferInfo();
                var encoderStatus = Codec.DequeueOutputBuffer(bufferInfo, TimeoutUsec);
                if (encoderStatus == (int)MediaCodecInfoState.TryAgainLater)
                {
                    if (!endOfStream)
                    {
                        break;
                    }
                }
                else if (encoderStatus == (int)MediaCodecInfoState.OutputFormatChanged)
                {
                    if (MuxerWrapper.IsMuxing())
                    {
                        throw new RuntimeException("format changed twice");
                    }

                    var newFormat = Codec.OutputFormat;

                    TrackIndex = MuxerWrapper.AddTrack(this, newFormat);
                }
                else if (encoderStatus >= 0)
                {
                    var encodedData = Codec.GetOutputBuffer(encoderStatus);
                    if (encodedData == null)
                    {
                        throw new RuntimeException("encoderOutputBuffer " + encoderStatus +
                                                   " was null");
                    }

                    if ((bufferInfo.Flags & MediaCodecBufferFlags.CodecConfig) != 0)
                    {
                        bufferInfo.Size = 0;
                    }

                    if (bufferInfo.Size != 0)
                    {
                        if (MuxerWrapper.IsMuxing())
                        {
                            encodedData.Position(bufferInfo.Offset);
                            encodedData.Limit(bufferInfo.Offset + bufferInfo.Size);
                            Debug.WriteLine(Type + "        " + bufferInfo.PresentationTimeUs);
                            MuxerWrapper.WriteSampleData(TrackIndex, encodedData, bufferInfo);
                        }
                    }

                    Codec.ReleaseOutputBuffer(encoderStatus, false);


                    if ((bufferInfo.Flags & MediaCodecBufferFlags.EndOfStream) != 0)
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

            MuxerWrapper.StopTrack(this);
        }
    }
}