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
using System.Linq;
using CoreVideo;
using CoreFoundation;
using System.IO;
using System.Diagnostics.Contracts;
using PureLayout.Net;

namespace Steepshot.iOS.Views
{
    public partial class PhotoViewController : BaseViewController, IAVCapturePhotoCaptureDelegate, IAVCaptureFileOutputRecordingDelegate
    {
        enum CameraMode
        { 
            Photo,
            Video
        }

        private AVCaptureSession _captureSession;
        private AVCaptureDevice _backCamera;
        private AVCaptureDevice _frontCamera;
        private AVCaptureDevice _currentCamera;
        private AVCaptureDeviceInput _captureDeviceInput;
        private AVCapturePhotoOutput _capturePhotoOutput;
        private AVCaptureVideoPreviewLayer _videoPreviewLayer;
        private AVCaptureFlashMode _flashMode = AVCaptureFlashMode.Auto;
        private AVCaptureMovieFileOutput _videoFileOutput;
        private UIDeviceOrientation currentOrientation;
        private UIDeviceOrientation orientationOnPhoto;
        private NSObject _orientationChangeEventToken;

        private UIButton _testSwitchModeBtn;
        private CameraMode _currentMode = CameraMode.Photo;

        private CAShapeLayer _sl;
        private CABasicAnimation _animation;
        private UIBezierPath _bezierPath;
        private bool _initialized;
        private bool _isRecording;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            galleryButton.Layer.CornerRadius = galleryButton.Frame.Height / 2;

            var photoTap = new UITapGestureRecognizer(CapturePhoto);
            photoButton.AddGestureRecognizer(photoTap);

            closeButton.TouchDown += GoBack;
            flashButton.TouchDown += OnFlashTouch;
            swapCameraButton.TouchDown += SwitchCameraButtonTapped;
            enableCameraAccess.TouchDown += EnableCameraAccess;

            var galleryTap = new UITapGestureRecognizer(GalleryTap);
            galleryButton.AddGestureRecognizer(galleryTap);

            _testSwitchModeBtn = new UIButton();
            _testSwitchModeBtn.BackgroundColor = UIColor.White;
            _testSwitchModeBtn.Layer.CornerRadius = 20;
            _testSwitchModeBtn.SetTitleColor(UIColor.Black, UIControlState.Normal);
            _testSwitchModeBtn.SetTitle("P", UIControlState.Normal);
            _testSwitchModeBtn.TouchDown += SwitchCameraMode;
            View.AddSubview(_testSwitchModeBtn);

            _testSwitchModeBtn.AutoPinEdge(ALEdge.Bottom, ALEdge.Top, swapCameraButton, -100);
            _testSwitchModeBtn.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);
            _testSwitchModeBtn.AutoSetDimensionsToSize(new CGSize(40, 40));
        }

        private void CapturePhoto()
        {
            switch (_currentMode)
            {
                case CameraMode.Photo:
                    ToogleButtons(false);
                    var settingKeys = new object[]
                    {
                        AVVideo.CodecKey,
                        AVVideo.CompressionPropertiesKey,
                    };

                    var settingObjects = new object[]
                    {
                        new NSString("jpeg"),
                        new NSDictionary(AVVideo.QualityKey, 1),
                    };

                    var settingsDictionary = NSDictionary<NSString, NSObject>.FromObjectsAndKeys(settingObjects, settingKeys);

                    var settings = AVCapturePhotoSettings.FromFormat(settingsDictionary);

                    if (_capturePhotoOutput.SupportedFlashModes.Length > 0 && _captureDeviceInput.Device.Position == AVCaptureDevicePosition.Back)
                        settings.FlashMode = _flashMode;

                    orientationOnPhoto = currentOrientation;
                    _capturePhotoOutput.CapturePhoto(settings, this);
                    break;
                case CameraMode.Video:
                    if (!_isRecording)
                    {
                        _testSwitchModeBtn.Enabled = false;
                        StartAnimation();

                        var outputFileName = new NSUuid().AsString();
                        var outputFilePath = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(outputFileName, "mov"));

                        _videoFileOutput?.StartRecordingToOutputFile(NSUrl.FromFilename(outputFilePath), this);
                    }
                    else
                    {
                        _testSwitchModeBtn.Enabled = true;
                        _sl.RemoveAllAnimations();
                        _sl.Hidden = true;

                        _videoFileOutput?.StopRecording();
                    }

                    _isRecording = !_isRecording;
                    break;
            }
        }

        private void SwitchCameraMode(object sender, EventArgs e)
        {
            if (_captureSession != null)
                _captureSession.StopRunning();

            switch (_currentMode)
            { 
                case CameraMode.Photo:
                    _currentMode = CameraMode.Video;
                    _testSwitchModeBtn.SetTitle("V", UIControlState.Normal);
                    SetupVideoTest();
                    break;
                case CameraMode.Video:
                    _currentMode = CameraMode.Photo;
                    AuthorizeCameraUse();
                    _testSwitchModeBtn.SetTitle("P", UIControlState.Normal);
                    break;
            }

            _captureSession?.StartRunning();
        }

        public override void ViewDidLayoutSubviews()
        {
            if (!_initialized)
            {
                _bezierPath = new UIBezierPath();
                _bezierPath.AddArc(photoButton.Center, 70, 3f * (float)Math.PI / 2f, 4.712327f, true);

                _sl = new CAShapeLayer();
                _sl.LineWidth = 3;
                _sl.StrokeColor = UIColor.FromRGB(255, 17, 0).CGColor;
                _sl.FillColor = UIColor.Black.ColorWithAlpha(0.5f).CGColor;
                _sl.LineCap = CAShapeLayer.CapRound;
                _sl.LineJoin = CAShapeLayer.CapRound;
                _sl.StrokeStart = 0.0f;
                _sl.StrokeEnd = 0.0f;
                _sl.Hidden = true;
                _sl.Path = _bezierPath.CGPath;

                View.Layer.AddSublayer(_sl);

                _initialized = true;
            }
        }

        private void PhotoButtonLongPress()
        {
            if (!_isRecording)
            {
                StartAnimation();

                var outputFileName = new NSUuid().AsString();
                var outputFilePath = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(outputFileName, "mov"));

                _videoFileOutput?.StartRecordingToOutputFile(NSUrl.FromFilename(outputFilePath), this);
            }
            else
            {
                _sl.RemoveAllAnimations();
                _sl.Hidden = true;

                _videoFileOutput?.StopRecording();
            }

            _isRecording = !_isRecording;
        }

        private void StartAnimation()
        {
            _sl.Hidden = false;
            _animation = CABasicAnimation.FromKeyPath("strokeEnd");
            _animation.From = NSNumber.FromDouble(0.0);
            _animation.To = NSNumber.FromDouble(1.0);
            _animation.Duration = 30;
            _animation.FillMode = CAFillMode.Forwards;
            _animation.RemovedOnCompletion = false;
            _sl.AddAnimation(_animation, "drawLineAnimation");
        }

        private void EnableCameraAccess(object sender, EventArgs e)
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

        public override void ViewWillAppear(bool animated)
        {
            NavigationController.SetNavigationBarHidden(true, false);
            CheckDeviceOrientation(null);
            SetGalleryButton();
            ToogleButtons(true);
        }

        private void ToogleButtons(bool isEnabled)
        {
            photoButton.Enabled = isEnabled;
            closeButton.Enabled = isEnabled;
            swapCameraButton.Enabled = isEnabled;
        }

        public override void ViewDidAppear(bool animated)
        {
            _orientationChangeEventToken = NSNotificationCenter.DefaultCenter.AddObserver(UIDevice.OrientationDidChangeNotification, CheckDeviceOrientation);
            if (_captureSession == null)
                AuthorizeCameraUse();
            else if (!_captureSession.Running)
                _captureSession.StartRunning();
        }

        public override void ViewDidDisappear(bool animated)
        {
            if (_orientationChangeEventToken != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_orientationChangeEventToken);
                _orientationChangeEventToken.Dispose();
            }
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

        private async void SetGalleryButton()
        {
            var status = await PHPhotoLibrary.RequestAuthorizationAsync();
            if (status == PHAuthorizationStatus.Authorized)
            {
                var fetchedAssets = PHAsset.FetchAssets(PHAssetMediaType.Image, null);
                var lastGalleryPhoto = fetchedAssets.LastObject as PHAsset;
                if (lastGalleryPhoto != null)
                {
                    galleryButton.UserInteractionEnabled = true;
                    var PHImageManager = new PHImageManager();
                    PHImageManager.RequestImageForAsset(lastGalleryPhoto, new CGSize(300, 300),
                                                        PHImageContentMode.AspectFill, new PHImageRequestOptions()
                                                        {
                                                            DeliveryMode = PHImageRequestOptionsDeliveryMode.Opportunistic,
                                                            ResizeMode = PHImageRequestOptionsResizeMode.Exact
                                                        }, (img, info) =>
                          {
                              galleryButton.Image = img;
                          });
                }
                else
                    galleryButton.UserInteractionEnabled = false;
            }
            else
                galleryButton.UserInteractionEnabled = true;
        }

        private void GoBack(object sender, EventArgs e)
        {
            NavigationController.PopViewController(true);
        }

        private void OnFlashTouch(object sender, EventArgs e)
        {
            switch (_flashMode)
            {
                case AVCaptureFlashMode.Auto:
                    _flashMode = AVCaptureFlashMode.On;
                    flashButton.SetImage(UIImage.FromBundle("ic_flashOn"), UIControlState.Normal);
                    break;
                case AVCaptureFlashMode.On:
                    _flashMode = AVCaptureFlashMode.Off;
                    flashButton.SetImage(UIImage.FromBundle("ic_flashOff"), UIControlState.Normal);
                    break;
                default:
                    _flashMode = AVCaptureFlashMode.Auto;
                    flashButton.SetImage(UIImage.FromBundle("ic_flash"), UIControlState.Normal);
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
                GoToDescription(photo, orientationOnPhoto);
            }
            catch (Exception ex)
            {
                ShowAlert(new InternalException(Core.Localization.LocalizationKeys.PhotoProcessingError, ex));
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            if (_captureSession != null && _captureSession.Running)
                _captureSession.StopRunning();

            NavigationController.SetNavigationBarHidden(false, false);
            base.ViewWillDisappear(animated);
        }

        private async void AuthorizeCameraUse()
        {
            var authorizationStatus = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video);

            if (authorizationStatus != AVAuthorizationStatus.Authorized)
            {
                if (!await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVMediaType.Video))
                {
                    enableCameraAccess.Hidden = false;

                    photoButton.Hidden = true;
                    flashButton.Hidden = true;
                    swapCameraButton.Hidden = true;
                    return;
                }
            }

            SetupLiveCameraStream();
        }

        private void SetupVideoTest()
        {
            _captureSession = new AVCaptureSession();

            _captureSession.SessionPreset = AVCaptureSession.PresetHigh;

            // device
            var deviceDiscoverySession = AVCaptureDeviceDiscoverySession.Create(new AVCaptureDeviceType[] { AVCaptureDeviceType.BuiltInWideAngleCamera }, AVMediaType.Video, AVCaptureDevicePosition.Unspecified);
            var devices = deviceDiscoverySession.Devices;
            foreach (var device in devices)
            {
                if (device.Position == AVCaptureDevicePosition.Back)
                    _backCamera = device;
                else if (device.Position == AVCaptureDevicePosition.Front)
                    _frontCamera = device;
            }
            _currentCamera = _backCamera;

            try
            {
                var captureDeviceInput = AVCaptureDeviceInput.FromDevice(_currentCamera);
                _captureSession.AddInput(captureDeviceInput);
                _videoFileOutput = new AVCaptureMovieFileOutput();
                _captureSession.AddOutput(_videoFileOutput);
            }
            catch (Exception ex)
            { }

            // setup preview
            _videoPreviewLayer = new AVCaptureVideoPreviewLayer(_captureSession)
            {
                VideoGravity = AVLayerVideoGravity.ResizeAspectFill,
                Orientation = AVCaptureVideoOrientation.Portrait,
                Frame = new CGRect(new CGPoint(0, 0), new CGSize(liveCameraStream.Frame.Width, liveCameraStream.Frame.Width))
            };

            ClearCameraStreamSublayers();
            liveCameraStream.Layer.AddSublayer(_videoPreviewLayer);
            _captureSession.StartRunning();
        }

        private void ClearCameraStreamSublayers()
        {
            if (liveCameraStream.Layer.Sublayers == null)
                return;

            foreach (var layer in liveCameraStream.Layer.Sublayers)
            {
                layer.RemoveFromSuperLayer();
            }
        }

        private void SetupLiveCameraStream()
        {
            _captureSession = new AVCaptureSession();

            var deviceDiscoverySession = AVCaptureDeviceDiscoverySession.Create(new AVCaptureDeviceType[] { AVCaptureDeviceType.BuiltInWideAngleCamera }, AVMediaType.Video, AVCaptureDevicePosition.Unspecified);
            var devices = deviceDiscoverySession.Devices;
            foreach (var device in devices)
            {
                if (device.Position == AVCaptureDevicePosition.Back)
                    _backCamera = device;
                else if (device.Position == AVCaptureDevicePosition.Front)
                    _frontCamera = device;
            }
            _currentCamera = _backCamera;

            ConfigureCameraForDevice(_currentCamera);
            _captureDeviceInput = AVCaptureDeviceInput.FromDevice(_currentCamera);
            if (!_captureSession.CanAddInput(_captureDeviceInput))
                return;

            _capturePhotoOutput = new AVCapturePhotoOutput();
            _capturePhotoOutput.IsHighResolutionCaptureEnabled = true;
            _capturePhotoOutput.IsLivePhotoCaptureEnabled = false;

            if (!_captureSession.CanAddOutput(_capturePhotoOutput))
                return;

            SetupSessionInputOutput(_capturePhotoOutput);

            _videoPreviewLayer = new AVCaptureVideoPreviewLayer(_captureSession)
            {
                Frame = liveCameraStream.Frame,
                VideoGravity = AVLayerVideoGravity.ResizeAspectFill
            };

            ClearCameraStreamSublayers();
            liveCameraStream.Layer.AddSublayer(_videoPreviewLayer);
            _captureSession.StartRunning();
        }

        private void SetupSessionInputOutput(AVCaptureOutput output)
        {
            _captureSession.BeginConfiguration();
            _captureSession.SessionPreset = AVCaptureSession.PresetPhoto;
            _captureSession.AddInput(_captureDeviceInput);
            _captureSession.AddOutput(output);
            _captureSession.CommitConfiguration();
        }

        private void ConfigureCameraForDevice(AVCaptureDevice device)
        {
            var error = new NSError();
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
            var devicePosition = _captureDeviceInput.Device.Position;
            if (devicePosition == AVCaptureDevicePosition.Front)
                devicePosition = AVCaptureDevicePosition.Back;
            else
                devicePosition = AVCaptureDevicePosition.Front;

            var device = GetCameraForOrientation(devicePosition);
            ConfigureCameraForDevice(device);

            _captureSession.BeginConfiguration();
            _captureSession.RemoveInput(_captureDeviceInput);
            _captureDeviceInput = AVCaptureDeviceInput.FromDevice(device);
            _captureSession.AddInput(_captureDeviceInput);
            _captureSession.CommitConfiguration();
            CheckDeviceOrientation(null);
        }

        private AVCaptureDevice GetCameraForOrientation(AVCaptureDevicePosition orientation)
        {
            var devices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video);
            foreach (var device in devices)
            {
                if (device.Position == orientation)
                    return device;
            }
            return null;
        }

        private void GoToDescription(UIImage image, UIDeviceOrientation orientation)
        {
            var descriptionViewController = new DescriptionViewController(new List<Tuple<NSDictionary, UIImage>>() { new Tuple<NSDictionary, UIImage>(null, image) }, "jpg", orientation);
            NavigationController.PushViewController(descriptionViewController, true);
        }

        public void FinishedRecording(AVCaptureFileOutput captureOutput, NSUrl outputFileUrl, NSObject[] connections, NSError error)
        {
            Action cleanup = () =>
            {
                var path = outputFileUrl.Path;
                if (NSFileManager.DefaultManager.FileExists(path))
                {
                    if (!NSFileManager.DefaultManager.Remove(path, out var err))
                    {
                        // Could not remove file at url: {outputFileUrl}
                    }
                }
            };

            bool success = true;

            if (error != null)
            {
                // Movie file finishing error: {error.LocalizedDescription}
                success = ((NSNumber)error.UserInfo[AVErrorKeys.RecordingSuccessfullyFinished]).BoolValue;
            }

            if (success)
            {
                // Check authorization status
                PHPhotoLibrary.RequestAuthorization(status =>
                {
                    if (status == PHAuthorizationStatus.Authorized)
                    {
                        PHPhotoLibrary.SharedPhotoLibrary.PerformChanges(() =>
                        {
                            var options = new PHAssetResourceCreationOptions
                            {
                                ShouldMoveFile = true
                            };
                            var creationRequest = PHAssetCreationRequest.CreationRequestForAsset();
                            creationRequest.AddResource(PHAssetResourceType.Video, outputFileUrl, options);
                        }, (success2, error2) =>
                        {
                            if (!success2)
                            {
                                // Could not save movie to photo library: {error2}
                            }
                            cleanup();
                        });
                    }
                    else
                        cleanup();
                });
            }
            else
                cleanup();
        }
    }
}
