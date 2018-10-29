#pragma warning disable 618
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Hardware;
using Android.Media;
using Android.Opengl;
using Android.Views;
using Java.IO;
using Java.Lang;
using Steepshot.CameraGL.Audio;
using Steepshot.CameraGL.Encoder;
using Steepshot.CameraGL.Gles;
using Steepshot.Utils;
using Camera = Android.Hardware.Camera;
using Object = Java.Lang.Object;
using VideoEncoder = Steepshot.CameraGL.Encoder.VideoEncoder;

namespace Steepshot.CameraGL
{
    public class CameraManager : Object, SurfaceTexture.IOnFrameAvailableListener, ISurfaceHolderCallback
    {
        public List<CameraInfo> SupportedCameras { get; }
        public CameraInfo CurrentCamera { get; private set; }
        public Camera.Parameters Parameters => _camera?.GetParameters();
        public string[] SupportFlashModes { get; set; }
        private Camera _camera;
        private CameraConfig _cameraConfig;
        private readonly CameraOrientationEventListener _cameraOrientationEventListener;
        private readonly SurfaceView _surface;

        public bool RecordingEnabled { get; private set; }

        private EglCore _eglCore;
        private WindowSurface _displaySurface;
        private SurfaceTexture _cameraTexture;
        private Texture2DProgram _texture2DProgram;
        private FullFrameRect _fullFrame;
        private WindowSurface _encoderSurface;
        private bool _encoderHasSurface;
        private readonly float[] _tmpMatrix = new float[16];
        private int _textureId;
        private int _previewWidth;
        private int _previewHeight;
        private int _encoderWidth;
        private int _encoderHeight;
        private int _orientation;

        private readonly VideoEncoder _videoEncoder;
        private readonly AudioRecorderWrapper _audioRecorder;

        public CameraManager(Context context, SurfaceView surface)
        {
            _surface = surface;

            _cameraOrientationEventListener = new CameraOrientationEventListener(context, SensorDelay.Normal);

            _videoEncoder = new VideoEncoder();
            _audioRecorder = new AudioRecorderWrapper();

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

            _surface.Holder.AddCallback(this);
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
                if (_videoEncoder.Config != videoConfig)
                {
                    _videoEncoder.Configure(videoConfig);
                    _encoderWidth = videoConfig.Width;
                    _encoderHeight = videoConfig.Height;
                }

                _cameraConfig = CameraConfig.Photo;
            }

            if (audioConfig != null)
            {
                _audioRecorder.Configure(new AudioRecorderConfig(audioConfig, Encoding.Pcm16bit));
                _cameraConfig = CameraConfig.Video;
            }

            var camParams = Parameters;

            if (_cameraConfig == CameraConfig.Photo)
            {
                camParams.SetRecordingHint(false);
                if (camParams.SupportedFocusModes.Contains(Camera.Parameters.FocusModeContinuousPicture))
                    camParams.FocusMode = Camera.Parameters.FocusModeContinuousPicture;
                else if (camParams.SupportedFocusModes.Contains(Camera.Parameters.FocusModeAuto))
                    camParams.FocusMode = Camera.Parameters.FocusModeAuto;
            }
            else
            {
                camParams.SetRecordingHint(true);
                if (camParams.SupportedFocusModes.Contains(Camera.Parameters.FocusModeContinuousVideo))
                    camParams.FocusMode = Camera.Parameters.FocusModeContinuousVideo;
                else if (camParams.SupportedFocusModes.Contains(Camera.Parameters.FocusModeAuto))
                    camParams.FocusMode = Camera.Parameters.FocusModeAuto;
                CameraUtils.ChooseFixedPreviewFps(camParams, 30000);
            }

            _camera.SetParameters(camParams);
        }

        public void ToggleFlashMode()
        {
            var supportedModes = Parameters.SupportedFlashModes?.Intersect(SupportFlashModes.ToList()).ToList();
            if (supportedModes == null)
                return;

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
            CameraUtils.ChoosePreviewSize(camParams, 1280, 720);
            _previewWidth = camParams.PreviewSize.Width;
            _previewHeight = camParams.PreviewSize.Height;

            var supportedFlashModes = camParams.SupportedFlashModes;

            if (supportedFlashModes?.Count > 0)
            {
                if (string.IsNullOrEmpty(CurrentCamera.FlashMode))
                    CurrentCamera.FlashMode = supportedFlashModes[0];
                camParams.FlashMode = CurrentCamera.FlashMode;
            }

            _camera.SetParameters(camParams);

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

        public void ToggleRecording(bool saveIfFinish = false)
        {
            RecordingEnabled = _videoEncoder.ChangeRecordingState(saveIfFinish);
            if (RecordingEnabled)
            {
                _encoderSurface = new WindowSurface(_eglCore, _videoEncoder.InputSurface, true);
                _encoderHasSurface = true;
            }
            else
            {
                _encoderSurface?.ReleaseEglSurface();
                _encoderSurface?.Release();
                _encoderSurface = null;
                _encoderHasSurface = false;
            }
            _audioRecorder.ChangeRecordingState(RecordingEnabled);
        }

        public void TakePicture(Camera.IShutterCallback shutterCallback, Camera.IPictureCallback pictureCallbackRaw, Camera.IPictureCallback pictureCallbackJpeg)
        {
            SetOrientation();
            //var data = _displaySurface.GetFrame();
            //pictureCallbackJpeg?.OnPictureTaken(data, _camera);
            _camera?.TakePicture(shutterCallback, pictureCallbackRaw, pictureCallbackJpeg);
        }

        private void SetOrientation()
        {
            var camParams = Parameters;
            var info = new Camera.CameraInfo();
            Camera.GetCameraInfo(CurrentCamera.Index, info);

            var orientation = (_orientation + 45) / 90 * 90;
            var rotation = info.Facing == Camera.CameraInfo.CameraFacingFront
                ? (info.Orientation - orientation + 360) % 360
                : (info.Orientation + orientation) % 360;

            CurrentCamera.Rotation = rotation;
            camParams.SetRotation(rotation);
            _camera.SetParameters(camParams);
        }

        public void CancelAutoFocus()
        {
            _camera?.CancelAutoFocus();
        }

        public void SetParameters(Camera.Parameters parameters)
        {
            _camera?.SetParameters(parameters);
        }

        public void OnFrameAvailable(SurfaceTexture surfaceTexture)
        {
            if (_eglCore == null)
                return;

            _displaySurface.MakeCurrent();
            _cameraTexture.UpdateTexImage();
            var timestamp = JavaSystem.NanoTime();
            _cameraTexture.GetTransformMatrix(_tmpMatrix);

            GLES20.GlViewport(0, 0, _surface.Width, _surface.Height);
            _texture2DProgram.AspectRatio = 1;
            _fullFrame.DrawFrame(_textureId, _tmpMatrix);
            _displaySurface.SwapBuffers();

            if (_encoderHasSurface)
            {
                _encoderSurface.MakeCurrent();
                GLES20.GlViewport(0, 0, _encoderWidth, _encoderHeight);
                _texture2DProgram.AspectRatio = _previewWidth / (float)_previewHeight;
                _fullFrame.DrawFrame(_textureId, _tmpMatrix);
                _videoEncoder.FrameAvailable();
                _encoderSurface.SetPresentationTime(timestamp);
                _encoderSurface.SwapBuffers();
            }
        }

        private void CameraOrientationEventListenerOnOrientationChanged(int orientation)
        {
            _orientation = orientation;
        }

        public void OnPause()
        {
            _cameraOrientationEventListener.Disable();
            Release();

            _videoEncoder?.Stop(true);

            if (_cameraTexture != null)
            {
                _cameraTexture.Release();
                _cameraTexture = null;
            }
            if (_displaySurface != null)
            {
                _displaySurface.Release();
                _displaySurface = null;
            }
            if (_fullFrame != null)
            {
                _fullFrame.Release(false);
                _fullFrame = null;
            }
            if (_eglCore != null)
            {
                _eglCore.Release();
                _eglCore = null;
            }
        }

        public void OnResume()
        {
            OpenCamera();

            if (_eglCore != null)
                StartPreview();
            else if (_surface.Holder.Surface.IsValid)
                SurfaceCreated(_surface.Holder);

            _cameraOrientationEventListener.Enable();
        }

        public void SurfaceChanged(ISurfaceHolder holder, Format format, int width, int height)
        {
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            _eglCore = new EglCore(null, EglCore.FlagRecordable);
            _displaySurface = new WindowSurface(_eglCore, holder.Surface, false);
            _displaySurface.MakeCurrent();

            _texture2DProgram = new Texture2DProgram();
            _fullFrame = new FullFrameRect(_texture2DProgram);
            _textureId = _fullFrame.CreateTextureObject();
            _cameraTexture = new SurfaceTexture(_textureId);
            _cameraTexture.SetOnFrameAvailableListener(this);

            StartPreview();
        }

        private void StartPreview()
        {
            if (_camera != null)
            {
                try
                {
                    _camera.SetPreviewTexture(_cameraTexture);
                }
                catch (IOException ioe)
                {
                    throw new RuntimeException(ioe);
                }

                _camera.StartPreview();
            }
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
        }
    }
}