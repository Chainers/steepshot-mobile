using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using AVFoundation;
using CoreGraphics;
using Foundation;
using Photos;
using UIKit;

namespace Steepshot.iOS
{
	public partial class PhotoViewController : UIViewController
	{
		AVCaptureSession captureSession;
		AVCaptureDeviceInput captureDeviceInput;
		AVCaptureStillImageOutput stillImageOutput;
		PhotoCollectionViewSource source;

		private bool _isCameraAccessDenied = false;

		protected PhotoViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public PhotoViewController()
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			photoCollection.RegisterClassForCell(typeof(PhotoCollectionViewCell), "PhotoCollectionViewCell");
			photoCollection.RegisterNibForCell(UINib.FromName("PhotoCollectionViewCell", NSBundle.MainBundle), "PhotoCollectionViewCell");

			photoButton.TouchDown += PhotoButton_TouchDown;
			swapCameraButton.TouchDown += SwitchCameraButtonTapped;
			RequestPhotoAuth();
			AuthorizeCameraUse();
		}

		public override void ViewDidAppear(bool animated)
		{
			SetNavBar();
			base.ViewDidAppear(animated);
		}

		async void PhotoButton_TouchDown(object sender, EventArgs e)
		{
			var videoConnection = stillImageOutput.ConnectionFromMediaType(AVMediaType.Video);
			var sampleBuffer = await stillImageOutput.CaptureStillImageTaskAsync(videoConnection);
			var jpegImageAsNsData = AVCaptureStillImageOutput.JpegStillToNSData(sampleBuffer);
			var photo = UIImage.LoadFromData(jpegImageAsNsData);
			var cropY = (int)(photo.Size.Height - photo.Size.Width) / 2;
			UIImage cropped = CropImage(photo, 0, cropY, (int)photo.Size.Width, (int)photo.Size.Width);
			GoToDescription(cropped);
		}

		private UIImage CropImage(UIImage sourceImage, int crop_x, int crop_y, int width, int height)
		{
			var imgSize = sourceImage.Size;
			UIGraphics.BeginImageContext(new SizeF(width, height));
			var context = UIGraphics.GetCurrentContext();
			var clippedRect = new RectangleF(0, 0, width, height);
			context.ClipToRect(clippedRect);

			var drawRect = new CGRect(-crop_x, -crop_y, imgSize.Width, imgSize.Height);
			sourceImage.Draw(drawRect);
			var modifiedImage = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();
			context.Dispose();
			return modifiedImage;
		}

		private UIImage NormalizeImage(UIImage sourceImage)
		{
			var imgSize = sourceImage.Size;
			UIGraphics.BeginImageContextWithOptions(sourceImage.Size, false, sourceImage.CurrentScale);

			var drawRect = new CGRect(0, 0, imgSize.Width, imgSize.Height);
			sourceImage.Draw(drawRect);
			var modifiedImage = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			return modifiedImage;
		}

		private async Task RequestPhotoAuth()
		{
			var status = await PHPhotoLibrary.RequestAuthorizationAsync();
			if (status == PHAuthorizationStatus.Authorized)
			{
				source = new PhotoCollectionViewSource();
				photoCollection.DataSource = source;
				photoCollection.Delegate = new CollectionViewFlowDelegate(PhotoPicked);
				photoCollection.ReloadData();
			}
		}

		public override void ViewWillAppear(bool animated)
		{
			if (TabBarController != null)
			{
				TabBarController.NavigationController.NavigationBarHidden = true;
			}
			base.ViewWillAppear(animated);
		}

		async Task AuthorizeCameraUse()
		{
			var authorizationStatus = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video);

			if (authorizationStatus != AVAuthorizationStatus.Authorized)
			{
				if (!await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVMediaType.Video))
				{
					_isCameraAccessDenied = true;
					return;
				}
			}
			SetupLiveCameraStream();
		}

		void SwitchSource(object sender, EventArgs e)
		{
			if (!_isCameraAccessDenied)
			{
				swapCameraButton.Hidden = !swapCameraButton.Hidden;
				photoButton.Hidden = !photoButton.Hidden;
			}
			liveCameraStream.Hidden = !liveCameraStream.Hidden;
			photoCollection.Hidden = !photoCollection.Hidden;
			if (photoCollection.Hidden)
			{
				var leftBarButton = new UIBarButtonItem(UIImage.FromFile("gallery.png"), UIBarButtonItemStyle.Plain, SwitchSource);
				NavigationItem.SetLeftBarButtonItem(leftBarButton, true);
			}
			else
			{
				var leftBarButton = new UIBarButtonItem(UIImage.FromFile("small_camera"), UIBarButtonItemStyle.Plain, SwitchSource);
				NavigationItem.SetLeftBarButtonItem(leftBarButton, true);
			}
		}

		private void SetNavBar()
		{
			NavigationController.SetNavigationBarHidden(false, false);
			var barHeight = NavigationController.NavigationBar.Frame.Height;

			var tw = new UILabel(new CoreGraphics.CGRect(0, 0, 120, barHeight));
			tw.TextColor = UIColor.White;
			tw.Text = "CHOOSE PHOTO"; // to constants
			tw.BackgroundColor = UIColor.Clear;
			tw.TextAlignment = UITextAlignment.Center;
			tw.Font = UIFont.SystemFontOfSize(17);

			NavigationItem.TitleView = tw;

			var leftBarButton = new UIBarButtonItem(UIImage.FromFile("small_camera"), UIBarButtonItemStyle.Plain, SwitchSource);
			NavigationItem.SetLeftBarButtonItem(leftBarButton, true);

			NavigationController.NavigationBar.TintColor = UIColor.White;
			NavigationController.NavigationBar.BarTintColor = UIColor.FromRGB(66, 165, 245); // To constants
		}

		public void SetupLiveCameraStream()
		{
			captureSession = new AVCaptureSession();

			var viewLayer = liveCameraStream.Layer;

			var videoPreviewLayer = new AVCaptureVideoPreviewLayer(captureSession)
			{
				Frame = new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Width) //liveCameraStream.Frame
			};
			videoPreviewLayer.VideoGravity = AVLayerVideoGravity.ResizeAspectFill;
			liveCameraStream.Layer.AddSublayer(videoPreviewLayer);

			var captureDevice = AVCaptureDevice.GetDefaultDevice(AVMediaType.Video);
			ConfigureCameraForDevice(captureDevice);
			captureDeviceInput = AVCaptureDeviceInput.FromDevice(captureDevice);
			captureSession.AddInput(captureDeviceInput);

			var dictionary = new NSMutableDictionary();
			dictionary[AVVideo.CodecKey] = new NSNumber((int)AVVideoCodec.JPEG);
			stillImageOutput = new AVCaptureStillImageOutput()
			{
				OutputSettings = new NSDictionary()
			};

			captureSession.AddOutput(stillImageOutput);
			captureSession.StartRunning();
		}

		void ConfigureCameraForDevice(AVCaptureDevice device)
		{
			var error = new NSError();
			if (device.IsFocusModeSupported(AVCaptureFocusMode.ContinuousAutoFocus))
			{
				device.LockForConfiguration(out error);
				device.FocusMode = AVCaptureFocusMode.ContinuousAutoFocus;
				device.UnlockForConfiguration();
			}
			else if (device.IsExposureModeSupported(AVCaptureExposureMode.ContinuousAutoExposure))
			{
				device.LockForConfiguration(out error);
				device.ExposureMode = AVCaptureExposureMode.ContinuousAutoExposure;
				device.UnlockForConfiguration();
			}
			else if (device.IsWhiteBalanceModeSupported(AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance))
			{
				device.LockForConfiguration(out error);
				device.WhiteBalanceMode = AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance;
				device.UnlockForConfiguration();
			}
		}

		private void PhotoPicked(NSIndexPath indexPath)
		{
			var collectionCell = (PhotoCollectionViewCell)photoCollection.CellForItem(indexPath);

			if (collectionCell.Asset != null)
			{
				using (var m = new PHImageManager())
				{
					m.RequestImageData(collectionCell.Asset, new PHImageRequestOptions(), (data, dataUti, orientation, info) =>
					   {
						   if (data != null)
						   {
							   var photo = UIImage.LoadFromData(data);
							   UIImage cropped = NormalizeImage(photo);
							   GoToDescription(cropped);
						   }
					   });
				}
			}
		}

		private void SwitchCameraButtonTapped(object sender, EventArgs e)
		{
			var devicePosition = captureDeviceInput.Device.Position;
			if (devicePosition == AVCaptureDevicePosition.Front)
			{
				devicePosition = AVCaptureDevicePosition.Back;
			}
			else
			{
				devicePosition = AVCaptureDevicePosition.Front;
			}

			var device = GetCameraForOrientation(devicePosition);
			ConfigureCameraForDevice(device);

			captureSession.BeginConfiguration();
			captureSession.RemoveInput(captureDeviceInput);
			captureDeviceInput = AVCaptureDeviceInput.FromDevice(device);
			captureSession.AddInput(captureDeviceInput);
			captureSession.CommitConfiguration();
		}

		public AVCaptureDevice GetCameraForOrientation(AVCaptureDevicePosition orientation)
		{
			var devices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video);
			foreach (var device in devices)
			{
				if (device.Position == orientation)
				{
					return device;
				}
			}

			return null;
		}

		private void GoToDescription(UIImage image)
		{
			var descriptionViewController = Storyboard.InstantiateViewController("DescriptionViewController") as DescriptionViewController;
			descriptionViewController.ImageAsset = image;
			TabBarController.NavigationController.PushViewController(descriptionViewController, true);
		}
	}

	public class CollectionViewFlowDelegate : UICollectionViewDelegateFlowLayout
	{
		Action ScrolledAction;
		Action<NSIndexPath> CellClick;
		public bool isGrid = true;
		List<NSMutableAttributedString> _commentString;

		public CollectionViewFlowDelegate(Action<NSIndexPath> cellClick = null, Action scrolled = null, List<NSMutableAttributedString> commentString = null)
		{
			ScrolledAction = scrolled;
			CellClick = cellClick;
			_commentString = commentString;
		}

		public override void Scrolled(UIScrollView scrollView)
		{
			if (ScrolledAction != null)
			{
				ScrolledAction();
			}
		}

		public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
		{
			if (!isGrid)
				return;

			if (CellClick != null)
				CellClick(indexPath);
		}

		public override CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
		{
			if (!isGrid)
			{
				//54 - margins sum
				var textSize = _commentString[indexPath.Row].GetBoundingRect(new CGSize(UIScreen.MainScreen.Bounds.Width - 54, 1000), NSStringDrawingOptions.UsesLineFragmentOrigin, null);
				//165 => 485-320 cell height without image size
				var cellHeight = 165 + UIScreen.MainScreen.Bounds.Width;
				return new CGSize(UIScreen.MainScreen.Bounds.Width, cellHeight + textSize.Size.Height);
			}
			return Constants.CellSize;//CGSize(UIScreen.MainScreen.Bounds.Width, cellHeight + textSize.Size.Height);
		}
	}
}
