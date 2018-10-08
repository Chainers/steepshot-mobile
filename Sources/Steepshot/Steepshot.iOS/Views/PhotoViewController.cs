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
using PureLayout.Net;
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

        private readonly AVCaptureSession _captureSession;
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

        private UIView _bottomPanel;
        private UIButton _photoTabButton;
        private UIButton _videoTabButton;
        private UIButton _swapCameraButton;
        private UIButton _photoButton;
        private UIImageView _galleryButton;
        private UIView _pointerView;
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

        public PhotoViewController()
        {
            _galleryTap = new UITapGestureRecognizer(GalleryTap);
            _photoTabTap = new UITapGestureRecognizer(SwitchToPhotoMode);
            _videoTabTap = new UITapGestureRecognizer(SwitchToVideoMode);
            _captureSession = new AVCaptureSession();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            SetupCameraControlls();
            SwitchTheme(Theme.Dark);

            _videoPreviewLayer = new AVCaptureVideoPreviewLayer(_captureSession)
            {
                VideoGravity = AVLayerVideoGravity.ResizeAspectFill
            };

            liveCameraStream.Layer.AddSublayer(_videoPreviewLayer);
        }

        public override void ViewWillAppear(bool animated)
        {
            NavigationController.SetNavigationBarHidden(true, false);
            CheckDeviceOrientation(null);
            SetGalleryButton();
            ToogleButtons(true);

            _galleryButton.AddGestureRecognizer(_galleryTap);
            _photoTabButton.AddGestureRecognizer(_photoTabTap);
            _videoTabButton.AddGestureRecognizer(_videoTabTap);

            closeButton.TouchDown += GoBack;
            flashButton.TouchDown += OnFlashTouch;
            enableCameraAccess.TouchDown += EnableCameraAccess;
            _photoButton.TouchDown += CaptureContent;
            _photoButton.TouchUpInside += OnPhotoButtonUp;
            _photoButton.TouchUpOutside += OnPhotoButtonUp;
            _swapCameraButton.TouchDown += SwitchCameraButtonTapped;
            ((MainTabBarController)NavigationController.ViewControllers[0]).DidEnterBackgroundAction += DidEnterBackground;
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

                Constants.CreateGradient(_pointerView, 0, GradientType.Orange);

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

            closeButton.TouchDown -= GoBack;
            flashButton.TouchDown -= OnFlashTouch;
            enableCameraAccess.TouchDown -= EnableCameraAccess;
            _photoButton.TouchDown -= CaptureContent;
            _photoButton.TouchUpInside -= OnPhotoButtonUp;
            _photoButton.TouchUpOutside -= OnPhotoButtonUp;
            _swapCameraButton.TouchDown -= SwitchCameraButtonTapped;
            ((MainTabBarController)NavigationController.ViewControllers[0]).DidEnterBackgroundAction -= DidEnterBackground;

            if (_orientationChangeEventToken != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_orientationChangeEventToken);
                _orientationChangeEventToken.Dispose();
            }

            base.ViewWillDisappear(animated);
        }

        private void SetupCameraControlls()
        {
            View.BackgroundColor = Constants.R255G255B255;

            _bottomPanel = new UIView();
            _bottomPanel.BackgroundColor = UIColor.White;
            _bottomPanel.Hidden = true;

            var bottomSeparator = new UIView();
            bottomSeparator.BackgroundColor = Constants.R255G255B255.ColorWithAlpha(0.2f);

            _photoTabButton = new UIButton();
            _photoTabButton.TitleLabel.Font = Constants.Semibold14;
            _photoTabButton.SetTitle(AppDelegate.Localization.GetText(LocalizationKeys.Photo), UIControlState.Normal);
            _photoTabButton.SetTitleColor(UIColor.White, UIControlState.Normal);

            _videoTabButton = new UIButton();
            _videoTabButton.TitleLabel.Font = Constants.Semibold14;
            _videoTabButton.SetTitle(AppDelegate.Localization.GetText(LocalizationKeys.Video), UIControlState.Normal);
            _videoTabButton.SetTitleColor(UIColor.White, UIControlState.Normal);

            View.AddSubview(_bottomPanel);
            View.AddSubview(bottomSeparator);
            View.AddSubview(_photoTabButton);
            View.AddSubview(_videoTabButton);

            _pointerView = new UIView();
            View.AddSubview(_pointerView);

            _galleryButton = new UIImageView();
            _galleryButton.Image = UIImage.FromBundle("ic_gallery.png");
            _galleryButton.ContentMode = UIViewContentMode.ScaleAspectFill;
            _galleryButton.Layer.CornerRadius = 20;
            _galleryButton.ClipsToBounds = true;
            _galleryButton.UserInteractionEnabled = true;
            View.AddSubview(_galleryButton);

            _swapCameraButton = new UIButton();
            _swapCameraButton.UserInteractionEnabled = true;
            _swapCameraButton.ContentMode = UIViewContentMode.ScaleAspectFill;
            _swapCameraButton.Layer.CornerRadius = 20;
            View.AddSubview(_swapCameraButton);

            closeButton.Layer.CornerRadius = 20;
            flashButton.Layer.CornerRadius = 20;
            flashButton.BackgroundColor = UIColor.Black.ColorWithAlpha(0.8f);

            _photoButton = new UIButton();
            _photoButton.UserInteractionEnabled = true;
            _photoButton.Layer.CornerRadius = 30;
            _photoButton.Layer.BorderWidth = 2;
            TogglePhotoButton(MediaType.Photo);
            View.AddSubview(_photoButton);

            bottomSeparator.AutoSetDimension(ALDimension.Height, 1);
            bottomSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            bottomSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, DeviceHelper.IsXDevice ? 34 : 0);
            bottomSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Right);

            _photoTabButton.AutoSetDimension(ALDimension.Height, 60);
            _photoTabButton.AutoSetDimension(ALDimension.Width, View.Frame.Width / 2);
            _photoTabButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _photoTabButton.AutoPinEdge(ALEdge.Bottom, ALEdge.Top, bottomSeparator);

            _videoTabButton.AutoSetDimension(ALDimension.Height, 60);
            _videoTabButton.AutoMatchDimension(ALDimension.Width, ALDimension.Width, _photoTabButton);
            _videoTabButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            _videoTabButton.AutoPinEdge(ALEdge.Bottom, ALEdge.Top, bottomSeparator);
            _videoTabButton.AutoPinEdge(ALEdge.Left, ALEdge.Right, _photoTabButton);

            _pointerView.AutoSetDimension(ALDimension.Height, 2);
            _pointerView.AutoMatchDimension(ALDimension.Width, ALDimension.Width, _photoTabButton);
            _pointerView.AutoPinEdge(ALEdge.Bottom, ALEdge.Top, bottomSeparator);
            _videoConstraint = _pointerView.AutoAlignAxis(ALAxis.Vertical, _videoTabButton);
            _photoConstraint = _pointerView.AutoAlignAxis(ALAxis.Vertical, _photoTabButton);
            _videoConstraint.Active = false;

            _galleryButton.AutoSetDimensionsToSize(new CGSize(40, 40));
            _galleryButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);
            _galleryButton.AutoPinEdge(ALEdge.Bottom, ALEdge.Top, bottomSeparator, -71);

            _swapCameraButton.AutoSetDimensionsToSize(new CGSize(40, 40));
            _swapCameraButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 30);
            _swapCameraButton.AutoPinEdge(ALEdge.Bottom, ALEdge.Top, bottomSeparator, -71);

            var bottomPanelHeight = UIScreen.MainScreen.Bounds.Size.Height - (DeviceHelper.IsXDevice ? 124 : 80) - UIScreen.MainScreen.Bounds.Width;
            _bottomPanel.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _bottomPanel.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            _bottomPanel.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            _bottomPanel.AutoSetDimension(ALDimension.Height, bottomPanelHeight);

            _photoButton.AutoSetDimensionsToSize(new CGSize(60, 60));
            _photoButton.AutoAlignAxisToSuperviewAxis(ALAxis.Vertical);
            _photoButton.AutoPinEdge(ALEdge.Bottom, ALEdge.Top, bottomSeparator, -bottomPanelHeight / 2);
            _activePanelHeight = (float)bottomPanelHeight - 60;

            topCloseBtnConstraint.Constant = DeviceHelper.IsXDevice ? 64 : 20;
            topFlashBtnConstraint.Constant = DeviceHelper.IsXDevice ? 64 : 20;
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

        private void SwitchTheme(Theme theme)
        {
            var buttonBGColor = theme.Equals(Theme.Dark) ? UIColor.Black.ColorWithAlpha(0.8f) : UIColor.Black.ColorWithAlpha(0.05f);

            _swapCameraButton.SetImage(theme.Equals(Theme.Dark) ? UIImage.FromBundle("ic_revert") : UIImage.FromBundle("ic_revert_dark"), UIControlState.Normal);
            _swapCameraButton.BackgroundColor = buttonBGColor;

            closeButton.SetImage(theme.Equals(Theme.Dark) ? UIImage.FromBundle("ic_white_close") : UIImage.FromBundle("ic_close_black"), UIControlState.Normal);
            closeButton.BackgroundColor = buttonBGColor;

            _photoTabButton.SetTitleColor(theme.Equals(Theme.Dark) ? UIColor.White : UIColor.Black, UIControlState.Normal);
            _videoTabButton.SetTitleColor(theme.Equals(Theme.Dark) ? UIColor.White : UIColor.Black, UIControlState.Normal);

            _bottomPanel.Hidden = theme.Equals(Theme.Dark);
            flashButton.Hidden = !theme.Equals(Theme.Dark);
        }

        private void SwitchMode(MediaType targetMode)
        {
            _photoTabButton.Enabled = targetMode != MediaType.Photo;
            _videoTabButton.Enabled = targetMode != MediaType.Video;
            _photoConstraint.Active = targetMode != MediaType.Video;
            _videoConstraint.Active = targetMode != MediaType.Photo;

            TogglePhotoButton(targetMode);

            UIView.Animate(0.2, 0, UIViewAnimationOptions.CurveEaseOut, View.LayoutIfNeeded, null);

            _currentMode = targetMode;
        }

        private void TogglePhotoButton(MediaType cameraMode)
        {
            var color = cameraMode.Equals(MediaType.Photo) ? Constants.R255G255B255 : Constants.R255G28B5;
            var circle = CircleBorder(50, color);

            _photoButton.BackgroundColor = cameraMode.Equals(MediaType.Photo) ? Constants.R217G217B217 : UIColor.White;
            _photoButton.SetImage(circle, UIControlState.Normal);
            _photoButton.Layer.BorderColor = color.CGColor;
        }

        private UIImage CircleBorder(nfloat diameter, UIColor color, bool opaque = false)
        {
            var rect = new CGRect(0, 0, diameter, diameter);

            UIGraphics.BeginImageContextWithOptions(rect.Size, opaque, 0);
            var ctx = UIGraphics.GetCurrentContext();
            ctx.SaveState();
            ctx.SetLineWidth(3);
            ctx.SetFillColor(color.CGColor);
            ctx.FillEllipseInRect(rect);
            ctx.RestoreState();
            var img = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            return img;
        }

        private void CaptureContent(object sender, EventArgs e)
        {
            switch (_currentMode)
            {
                case MediaType.Photo:
                    ToogleButtons(false);
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
                        ToogleButtons(false);

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
                ToogleButtons(true);
                _videoFileOutput?.StopRecording();
                _isRecording = !_isRecording;
            }
        }

        private void StartAnimation()
        {
            _sl.Hidden = false;
            _animation = CABasicAnimation.FromKeyPath("strokeEnd");
            _animation.From = NSNumber.FromDouble(0.0);
            _animation.To = NSNumber.FromDouble(1.0);
            _animation.Duration = 20;
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

        private void ToogleButtons(bool isEnabled)
        {
            _photoButton.Enabled = isEnabled;
            closeButton.Enabled = isEnabled;
            _swapCameraButton.Enabled = isEnabled;
            _photoTabButton.Enabled = isEnabled;
            _videoTabButton.Enabled = isEnabled;
            _galleryButton.UserInteractionEnabled = isEnabled;
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

        private void SetGalleryButton()
        {
            PHPhotoLibrary.RequestAuthorizationAsync().ContinueWith((status) =>
            {
                if (status.Result == PHAuthorizationStatus.Authorized)
                {
                    var fetchedAssets = PHAsset.FetchAssets(PHAssetMediaType.Image, null);
                    if (fetchedAssets.LastObject is PHAsset lastGalleryPhoto)
                    {
                        _galleryButton.UserInteractionEnabled = true;
                        var PHImageManager = new PHImageManager();
                        PHImageManager.RequestImageForAsset(lastGalleryPhoto, new CGSize(300, 300),
                                                            PHImageContentMode.AspectFill, new PHImageRequestOptions()
                                                            {
                                                                DeliveryMode = PHImageRequestOptionsDeliveryMode.Opportunistic,
                                                                ResizeMode = PHImageRequestOptionsResizeMode.Exact
                                                            }, (img, info) =>
                                                            {
                                                                _galleryButton.Image = img;
                                                            });
                    }
                    else
                        _galleryButton.UserInteractionEnabled = false;
                }
                else
                    _galleryButton.UserInteractionEnabled = true;
            });
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
                        enableCameraAccess.Hidden = false;
                        _photoButton.Hidden = true;
                        flashButton.Hidden = true;
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
                    _videoPreviewLayer.Frame = liveCameraStream.Frame;
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
                    _videoPreviewLayer.Frame = new CGRect(new CGPoint(0, DeviceHelper.IsXDevice ? 124 : 80), new CGSize(liveCameraStream.Frame.Width, liveCameraStream.Frame.Height));
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
            InvokeOnMainThread(() => NavigationController.PushViewController(descriptionViewController, true));
        }

        public void DidEnterBackground()
        {
            StopCapturing(true);
        }

        public void FinishedRecording(AVCaptureFileOutput captureOutput, NSUrl outputFileUrl, NSObject[] connections, NSError error)
        {
            _sl.RemoveAllAnimations();
            _sl.Hidden = true;

            ToogleButtons(true);
            _isRecording = false;

            if (_isCancelled)
            {
                _isCancelled = false;
                CleanupLocation(outputFileUrl);
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
                CleanupLocation(outputFileUrl);
                return;
            }

            var videoTrack = asset.TracksWithMediaType(AVMediaType.Video)[0];
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
                var audioTrack = asset.TracksWithMediaType(AVMediaType.Audio)[0];

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
                CleanupLocation(_exportLocation);
        }

        private void CheckPhotoLibraryAuthorizationStatus(PHAuthorizationStatus authorizationStatus)
        {
            if (authorizationStatus == PHAuthorizationStatus.Authorized)
                PHPhotoLibrary.SharedPhotoLibrary.PerformChanges(CreateResourceInPhotoLibrary, PhotoLibraryResult);
            else
                CleanupLocation(_exportLocation);
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
                CleanupLocation(_exportLocation);
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
