using System.Threading;
using Android.Media;
using Android.OS;
using Java.Lang;
using Thread = Java.Lang.Thread;

namespace Steepshot.CameraGL.Encoder
{
    public abstract class BaseMediaEncoder : MuxerTrack, IRunnable
    {
        public BaseEncoderConfig Config { get; protected set; }
        public abstract MediaFormat Format { get; protected set; }
        protected int TimeoutUsec => 0;
        protected MediaCodec Codec;
        protected MuxerWrapper Muxer;

        protected Handler Handler;
        protected readonly object Lock = new object();
        protected bool Ready;
        protected bool Running;

        public virtual void Start()
        {
            InitCodec();

            lock (Lock)
            {
                if (Running)
                {
                    return;
                }

                Running = true;
                new Thread(this).Start();
                while (!Ready)
                {
                    try
                    {
                        Monitor.Wait(Lock);
                    }
                    catch (InterruptedException)
                    {
                        // ignore
                    }
                }
            }

            Handler.Post(() =>
            {
                Codec.Start();
            });
        }

        public virtual void Stop(bool save = false)
        {
            lock (Lock)
            {
                if (!Ready)
                {
                    return;
                }
            }

            Handler.Post(() =>
            {
                if (save)
                {
                    DrainEncoder(true);
                    Muxer.MuxingFinished += MuxingFinished;
                    Muxer.AddTrack(this);
                    Looper.MyLooper().Quit();
                    return;
                }

                Release();
                Looper.MyLooper().Quit();
            });
        }

        protected void MuxingFinished(string path)
        {
            Muxer.MuxingFinished -= MuxingFinished;
            Release();
        }

        public void FrameAvailable()
        {
            Handler.Post(() =>
            {
                DrainEncoder(false);
            });
        }

        public void Run()
        {
            Looper.Prepare();
            Handler = new Handler();
            lock (Lock)
            {
                Ready = true;
                Monitor.Pulse(Lock);
            }

            Looper.Loop();

            lock (Lock)
            {
                Ready = Running = false;
                Handler = null;
            }
        }

        private bool IsRunning()
        {
            lock (Lock)
            {
                return Running;
            }
        }

        public bool ChangeRecordingState(bool saveIfFinish)
        {
            if (IsRunning())
            {
                Stop(saveIfFinish);
                return false;
            }

            Start();
            return true;
        }

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
                    OutputFormat = Codec.OutputFormat;
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
                        encodedData.Position(bufferInfo.Offset);
                        encodedData.Limit(bufferInfo.Offset + bufferInfo.Size);
                        CircularBuffer.Add(encodedData, (int)bufferInfo.Flags, bufferInfo.PresentationTimeUs);
                    }

                    Codec.ReleaseOutputBuffer(encoderStatus, false);

                    if ((bufferInfo.Flags & MediaCodecBufferFlags.EndOfStream) != 0)
                    {
                        break;
                    }
                }
            }
        }

        protected abstract void InitCodec();

        public virtual void Configure(BaseEncoderConfig config)
        {
            lock (Lock)
            {
                if (Running)
                {
                    Stop();
                }
            }

            Config = config;
        }

        protected virtual void Release()
        {
            if (Codec != null)
            {
                Codec.Stop();
                Codec.Release();
                Codec = null;
            }

            if (CircularBuffer != null)
            {
                CircularBuffer.Release();
                CircularBuffer = null;
            }
        }
    }
}