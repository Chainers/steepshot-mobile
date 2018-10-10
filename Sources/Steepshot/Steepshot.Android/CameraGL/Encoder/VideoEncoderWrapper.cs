using System.Threading;
using Android.Graphics;
using Android.OS;
using Java.IO;
using Java.Lang;
using Steepshot.CameraGL.Enums;
using Steepshot.CameraGL.Gles;
using Steepshot.Utils;
using Object = Java.Lang.Object;
using Process = Android.OS.Process;
using Thread = Java.Lang.Thread;
using ThreadPriority = Android.OS.ThreadPriority;

namespace Steepshot.CameraGL.Encoder
{
    public class VideoEncoderWrapper : Object, IRunnable
    {
        public VideoEncoderConfig Config { get; private set; }

        private EglCore _eglCore;
        private VideoEncoder _videoEncoder;
        private WindowSurface _inputWindowSurface;
        private FullFrameRect _fullScreen;
        private readonly float[] _transform = new float[16];
        private int _textureId;

        private volatile VideoEncoderHandler _handler;
        private readonly object _readyFence = new object();
        private bool _ready;
        private bool _running;

        public void Configure(VideoEncoderConfig config)
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
        }

        public bool IsRecording()
        {
            lock (_readyFence)
            {
                return _running;
            }
        }

        public void UpdateSharedContext(VideoEncoderConfig config)
        {
            _handler.SendMessage(_handler.ObtainMessage((int)EncoderMessages.UpdateSharedContext, config));
        }

        public void FrameAvailable(SurfaceTexture st)
        {
            lock (_readyFence)
            {
                if (!_ready)
                {
                    return;
                }
            }

            st.GetTransformMatrix(_transform);
            var timestamp = st.Timestamp;
            if (timestamp == 0)
            {
                return;
            }

            _handler.SendMessage(_handler.ObtainMessage((int)EncoderMessages.FrameAvailable, (int)(timestamp >> 32), (int)timestamp, _transform));
        }

        public void SetTextureId(int id)
        {
            lock (_readyFence)
            {
                if (!_ready)
                {
                    return;
                }
            }

            _handler.SendMessage(_handler.ObtainMessage((int)EncoderMessages.SetTextureId, id, 0, null));
        }

        public void Run()
        {
            Process.SetThreadPriority(ThreadPriority.Video);
            Looper.Prepare();

            lock (_readyFence)
            {
                _handler = new VideoEncoderHandler(this);
                _ready = true;
                Monitor.Pulse(_readyFence);
            }

            Looper.Loop();

            lock (_readyFence)
            {
                _ready = _running = false;
                _handler = null;
            }
        }

        public void HandleStartRecording(VideoEncoderConfig config)
        {
            try
            {
                _videoEncoder = new VideoEncoder(config);
            }
            catch (IOException ioe)
            {
                throw new RuntimeException(ioe);
            }
            _eglCore = new EglCore(config.EglContext, EglCore.FlagRecordable);
            _inputWindowSurface = new WindowSurface(_eglCore, _videoEncoder.InputSurface, true);
            _inputWindowSurface.MakeCurrent();

            _fullScreen = new FullFrameRect(new Texture2DProgram
            {
                AspectRatio = Style.ScreenHeight / (float)Style.ScreenWidth
            });
        }

        public void HandleFrameAvailable(float[] transform, long timestampNanos)
        {
            _videoEncoder.DrainEncoder(false);
            _fullScreen.DrawFrame(_textureId, transform);

            _inputWindowSurface.SetPresentationTime(timestampNanos);
            _inputWindowSurface.SwapBuffers();
        }

        public void HandleStopRecording()
        {
            _videoEncoder.DrainEncoder(true);
            ReleaseEncoder();
        }

        public void HandleSetTexture(int id)
        {
            _textureId = id;
        }

        public void HandleUpdateSharedContext(VideoEncoderConfig config)
        {
            _inputWindowSurface.ReleaseEglSurface();
            _fullScreen.Release(false);
            _eglCore.Release();

            _eglCore = new EglCore(config.EglContext, EglCore.FlagRecordable);
            _inputWindowSurface.Recreate(_eglCore);
            _inputWindowSurface.MakeCurrent();

            _fullScreen = new FullFrameRect(new Texture2DProgram
            {
                AspectRatio = Style.ScreenHeight / (float)Style.ScreenWidth
            });
        }

        private void ReleaseEncoder()
        {
            _videoEncoder.Release();
            if (_inputWindowSurface != null)
            {
                _inputWindowSurface.Release();
                _inputWindowSurface = null;
            }
            if (_fullScreen != null)
            {
                _fullScreen.Release(false);
                _fullScreen = null;
            }
            if (_eglCore != null)
            {
                _eglCore.Release();
                _eglCore = null;
            }
        }
    }
}