using System.Threading;
using Android.OS;
using CameraTest.VideoRecordEnums;
using Java.IO;
using Java.Lang;
using Object = Java.Lang.Object;
using Thread = Java.Lang.Thread;

namespace Steepshot.CameraGL.Encoder
{
    public class AudioEncoderWrapper : Object, IRunnable
    {
        public AudioEncoderConfig Config { get; private set; }
        private AudioEncoder _audioEncoder;

        private volatile AudioEncoderHandler _handler;
        private readonly object _readyFence = new object();
        private bool _ready;
        private bool _running;

        public void Configure(AudioEncoderConfig config)
        {
            if (IsRecording())
                StopRecording();

            Config = config;
        }

        public void StartRecording()
        {
            lock (_readyFence)
            {
                if (_running)
                {
                    return;
                }
                _running = true;
                new Thread(this).Start();
                while (!_ready)
                {
                    try
                    {
                        Monitor.Wait(_readyFence);
                    }
                    catch (InterruptedException)
                    {
                        // ignore
                    }
                }
            }

            _handler.SendMessage(_handler.ObtainMessage((int)EncoderMessages.StartRecording, Config));
        }

        public void StopRecording()
        {
            _handler.SendMessage(_handler.ObtainMessage((int)EncoderMessages.StopRecording));
            _handler.SendMessage(_handler.ObtainMessage((int)EncoderMessages.Quit));
        }

        public void FrameAvailable(byte[] buffer)
        {
            lock (_readyFence)
            {
                if (!_ready)
                {
                    return;
                }
            }

            var timestamp = JavaSystem.NanoTime() / 1000L;

            if (timestamp == 0)
            {
                return;
            }

            _handler.SendMessage(_handler.ObtainMessage((int)EncoderMessages.FrameAvailable, (int)(timestamp >> 32), (int)timestamp, buffer));
        }

        public bool IsRecording()
        {
            lock (_readyFence)
            {
                return _running;
            }
        }

        public void Run()
        {
            Looper.Prepare();

            lock (_readyFence)
            {
                _handler = new AudioEncoderHandler(this);
                _ready = true;
                Monitor.Pulse(_readyFence);
            }

            Looper.Loop();

            lock (_readyFence)
            {
                Monitor.Wait(_readyFence);
                _ready = _running = false;
                _handler = null;
            }
        }

        public void HandleStartRecording(AudioEncoderConfig config)
        {
            try
            {
                _audioEncoder = new AudioEncoder(config);
            }
            catch (IOException ioe)
            {
                throw new RuntimeException(ioe);
            }
        }

        public void HandleFrameAvailable(byte[] buffer, long timestampNanos)
        {
            _audioEncoder.Poll(buffer, timestampNanos);
            _audioEncoder.DrainEncoder(false);
        }

        public void HandleStopRecording()
        {
            _audioEncoder.DrainEncoder(true);
            ReleaseEncoder();
        }

        private void ReleaseEncoder()
        {
            _audioEncoder.Release();
        }
    }
}
