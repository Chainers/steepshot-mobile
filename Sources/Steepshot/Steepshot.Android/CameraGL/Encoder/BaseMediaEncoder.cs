using System;
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

        private int totalDrains;
        private long avgTime;
        public void DrainEncoder(bool endOfStream)
        {
            var ss = DateTime.Now;
            totalDrains++;
            if (endOfStream)
            {
                SignalEndOfInputStream();
                Debug.WriteLine(Type + " draint time " + avgTime / totalDrains);
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
                            MuxerWrapper.WriteSampleData(TrackIndex, encoderStatus, encodedData, bufferInfo);
                        }
                    }
                    else
                    {
                        Codec.ReleaseOutputBuffer(encoderStatus, false);
                    }

                    if ((bufferInfo.Flags & MediaCodecBufferFlags.EndOfStream) != 0)
                    {
                        break;
                    }
                }
            }

            avgTime += (int)(DateTime.Now - ss).TotalMilliseconds;
        }

        public void ReleaseOutputBuffer(int bufferIndex)
        {
            Codec.ReleaseOutputBuffer(bufferIndex, false);
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