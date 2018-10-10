#pragma warning disable 618
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Hardware;
using Android.Media;
using Android.Opengl;
using Java.Lang;
using Steepshot.CameraGL.Audio;
using Steepshot.CameraGL.Encoder;
using Steepshot.Utils;
using Camera = Android.Hardware.Camera;
using Exception = Java.Lang.Exception;
using Object = Java.Lang.Object;

namespace Steepshot.CameraGL
{
    public class CameraManager : Object, SurfaceTexture.IOnFrameAvailableListener
    {
        public List<CameraInfo> SupportedCameras { get; }
        public CameraInfo CurrentCamera { get; private set; }
        public Camera.Parameters Parameters => _camera?.GetParameters();
        public string[] SupportFlashModes { get; set; }
        public bool RecordingEnabled { get; private set; }
        private Camera _camera;
        private CameraConfig _cameraConfig;
        private readonly CameraOrientationEventListener _cameraOrientationEventListener;
        private readonly CameraHandler _cameraHandler;
        private readonly GLSurfaceView _gLView;
        private readonly VideoEncoderWrapper _videoEncoderWrapper;
        private readonly AudioEncoderWrapper _audioEncoderWrapper;
        private readonly CameraSurfaceRenderer _renderer;
        private readonly AudioRecorderWrapper _audioRecorderWrapper;
        private bool _isGlInitialized;
        private int _cameraPreviewWidth;
        private int _cameraPreviewHeight;

        public CameraManager(Context context, GLSurfaceView gLView)
        {
            _gLView = gLView;

            _cameraOrientationEventListener = new CameraOrientationEventListener(context, SensorDelay.Normal);
            _cameraHandler = new CameraHandler(this);

            _videoEncoderWrapper = new VideoEncoderWrapper();
            _renderer = new CameraSurfaceRenderer(_cameraHandler, _videoEncoderWrapper);

            _audioEncoderWrapper = new AudioEncoderWrapper();
            _audioRecorderWrapper = new AudioRecorderWrapper();

            SupportedCameras = new List<CameraInfo>();

            for (int i = 0; i < Camera.NumberOfCameras; i++)
            {
                var info = new Camera.CameraInfo();
                Camera.GetCameraInfo(i, info);
                if (SupportedCameras.All(x => x.Info.Facing != info.Facing))
                {
                    SupportedCameras.Add(new CameraInfo
                    {
                        Index = i,
                        Info = info
                    });
                }
            }

            CurrentCamera = SupportedCameras.Find(x => x.Info.Facing == CameraFacing.Back);
            _cameraOrientationEventListener.OrientationChanged += CameraOrientationEventListenerOnOrientationChanged;
        }

        private void InitGl()
        {
            if (_isGlInitialized)
                return;

            _gLView.SetEGLContextClientVersion(2); // select GLES 2.0 
            _gLView.SetRenderer(_renderer);
            _gLView.RenderMode = Rendermode.WhenDirty;

            _isGlInitialized = true;
        }

        public void ReConfigure(CameraInfo cameraInfo, VideoEncoderConfig videoConfig, AudioEncoderConfig audioConfig)
        {
            var revertCamera = CurrentCamera != cameraInfo;
            CurrentCamera = cameraInfo;

            if (revertCamera)
            {
                OnPause();
                OnResume();
            }

            if (videoConfig != null)
            {
                _videoEncoderWrapper.Configure(videoConfig);
                _cameraConfig = CameraConfig.Photo;
            }

            if (audioConfig != null)
            {
                _audioEncoderWrapper.Configure(audioConfig);
                _audioRecorderWrapper.Configure(new AudioRecorderConfig(_audioEncoderWrapper, ChannelIn.Mono, Encoding.Pcm16bit));
                _cameraConfig = CameraConfig.Video;
            }

            var camParams = Parameters;

            if (_cameraConfig == CameraConfig.Photo)
            {
                camParams.SetRecordingHint(false);
                if (camParams.SupportedFocusModes.Contains(Camera.Parameters.FocusModeAuto))
                    camParams.FocusMode = Camera.Parameters.FocusModeAuto;
                else if (camParams.SupportedFocusModes.Contains(Camera.Parameters.FocusModeContinuousPicture))
                    camParams.FocusMode = Camera.Parameters.FocusModeContinuousPicture;
            }
            else
            {
                camParams.SetRecordingHint(true);
                if (camParams.SupportedFocusModes.Contains(Camera.Parameters.FocusModeContinuousVideo))
                    camParams.FocusMode = Camera.Parameters.FocusModeContinuousVideo;
                CameraUtils.ChooseFixedPreviewFps(camParams, 30000);
            }

            _camera.SetParameters(camParams);
        }

        public void ToggleFlashMode()
        {
            var supportedModes = _camera.GetParameters().SupportedFlashModes.Intersect(SupportFlashModes.ToList()).ToList();
            var nextModeInd = supportedModes.IndexOf(CurrentCamera.FlashMode) + 1;
            var nextMode = supportedModes[nextModeInd < supportedModes.Count ? nextModeInd : 0];
            CurrentCamera.FlashMode = nextMode;
            var camParams = Parameters;
            camParams.FlashMode = CurrentCamera.FlashMode;
            _camera.SetParameters(camParams);
        }

        private void OpenCamera()
        {
            if (_camera != null)
            {
                throw new RuntimeException("camera already initialized");
            }

            _camera = Camera.Open(CurrentCamera.Index);

            if (_camera == null)
            {
                throw new RuntimeException("Unable to open camera");
            }

            var camParams = Parameters;
            camParams.SetPreviewSize(camParams.PreferredPreviewSizeForVideo.Width,
                camParams.PreferredPreviewSizeForVideo.Height);

            var supportedFlashModes = camParams.SupportedFlashModes;

            if (supportedFlashModes.Count > 0)
            {
                if (string.IsNullOrEmpty(CurrentCamera.FlashMode))
                    CurrentCamera.FlashMode = supportedFlashModes[0];
                camParams.FlashMode = CurrentCamera.FlashMode;
            }

            _camera.SetParameters(camParams);

            _cameraPreviewWidth = camParams.PreviewSize.Width;
            _cameraPreviewHeight = camParams.PreviewSize.Height;

            _camera.SetDisplayOrientation(90);
        }

        private void Release()
        {
            if (_camera != null)
            {
                _camera.StopPreview();
                _camera.Release();
                _camera = null;
            }
        }

        public void ToggleRecording()
        {
            RecordingEnabled = !RecordingEnabled;
            _gLView.QueueEvent(() =>
            {
                _renderer.ChangeRecordingState(RecordingEnabled);
            });
            _audioRecorderWrapper.ChangeRecordingState(RecordingEnabled);
        }

        public void TakePicture(Camera.IShutterCallback shutterCallback, Camera.IPictureCallback pictureCallbackRaw, Camera.IPictureCallback pictureCallbackJpeg)
        {
            _camera?.TakePicture(shutterCallback, pictureCallbackRaw, pictureCallbackJpeg);
        }

        public void AutoFocus(Camera.IAutoFocusCallback cb)
        {
            _camera?.AutoFocus(cb);
        }

        public void CancelAutoFocus()
        {
            _camera?.CancelAutoFocus();
        }

        public void SetParameters(Camera.Parameters parameters)
        {
            _camera?.SetParameters(parameters);
        }

        public void HandleSetSurfaceTexture(SurfaceTexture st)
        {
            st.SetOnFrameAvailableListener(this);
            try
            {
                _camera.SetPreviewTexture(st);
            }
            catch (Exception ioe)
            {
                throw new RuntimeException(ioe);
            }

            _camera.StartPreview();
        }

        public void OnFrameAvailable(SurfaceTexture surfaceTexture)
        {
            _gLView.RequestRender();
        }

        private void CameraOrientationEventListenerOnOrientationChanged(int orientation)
        {
            if (_camera == null || RecordingEnabled)
                return;

            var camParams = Parameters;
            var info = new Camera.CameraInfo();
            Camera.GetCameraInfo(CurrentCamera.Index, info);

            orientation = (orientation + 45) / 90 * 90;
            var rotation = info.Facing == Camera.CameraInfo.CameraFacingFront
                ? (info.Orientation - orientation + 360) % 360
                : (info.Orientation + orientation) % 360;

            CurrentCamera.Rotation = rotation;
            camParams.SetRotation(rotation);
            _camera.SetParameters(camParams);
        }

        public void OnPause()
        {
            _cameraOrientationEventListener.Disable();
            Release();
            _gLView.QueueEvent(() =>
                _renderer?.NotifyPausing());
            _gLView.OnPause();
        }

        public void OnResume()
        {
            InitGl();
            OpenCamera();
            _gLView.QueueEvent(() =>
            {
                _renderer.SetCameraPreviewSize(_cameraPreviewWidth, _cameraPreviewHeight);
            });
            _gLView.OnResume();
            _cameraOrientationEventListener.Enable();
        }

        public void OnDestroyed()
        {
            _cameraOrientationEventListener.Disable();
            _cameraHandler.InvalidateHandler();
            Release();
        }
    }
}