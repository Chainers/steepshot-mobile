using System;
using System.Collections.Generic;
using AVFoundation;
using CoreGraphics;
using CoreMedia;
using Foundation;
using Photos;
using Steepshot.Core.Exceptions;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using UIKit;
using CoreAnimation;
using System.IO;
using Steepshot.Core.Localization;
using System.Drawing;
using System.Linq;
using Steepshot.Core.Models.Enums;
using System.Threading.Tasks;

namespace Steepshot.iOS.Views
{
    public partial class PhotoViewController : BaseViewController, IAVCapturePhotoCaptureDelegate, IAVCaptureFileOutputRecordingDelegate
    {
        enum Theme
        {
            Light,
            Dark
        }

        private const int maxLineWidth = 20;
        private const int maxProgressRadius = 44;

        private readonly UITapGestureRecognizer _galleryTap;
        private readonly UITapGestureRecognizer _photoTabTap;
        private readonly UITapGestureRecognizer _videoTabTap;
        private readonly UITapGestureRecognizer _enableCameraTap;

        private readonly AVCaptureSession _captureSession = new AVCaptureSession();
        private AVCaptureDevice _backCamera;
        private AVCaptureDevice _frontCamera;
        private AVCaptureDevice _currentCamera;
        private AVCaptureDeviceInput _audioInput;
        private AVCaptureDeviceInput _captureDeviceInput;
        private AVCapturePhotoOutput _capturePhotoOutput;
        private AVCaptureVideoPreviewLayer _videoPreviewLayer;
        private AVCaptureFlashMode _flashMode = AVCaptureFlashMode.Auto;
        private AVCaptureMovieFileOutput _videoFileOutput;
        private UIDeviceOrientation currentOrientation;
        private UIDeviceOrientation orientationOnPhoto;
        private NSObject _orientationChangeEventToken;
        private AVAuthorizationStatus _authorizationAudioStatus;
        private AVAssetExportSession exportSession;

        private UIView _liveCameraStream;
        private UIView _bottomPanel;
        private UIView _pointerView;
        private UILabel _enableCameraAccess;
        private UIButton _closeButton;
        private UIButton _flashButton;
        private UIButton _photoTabButton;
        private UIButton _videoTabButton;
        private UIButton _swapCameraButton;
        private UIButton _photoButton;
        private UIImageView _galleryButton;
        private MediaType _currentMode = MediaType.Photo;
        private NSLayoutConstraint _photoConstraint;
        private NSLayoutConstraint _videoConstraint;

        private CAShapeLayer _sl;
        private CABasicAnimation _animation;
        private UIBezierPath _bezierPath;
        private NSUrl _exportLocation;
        private bool _initialized;
        private bool _isRecording;
        private bool _isCancelled;
        private bool _successfulRecord;
        private float _activePanelHeight;

        private Task _switchCameraTask;
        private Task _photoCameraSwitchTask;
        private Task _videoCameraSwitchTask;

        UIActivityIndicatorView _videoLoader = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.White);

        public PhotoViewController()
        {
            _galleryTap = new UITapGestureRecognizer(GalleryTap);
            _photoTabTap = new UITapGestureRecognizer(SwitchToPhotoMode);
            _videoTabTap = new UITapGestureRecognizer(SwitchToVideoMode);
            _enableCameraTap = new UITapGestureRecognizer(EnableCameraAccess);
            _captureSession = new AVCaptureSession();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            SetupCameraView();
            SwitchTheme(Theme.Dark);

            _videoPreviewLayer = new AVCaptureVideoPreviewLayer(_captureSession)
            {
                VideoGravity = AVLayerVideoGravity.ResizeAspectFill
            };

            _liveCameraStream.Layer.AddSublayer(_videoPreviewLayer);
        }

        public override void ViewWillAppear(bool animated)
        {
            NavigationController.SetNavigationBarHidden(true, false);
            CheckDeviceOrientation(null);
            SetGalleryButton();
            ToggleButtons(true);

            _galleryButton.AddGestureRecognizer(_galleryTap);
            _photoTabButton.AddGestureRecognizer(_photoTabTap);
            _videoTabButton.AddGestureRecognizer(_videoTabTap);
            _enableCameraAccess.AddGestureRecognizer(_enableCameraTap);

            _closeButton.TouchDown += GoBack;
            _flashButton.TouchDown += OnFlashTouch;
            _photoButton.TouchDown += CaptureContent;
            _photoButton.TouchUpInside += OnPhotoButtonUp;
            _photoButton.TouchUpOutside += OnPhotoButtonUp;
            _swapCameraButton.TouchDown += SwitchCameraButtonTapped;
            ((InteractivePopNavigationController)NavigationController).DidEnterBackgroundEvent += DidEnterBackground;
            _orientationChangeEventToken = NSNotificationCenter.DefaultCenter.AddObserver(UIDevice.OrientationDidChangeNotification, CheckDeviceOrientation);
        }

        public override void ViewDidAppear(bool animated)
        {
            AuthorizeCameraUse();
        }

        public override void ViewDidLayoutSubviews()
        {
            if (!_initialized)
            {
                var lineWidth = (_activePanelHeight / 2 - 39);
                var radius = lineWidth / 2 + 33;

                if (radius > maxProgressRadius)
                {
                    radius = maxProgressRadius;
                    lineWidth = maxLineWidth;
                }

                _bezierPath = new UIBezierPath();
                _bezierPath.AddArc(_photoButton.Center, radius, 3f * (float)Math.PI / 2f, 4.712327f, true);

                _sl = new CAShapeLayer();
                _sl.LineWidth = lineWidth;
                _sl.StrokeColor = UIColor.FromRGB(255, 17, 0).CGColor;
                _sl.FillColor = UIColor.Clear.CGColor;
                _sl.LineCap = CAShapeLayer.CapButt;
                _sl.LineJoin = CAShapeLayer.CapButt;
                _sl.StrokeStart = 0.0f;
                _sl.StrokeEnd = 0.0f;
                _sl.Hidden = true;
                _sl.Path = _bezierPath.CGPath;

                View.Layer.AddSublayer(_sl);

                _initialized = true;
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            if (_captureSession != null && _captureSession.Running)
                _captureSession.StopRunning();

            NavigationController.SetNavigationBarHidden(false, false);

            _galleryButton.RemoveGestureRecognizer(_galleryTap);
            _photoTabButton.RemoveGestureRecognizer(_photoTabTap);
            _videoTabButton.RemoveGestureRecognizer(_videoTabTap);
            _enableCameraAccess.RemoveGestureRecognizer(_enableCameraTap);

            _closeButton.TouchDown -= GoBack;
            _flashButton.TouchDown -= OnFlashTouch;
            _photoButton.TouchDown -= CaptureContent;
            _photoButton.TouchUpInside -= OnPhotoButtonUp;
            _photoButton.TouchUpOutside -= OnPhotoButtonUp;
            _swapCameraButton.TouchDown -= SwitchCameraButtonTapped;
            ((InteractivePopNavigationController)NavigationController).DidEnterBackgroundEvent -= DidEnterBackground;

            if (_orientationChangeEventToken != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_orientationChangeEventToken);
                _orientationChangeEventToken.Dispose();
            }

            base.ViewWillDisappear(animated);
        }

        private void SwitchToPhotoMode()
        {
            SetupPhotoCameraStream();
            SwitchMode(MediaType.Photo);
            SwitchTheme(Theme.Dark);
        }

        private void SwitchToVideoMode()
        {
            SetupVideoCameraStream();
            SwitchMode(MediaType.Video);
            SwitchTheme(Theme.Light);
        }

        private void CaptureContent(object sender, EventArgs e)
        {
            switch (_currentMode)
            {
                case MediaType.Photo:
                    ToggleButtons(false);
                    var _settingKeys = new object[] { AVVideo.CodecKey, AVVideo.CompressionPropertiesKey };
                    var _settingObjects = new object[] { new NSString("jpeg"), new NSDictionary(AVVideo.QualityKey, 1) };
                    var settingsDictionary = NSDictionary<NSString, NSObject>.FromObjectsAndKeys(_settingObjects, _settingKeys);
                    var _photoSettings = AVCapturePhotoSettings.FromFormat(settingsDictionary);
                    if (_capturePhotoOutput.SupportedFlashModes.Length > 0 && _captureDeviceInput.Device.Position == AVCaptureDevicePosition.Back)
                        _photoSettings.FlashMode = _flashMode;
                    orientationOnPhoto = currentOrientation;
                    _capturePhotoOutput.CapturePhoto(_photoSettings, this);
                    break;
                case MediaType.Video:
                    if (!_isRecording)
                    {
                        StartAnimation();
                        ToggleButtons(false);

                        var outputFileName = new NSUuid().AsString();
                        var outputFilePath = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(outputFileName, "mov"));

                        _videoFileOutput?.StartRecordingToOutputFile(NSUrl.FromFilename(outputFilePath), this);
                        _isRecording = !_isRecording;
                    }
                    break;
            }
        }

        private void OnPhotoButtonUp(object sender, EventArgs e)
        {
            StopCapturing();
        }

        private void StopCapturing(bool withCancel = false)
        {
            if (_currentMode == MediaType.Video && _isRecording)
            {
                _isCancelled = withCancel;
                _sl.RemoveAllAnimations();
                _sl.Hidden = true;
                _videoFileOutput?.StopRecording();
                _isRecording = !_isRecording;
            }
        }

        private void EnableCameraAccess()
        {
            UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString), new NSDictionary(), null);
        }

        private void GalleryTap()
        {
            if (PHPhotoLibrary.AuthorizationStatus == PHAuthorizationStatus.Authorized)
            {
                var descriptionViewController = new PhotoPreviewViewController();
                NavigationController.PushViewController(descriptionViewController, true);
            }
            else
                UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString), new NSDictionary(), null);
        }

        public override bool PrefersStatusBarHidden()
        {
            return true;
        }

        private void CheckDeviceOrientation(NSNotification notification)
        {
            if (_captureDeviceInput?.Device?.Position == AVCaptureDevicePosition.Front)
            {
                switch (UIDevice.CurrentDevice.Orientation)
                {
                    case UIDeviceOrientation.LandscapeLeft:
                        currentOrientation = UIDeviceOrientation.LandscapeRight;
                        break;
                    case UIDeviceOrientation.LandscapeRight:
                        currentOrientation = UIDeviceOrientation.LandscapeLeft;
                        break;
                    default:
                        currentOrientation = UIDevice.CurrentDevice.Orientation;
                        break;
                }
            }
            else
                currentOrientation = UIDevice.CurrentDevice.Orientation;
        }

        private void OnFlashTouch(object sender, EventArgs e)
        {
            switch (_flashMode)
            {
                case AVCaptureFlashMode.Auto:
                    _flashMode = AVCaptureFlashMode.On;
                    _flashButton.SetImage(UIImage.FromBundle("ic_flashOn"), UIControlState.Normal);
                    break;
                case AVCaptureFlashMode.On:
                    _flashMode = AVCaptureFlashMode.Off;
                    _flashButton.SetImage(UIImage.FromBundle("ic_flashOff"), UIControlState.Normal);
                    break;
                default:
                    _flashMode = AVCaptureFlashMode.Auto;
                    _flashButton.SetImage(UIImage.FromBundle("ic_flash"), UIControlState.Normal);
                    break;
            }
        }

        [Export("captureOutput:didFinishProcessingPhotoSampleBuffer:previewPhotoSampleBuffer:resolvedSettings:bracketSettings:error:")]
        public void DidFinishProcessingPhoto(AVCapturePhotoOutput captureOutput, CMSampleBuffer photoSampleBuffer, CMSampleBuffer previewPhotoSampleBuffer, AVCaptureResolvedPhotoSettings resolvedSettings, AVCaptureBracketedStillImageSettings bracketSettings, NSError error)
        {
            try
            {
                var jpegData = AVCapturePhotoOutput.GetJpegPhotoDataRepresentation(photoSampleBuffer, previewPhotoSampleBuffer);
                var photo = UIImage.LoadFromData(jpegData);

                var inSampleSize = ImageHelper.CalculateInSampleSize(photo.Size, Core.Constants.PhotoMaxSize, Core.Constants.PhotoMaxSize);
                var deviceRatio = Math.Round(UIScreen.MainScreen.Bounds.Width / UIScreen.MainScreen.Bounds.Height, 2);

                var x = ((float)inSampleSize.Width - Core.Constants.PhotoMaxSize * (float)deviceRatio) / 2f;
                photo = ImageHelper.CropImage(photo, x, 0, Core.Constants.PhotoMaxSize * (float)deviceRatio, Core.Constants.PhotoMaxSize, inSampleSize);
                SendPhotoToDescription(photo, orientationOnPhoto);
            }
            catch (Exception ex)
            {
                ShowAlert(new InternalException(LocalizationKeys.PhotoProcessingError, ex));
            }
        }

        private void AuthorizeCameraUse()
        {
            var authorizationVideoStatus = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video);
            var authorizationAudioStatus = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Audio);

            if (authorizationVideoStatus != AVAuthorizationStatus.Authorized)
            {
                AVCaptureDevice.RequestAccessForMediaTypeAsync(AVMediaType.Video).ContinueWith((arg) =>
                {
                    if (!arg.Result)
                    {
                        _enableCameraAccess.Hidden = false;
                        _photoButton.Hidden = true;
                        _flashButton.Hidden = true;
                        _swapCameraButton.Hidden = true;
                    }
                    else
                    {
                        if (authorizationAudioStatus != AVAuthorizationStatus.Authorized)
                            AVCaptureDevice.RequestAccessForMediaTypeAsync(AVMediaType.Audio);
                        InitializeCamera();
                    }
                });
            }
            else
                InitializeCamera();
        }

        private void InitializeCamera()
        {
            ConnectCamera();
            if (_currentMode.Equals(MediaType.Photo))
                SetupPhotoCameraStream();
            else
                SetupVideoCameraStream();
        }

        private void SetupPhotoCameraStream()
        {
            if (_photoCameraSwitchTask != null && !_photoCameraSwitchTask.IsCompleted)
                return;

            _photoCameraSwitchTask = Task.Run(() =>
            {
                InvokeOnMainThread(() =>
                {
                    _videoPreviewLayer.Frame = _liveCameraStream.Frame;
                });

                _captureSession.BeginConfiguration();
                _captureSession.SessionPreset = AVCaptureSession.PresetPhoto;

                if (_captureDeviceInput == null)
                    _captureDeviceInput = AVCaptureDeviceInput.FromDevice(_currentCamera);

                if (_captureSession.CanAddInput(_captureDeviceInput))
                    _captureSession.AddInput(_captureDeviceInput);

                if (_capturePhotoOutput == null)
                {
                    _capturePhotoOutput = new AVCapturePhotoOutput();
                    _capturePhotoOutput.IsHighResolutionCaptureEnabled = true;
                    _capturePhotoOutput.IsLivePhotoCaptureEnabled = false;
                }

                if (_captureSession.CanAddOutput(_capturePhotoOutput))
                    _captureSession.AddOutput(_capturePhotoOutput);

                _captureSession.CommitConfiguration();
                if (!_captureSession.Running)
                    _captureSession.StartRunning();
            });
        }

        private void SetupVideoCameraStream()
        {
            if (_videoCameraSwitchTask != null && !_videoCameraSwitchTask.IsCompleted)
                return;

            _videoCameraSwitchTask = Task.Run(() =>
            {
                InvokeOnMainThread(() =>
                {
                    _videoPreviewLayer.Frame = new CGRect(new CGPoint(0, DeviceHelper.IsXDevice ? 124 : 80), new CGSize(_liveCameraStream.Frame.Width, _liveCameraStream.Frame.Height));
                });

                _captureSession.BeginConfiguration();
                _captureSession.SessionPreset = AVCaptureSession.Preset1280x720;
                _captureSession.RemoveOutput(_capturePhotoOutput);

                _authorizationAudioStatus = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Audio);
                if (_authorizationAudioStatus == AVAuthorizationStatus.Authorized)
                {
                    if (_audioInput == null)
                    {
                        var audioInputDevice = AVCaptureDevice.GetDefaultDevice(AVMediaType.Audio);
                        _audioInput = AVCaptureDeviceInput.FromDevice(audioInputDevice);
                    }

                    if (_captureSession.CanAddInput(_audioInput))
                        _captureSession.AddInput(_audioInput);
                }
                else
                    _captureSession.UsesApplicationAudioSession = false;

                if (_videoFileOutput == null)
                {
                    _videoFileOutput = new AVCaptureMovieFileOutput();
                    var maxDuration = CMTime.FromSeconds(20, 30);
                    _videoFileOutput.MaxRecordedDuration = maxDuration;
                }

                if (_captureSession.CanAddOutput(_videoFileOutput))
                    _captureSession.AddOutput(_videoFileOutput);

                _captureSession.CommitConfiguration();
                if (!_captureSession.Running)
                    _captureSession.StartRunning();
            });
        }

        private void ConnectCamera()
        {
            var deviceDiscoverySession = AVCaptureDeviceDiscoverySession.Create(new AVCaptureDeviceType[] { AVCaptureDeviceType.BuiltInWideAngleCamera }, AVMediaType.Video, AVCaptureDevicePosition.Unspecified);
            var devices = deviceDiscoverySession.Devices;
            foreach (var device in devices)
            {
                if (device.Position == AVCaptureDevicePosition.Back)
                {
                    _backCamera = device;
                    ConfigureCameraForDevice(_backCamera);
                }
                else if (device.Position == AVCaptureDevicePosition.Front)
                {
                    _frontCamera = device;
                    ConfigureCameraForDevice(_frontCamera);
                }
            }
            _currentCamera = _backCamera;
        }

        private void ConfigureCameraForDevice(AVCaptureDevice device)
        {
            NSError error;
            if (device.IsFocusModeSupported(AVCaptureFocusMode.ContinuousAutoFocus))
            {
                device.LockForConfiguration(out error);
                device.FocusMode = AVCaptureFocusMode.ContinuousAutoFocus;
                device.UnlockForConfiguration();
            }
            if (device.IsExposureModeSupported(AVCaptureExposureMode.ContinuousAutoExposure))
            {
                device.LockForConfiguration(out error);
                device.ExposureMode = AVCaptureExposureMode.ContinuousAutoExposure;
                device.UnlockForConfiguration();
            }
            if (device.IsWhiteBalanceModeSupported(AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance))
            {
                device.LockForConfiguration(out error);
                device.WhiteBalanceMode = AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance;
                device.UnlockForConfiguration();
            }
        }

        private void SwitchCameraButtonTapped(object sender, EventArgs e)
        {
            if (_switchCameraTask != null && !_switchCameraTask.IsCompleted)
                return;

            _switchCameraTask = Task.Run(() =>
            {
                var devicePosition = _captureDeviceInput.Device.Position;
                if (devicePosition == AVCaptureDevicePosition.Front)
                    devicePosition = AVCaptureDevicePosition.Back;
                else
                    devicePosition = AVCaptureDevicePosition.Front;

                var device = devicePosition == _backCamera.Position ? _backCamera : _frontCamera;

                if (_currentMode == MediaType.Photo)
                    ConfigureCameraForDevice(device);

                _captureSession.BeginConfiguration();
                _captureSession.RemoveInput(_captureDeviceInput);
                _captureDeviceInput = AVCaptureDeviceInput.FromDevice(device);
                _captureSession.AddInput(_captureDeviceInput);
                _captureSession.CommitConfiguration();
                CheckDeviceOrientation(null);
            });
        }

        private void SendPhotoToDescription(UIImage image, UIDeviceOrientation orientation)
        {
            var descriptionViewController = new DescriptionViewController(new List<Tuple<NSDictionary, UIImage>>() { new Tuple<NSDictionary, UIImage>(null, image) }, "jpg", orientation);
            NavigationController.PushViewController(descriptionViewController, true);
        }

        private void SendVideoToDescription()
        {
            var descriptionViewController = new DescriptionViewController(_exportLocation);
            InvokeOnMainThread(() =>
            {
                StopLoading();
                NavigationController.PushViewController(descriptionViewController, true);
            });
        }

        private void StopLoading()
        {
            ToggleButtons(true);
            _videoLoader.StopAnimating();
        }

        private void DidEnterBackground()
        {
            StopCapturing(true);
        }

        public void FinishedRecording(AVCaptureFileOutput captureOutput, NSUrl outputFileUrl, NSObject[] connections, NSError error)
        {
            _videoLoader.StartAnimating();

            _sl.RemoveAllAnimations();
            _sl.Hidden = true;

            _isRecording = false;

            if (_isCancelled)
            {
                _isCancelled = false;
                CleanupLocation(outputFileUrl);
                StopLoading();
                return;
            }

            _successfulRecord = true;
            if (error != null)
            {
                if (error.LocalizedFailureReason == null)
                    _successfulRecord = ((NSNumber)error.UserInfo[AVErrorKeys.RecordingSuccessfullyFinished]).BoolValue;
            }

            var composition = AVMutableComposition.Create();
            var compositionTrackVideo = composition.AddMutableTrack(AVMediaType.Video, 0);
            var videoCompositionInstructions = new AVVideoCompositionInstruction[1];

            var asset = new AVUrlAsset(outputFileUrl, new AVUrlAssetOptions());

            if (asset.Duration.Seconds < Core.Constants.VideoMinDuration)
            {
                StopLoading();
                CleanupLocation(outputFileUrl);
                return;
            }

            var videoTrack = asset.TracksWithMediaType(AVMediaType.Video).First();
            var renderSize = new SizeF((float)videoTrack.NaturalSize.Height, (float)videoTrack.NaturalSize.Height);
            var assetTimeRange = new CMTimeRange { Start = CMTime.Zero, Duration = asset.Duration };

            compositionTrackVideo.InsertTimeRange(assetTimeRange, videoTrack, CMTime.Zero, out var nsError);

            var transformer = new AVMutableVideoCompositionLayerInstruction
            {
                TrackID = videoTrack.TrackID
            };

            var t1 = CGAffineTransform.MakeTranslation(videoTrack.NaturalSize.Height, 0);
            var t2 = CGAffineTransform.Rotate(t1, (nfloat)Math.PI / 2);
            var finalTransform = t2;
            transformer.SetTransform(t2, CMTime.Zero);

            var audioMix = AVMutableAudioMix.Create();
            audioMix.InputParameters = null;
            if (_authorizationAudioStatus == AVAuthorizationStatus.Authorized)
            {
                var compositionTrackAudio = composition.AddMutableTrack(AVMediaType.Audio, 0);
                var audioTrack = asset.TracksWithMediaType(AVMediaType.Audio).First();

                compositionTrackAudio.InsertTimeRange(new CMTimeRange
                {
                    Start = CMTime.Zero,
                    Duration = asset.Duration
                }, audioTrack, CMTime.Zero, out nsError);

                var mixParameters = new AVMutableAudioMixInputParameters
                {
                    TrackID = audioTrack.TrackID
                };

                mixParameters.SetVolumeRamp(1.0f, 1.0f, new CMTimeRange
                {
                    Start = CMTime.Zero,
                    Duration = asset.Duration
                });
                audioMix.InputParameters = new[] { mixParameters };
            }

            var instruction = new AVMutableVideoCompositionInstruction
            {
                TimeRange = assetTimeRange,
                LayerInstructions = new[] { transformer }
            };

            videoCompositionInstructions[0] = instruction;

            var videoComposition = new AVMutableVideoComposition();
            videoComposition.FrameDuration = new CMTime(1, (int)videoTrack.NominalFrameRate);
            videoComposition.RenderScale = 1;
            videoComposition.Instructions = videoCompositionInstructions;
            videoComposition.RenderSize = renderSize;

            var outputFileName = new NSUuid().AsString();
            var documentsPath = NSSearchPath.GetDirectories(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User, true).First();
            var outputFilePath = Path.Combine(documentsPath, Path.ChangeExtension(outputFileName, "mp4"));
            _exportLocation = NSUrl.CreateFileUrl(outputFilePath, false, null);

            exportSession = new AVAssetExportSession(composition, AVAssetExportSession.PresetHighestQuality)
            {
                OutputUrl = _exportLocation,
                OutputFileType = AVFileType.Mpeg4,
                VideoComposition = videoComposition,
                AudioMix = audioMix,
                ShouldOptimizeForNetworkUse = true
            };
            exportSession.ExportAsynchronously(OnExportDone);

            CleanupLocation(outputFileUrl);
        }

        private void OnExportDone()
        {
            if (exportSession.Status == AVAssetExportSessionStatus.Completed && _successfulRecord)
                CheckPhotoLibraryAuthorizationStatus(PHPhotoLibrary.AuthorizationStatus);
            else
            {
                CleanupLocation(_exportLocation);
                StopLoading();
            }
        }

        private void CheckPhotoLibraryAuthorizationStatus(PHAuthorizationStatus authorizationStatus)
        {
            if (authorizationStatus == PHAuthorizationStatus.Authorized)
                PHPhotoLibrary.SharedPhotoLibrary.PerformChanges(CreateResourceInPhotoLibrary, PhotoLibraryResult);
            else
            {
                StopLoading();
                CleanupLocation(_exportLocation);
            }
        }

        private void CreateResourceInPhotoLibrary()
        {
            PHAssetChangeRequest.FromVideo(_exportLocation);
        }

        private void PhotoLibraryResult(bool success, NSError error)
        {
            if (success && error == null)
                SendVideoToDescription();
            else
            {
                StopLoading();
                CleanupLocation(_exportLocation);
            }
        }

        private void CleanupLocation(NSUrl location)
        {
            var path = location.Path;
            if (NSFileManager.DefaultManager.FileExists(path))
            {
                if (!NSFileManager.DefaultManager.Remove(path, out var err))
                {
                    // Could not remove file at url: {outputFileUrl}
                }
            }
        }
    }
}
