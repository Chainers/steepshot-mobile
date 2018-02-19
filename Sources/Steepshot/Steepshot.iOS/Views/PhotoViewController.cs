using System;
using System.Drawing;
using System.Linq;
using AVFoundation;
using CoreGraphics;
using CoreMedia;
using Foundation;
using Photos;
using Steepshot.Core.Utils;
using Steepshot.iOS.ViewControllers;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class PhotoViewController : BaseViewController, IAVCapturePhotoCaptureDelegate
    {
        private AVCaptureSession _captureSession;
        private AVCaptureDeviceInput _captureDeviceInput;
        private UIImagePickerController _imagePicker;
        private AVCapturePhotoOutput _capturePhotoOutput;
        private AVCaptureVideoPreviewLayer _videoPreviewLayer;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            galleryButton.Layer.CornerRadius = galleryButton.Frame.Height / 2;

            photoButton.TouchDown += CapturePhoto;
            closeButton.TouchDown += GoBack;
            swapCameraButton.TouchDown += SwitchCameraButtonTapped;
            enableCameraAccess.TouchDown += EnableCameraAccess;

            var galleryTap = new UITapGestureRecognizer(GalleryTap);
            galleryButton.AddGestureRecognizer(galleryTap);

            _imagePicker = new UIImagePickerController();
            _imagePicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
            //_imagePicker.MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary);

            _imagePicker.FinishedPickingMedia += FinishedPickingMedia;
            _imagePicker.Canceled += Handle_Canceled;
        }

        private void EnableCameraAccess(object sender, EventArgs e)
        {
            UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString), new NSDictionary(), null);
        }

        private void GalleryTap()
        {
            if (PHPhotoLibrary.AuthorizationStatus == PHAuthorizationStatus.Authorized)
                NavigationController.PresentModalViewController(_imagePicker, true);
            else
                UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString), new NSDictionary(), null);
        }

        private void Handle_Canceled(object sender, EventArgs e)
        {
            _imagePicker.DismissViewControllerAsync(false);
        }

        public override bool PrefersStatusBarHidden()
        {
            return true;
        }

        public override void ViewWillAppear(bool animated)
        {
            NavigationController.SetNavigationBarHidden(true, false);
            SetGalleryButton();
        }

        public override void ViewDidAppear(bool animated)
        {
            if (_captureSession == null)
                AuthorizeCameraUse();
            else if (!_captureSession.Running)
                _captureSession.StartRunning();
        }

        private async void SetGalleryButton()
        {
            var status = await PHPhotoLibrary.RequestAuthorizationAsync();
            if (status == PHAuthorizationStatus.Authorized)
            {
                var options = new PHFetchOptions
                {
                    SortDescriptors = new[] { new NSSortDescriptor("creationDate", false) },
                    FetchLimit = 5,
                };

                var fetchedAssets = PHAsset.FetchAssets(PHAssetMediaType.Image, options);
                var lastGalleryPhoto = fetchedAssets.FirstOrDefault() as PHAsset;
                if (lastGalleryPhoto != null)
                {
                    var PHImageManager = new PHImageManager();
                    PHImageManager.RequestImageForAsset(lastGalleryPhoto, new CGSize(300, 300),
                                            PHImageContentMode.AspectFill, new PHImageRequestOptions(), (img, info) =>
                                            {
                                                galleryButton.Image = img;
                                            });
                }
                else
                    galleryButton.Image = UIImage.FromBundle("ic_noavatar");
            }
        }

        private void FinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs e)
        {
            var originalImage = e.Info[UIImagePickerController.OriginalImage] as UIImage;
            GoToDescription(originalImage);
            _imagePicker.DismissViewControllerAsync(false);
        }

        private void GoBack(object sender, EventArgs e)
        {
            NavigationController.PopViewController(true);
        }

        private void CapturePhoto(object sender, EventArgs e)
        {
            var settingKeys = new object[]
            {
                    AVVideo.CodecKey,
                    AVVideo.CompressionPropertiesKey,
            };

            var settingObjects = new object[]
            {
                    new NSString("jpeg"),
                    new NSDictionary(AVVideo.QualityKey, 0.5),
            };

            var settingsDictionary = NSDictionary<NSString, NSObject>.FromObjectsAndKeys(settingObjects, settingKeys);

            var settings = AVCapturePhotoSettings.FromFormat(settingsDictionary);
            if (_captureDeviceInput.Device.Position == AVCaptureDevicePosition.Back)
                settings.FlashMode = AVCaptureFlashMode.Auto;

            _capturePhotoOutput.CapturePhoto(settings, this);
        }

        [Export("captureOutput:didFinishProcessingPhotoSampleBuffer:previewPhotoSampleBuffer:resolvedSettings:bracketSettings:error:")]
        public void DidFinishProcessingPhoto(AVCapturePhotoOutput captureOutput, CMSampleBuffer photoSampleBuffer, CMSampleBuffer previewPhotoSampleBuffer, AVCaptureResolvedPhotoSettings resolvedSettings, AVCaptureBracketedStillImageSettings bracketSettings, NSError error)
        {
            try
            {
                var jpegData = AVCapturePhotoOutput.GetJpegPhotoDataRepresentation(photoSampleBuffer, previewPhotoSampleBuffer);
                var photo = UIImage.LoadFromData(jpegData);
                GoToDescription(photo);
            }
            catch(Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
                ShowAlert(Core.Localization.LocalizationKeys.PhotoProcessingError);
            }
        }

        private UIImage CropImage(UIImage sourceImage, int cropX, int cropY, int width, int height)
        {
            var imgSize = sourceImage.Size;
            UIGraphics.BeginImageContext(new SizeF(width, height));
            var context = UIGraphics.GetCurrentContext();
            var clippedRect = new RectangleF(0, 0, width, height);
            context.ClipToRect(clippedRect);

            var drawRect = new CGRect(-cropX, -cropY, imgSize.Width, imgSize.Height);
            sourceImage.Draw(drawRect);
            var modifiedImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            context.Dispose();
            return modifiedImage;
        }

        public override void ViewWillDisappear(bool animated)
        {
            if (_captureSession.Running)
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
                    return;
                }
            }
            SetupLiveCameraStream();
        }

        private void SetupLiveCameraStream()
        {
            _captureSession = new AVCaptureSession();

            AVCaptureDevice captureDevice;
            captureDevice = AVCaptureDevice.GetDefaultDevice(AVCaptureDeviceType.BuiltInDualCamera, AVMediaType.Video, AVCaptureDevicePosition.Back);

            if (captureDevice == null)
                captureDevice = AVCaptureDevice.GetDefaultDevice(AVMediaType.Video);

            ConfigureCameraForDevice(captureDevice);
            _captureDeviceInput = AVCaptureDeviceInput.FromDevice(captureDevice);
            if (!_captureSession.CanAddInput(_captureDeviceInput))
                return;

            _capturePhotoOutput = new AVCapturePhotoOutput();
            _capturePhotoOutput.IsHighResolutionCaptureEnabled = true;
            _capturePhotoOutput.IsLivePhotoCaptureEnabled = false;


            if (!_captureSession.CanAddOutput(_capturePhotoOutput))
                return;

            _captureSession.BeginConfiguration();

            _captureSession.SessionPreset = AVCaptureSession.PresetPhoto;
            _captureSession.AddInput(_captureDeviceInput);
            _captureSession.AddOutput(_capturePhotoOutput);

            _captureSession.CommitConfiguration();

            _videoPreviewLayer = new AVCaptureVideoPreviewLayer(_captureSession)
            {
                Frame = liveCameraStream.Frame,
                VideoGravity = AVLayerVideoGravity.ResizeAspectFill,
            };

            liveCameraStream.Layer.AddSublayer(_videoPreviewLayer);

            _captureSession.StartRunning();
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

        /*
        private void PhotoPicked(NSIndexPath indexPath)
        {
            var collectionCell = (PhotoCollectionViewCell)photoCollection.CellForItem(indexPath);

            if (collectionCell.Asset != null)
            {
                using (var m = new PHImageManager())
                {
                    var options = new PHImageRequestOptions();

                    options.DeliveryMode = PHImageRequestOptionsDeliveryMode.FastFormat;
                    options.Synchronous = false;
                    options.NetworkAccessAllowed = true;

                    m.RequestImageData(collectionCell.Asset, options, (data, dataUti, orientation, info) =>
                       {
                           if (data != null)
                           {
                               var photo = UIImage.LoadFromData(data);
                               GoToDescription(photo);
                           }
                       });
                }
            }
        }*/

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

        private void GoToDescription(UIImage image)
        {
            var descriptionViewController = new DescriptionViewController(image, "jpg");
            NavigationController.PushViewController(descriptionViewController, true);
        }
    }
}
