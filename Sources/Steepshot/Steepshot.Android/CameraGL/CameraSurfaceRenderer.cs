using Android.Graphics;
using Android.Opengl;
using Android.Util;
using Java.Lang;
using Javax.Microedition.Khronos.Opengles;
using Steepshot.CameraGL.Encoder;
using Steepshot.CameraGL.Enums;
using Steepshot.CameraGL.Gles;
using EGLConfig = Javax.Microedition.Khronos.Egl.EGLConfig;

namespace Steepshot.CameraGL
{
    public class CameraSurfaceRenderer : Object, GLSurfaceView.IRenderer
    {
        private const string Tag = "";

        private readonly CameraHandler _cameraHandler;
        private readonly VideoEncoderWrapper _videoEncoderWrapper;

        private FullFrameRect _fullScreen;

        private readonly float[] _sTMatrix = new float[16];
        private int _textureId;

        private SurfaceTexture _surfaceTexture;
        private bool _recordingEnabled;
        private RecordingStatus _recordingStatus;

        // width/height of the incoming camera preview frames
        private bool _incomingSizeUpdated;
        private int _incomingWidth;
        private int _incomingHeight;

        public CameraSurfaceRenderer(CameraHandler cameraHandler, VideoEncoderWrapper movieEncoderWrapper)
        {
            _cameraHandler = cameraHandler;
            _videoEncoderWrapper = movieEncoderWrapper;

            _textureId = -1;

            _recordingStatus = RecordingStatus.None;
            _recordingEnabled = false;

            _incomingSizeUpdated = false;
            _incomingWidth = _incomingHeight = -1;
        }

        public void NotifyPausing()
        {
            if (_surfaceTexture != null)
            {
                Log.Debug(Tag, "renderer pausing -- releasing SurfaceTexture");
                _surfaceTexture.Release();
                _surfaceTexture = null;
            }
            if (_fullScreen != null)
            {
                _fullScreen.Release(false);     // assume the GLSurfaceView EGL context is about
                _fullScreen = null;             //  to be destroyed
            }
            _incomingWidth = _incomingHeight = -1;
        }

        public void ChangeRecordingState(bool isRecording)
        {
            _recordingEnabled = isRecording;
        }

        public void SetCameraPreviewSize(int width, int height)
        {
            Log.Debug(Tag, "setCameraPreviewSize");
            _incomingWidth = width;
            _incomingHeight = height;
            _incomingSizeUpdated = true;
        }

        public void OnDrawFrame(IGL10 gl)
        {
            _surfaceTexture.UpdateTexImage();

            if (_recordingEnabled)
            {
                switch (_recordingStatus)
                {
                    case RecordingStatus.Off:
                        _videoEncoderWrapper.Config.EglContext = EGL14.EglGetCurrentContext();
                        _videoEncoderWrapper.StartRecording();
                        _recordingStatus = RecordingStatus.On;
                        break;
                    case RecordingStatus.Resumed:
                        _videoEncoderWrapper.Config.EglContext = EGL14.EglGetCurrentContext();
                        _videoEncoderWrapper.UpdateSharedContext(_videoEncoderWrapper.Config);
                        _recordingStatus = RecordingStatus.On;
                        break;
                    case RecordingStatus.On:
                        // yay
                        break;
                    default:
                        throw new RuntimeException("unknown status " + _recordingStatus);
                }

                _videoEncoderWrapper.SetTextureId(_textureId);
                _videoEncoderWrapper.FrameAvailable(_surfaceTexture);
            }
            else
            {
                switch (_recordingStatus)
                {
                    case RecordingStatus.On:
                    case RecordingStatus.Resumed:
                        Log.Debug(Tag, "STOP recording");
                        _videoEncoderWrapper.StopRecording();
                        _recordingStatus = RecordingStatus.Off;
                        break;
                    case RecordingStatus.Off:
                        // yay
                        break;
                    default:
                        throw new RuntimeException("unknown status " + _recordingStatus);
                }
            }

            if (_incomingWidth <= 0 || _incomingHeight <= 0)
            {
                Log.Info(Tag, "Drawing before incoming texture size set; skipping");
                return;
            }

            if (_incomingSizeUpdated)
            {
                _fullScreen.GetProgram().SetTexSize(_incomingWidth, _incomingHeight);
                _incomingSizeUpdated = false;
            }

            _surfaceTexture.GetTransformMatrix(_sTMatrix);
            _fullScreen.DrawFrame(_textureId, _sTMatrix);
        }

        public void OnSurfaceChanged(IGL10 gl, int width, int height)
        {
        }

        public void OnSurfaceCreated(IGL10 gl, EGLConfig config)
        {
            _recordingEnabled = _videoEncoderWrapper.IsRecording();
            _recordingStatus = _recordingEnabled ? RecordingStatus.Resumed : RecordingStatus.Off;

            _fullScreen = new FullFrameRect(new Texture2DProgram());
            _textureId = _fullScreen.CreateTextureObject();
            _surfaceTexture = new SurfaceTexture(_textureId);

            _cameraHandler.SendMessage(_cameraHandler.ObtainMessage(CameraHandler.MsgSetSurfaceTexture, _surfaceTexture));
        }
    }
}