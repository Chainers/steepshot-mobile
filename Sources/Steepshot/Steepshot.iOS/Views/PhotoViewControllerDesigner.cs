using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Photos;
using PureLayout.Net;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Enums;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class PhotoViewController
    {
        private void SetupCameraView()
        {
            View.BackgroundColor = Constants.R255G255B255;

            _liveCameraStream = new UIView();
            _liveCameraStream.Frame = new CGRect(0, 0, View.Frame.Width, View.Frame.Height);
            _liveCameraStream.UserInteractionEnabled = false;
            _liveCameraStream.TranslatesAutoresizingMaskIntoConstraints = false;
            _liveCameraStream.ContentMode = UIViewContentMode.ScaleToFill;
            View.AddSubview(_liveCameraStream);

            _closeButton = CreateRoundButton();
            _flashButton = CreateRoundButton();
            _flashButton.SetImage(UIImage.FromBundle("ic_flash"), UIControlState.Normal);
            _flashButton.BackgroundColor = UIColor.Black.ColorWithAlpha(0.8f);

            _enableCameraAccess = new UILabel();
            _enableCameraAccess.Font = Constants.Regular16;
            _enableCameraAccess.TextColor = Constants.R151G155B158;
            _enableCameraAccess.Text = AppDelegate.Localization.GetText(LocalizationKeys.EnableCameraAccess);
            _enableCameraAccess.TextAlignment = UITextAlignment.Center;
            _enableCameraAccess.UserInteractionEnabled = true;
            _enableCameraAccess.Hidden = true;

            View.AddSubview(_closeButton);
            View.AddSubview(_flashButton);
            View.AddSubview(_enableCameraAccess);

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

            _swapCameraButton = CreateRoundButton();
            View.AddSubview(_swapCameraButton);

            _shotButton = new UIButton();
            _shotButton.UserInteractionEnabled = true;
            _shotButton.Layer.CornerRadius = 30;
            _shotButton.Layer.BorderWidth = 2;
            ToggleShotButton(MediaType.Photo);
            View.AddSubview(_shotButton);

            _videoLoader.HidesWhenStopped = true;
            _shotButton.AddSubview(_videoLoader);

            _videoLoader.AutoCenterInSuperview();

            _closeButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);
            _closeButton.AutoPinEdgeToSuperviewEdge(ALEdge.Top, DeviceHelper.IsXDevice ? 64 : 20);
            _closeButton.AutoSetDimensionsToSize(new CGSize(40, 40));

            _flashButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 30);
            _flashButton.AutoPinEdgeToSuperviewEdge(ALEdge.Top, DeviceHelper.IsXDevice ? 64 : 20);
            _flashButton.AutoSetDimensionsToSize(new CGSize(40, 40));

            _enableCameraAccess.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _enableCameraAccess.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            _enableCameraAccess.AutoSetDimension(ALDimension.Height, 20);
            _enableCameraAccess.AutoAlignAxisToSuperviewAxis(ALAxis.Vertical);
            _enableCameraAccess.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);

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

            _shotButton.AutoSetDimensionsToSize(new CGSize(60, 60));
            _shotButton.AutoAlignAxisToSuperviewAxis(ALAxis.Vertical);
            _shotButton.AutoPinEdge(ALEdge.Bottom, ALEdge.Top, bottomSeparator, -bottomPanelHeight / 2);
            _activePanelHeight = (float)bottomPanelHeight - 60;

            _pointerView.LayoutIfNeeded();
            Constants.CreateGradient(_pointerView, 0, GradientType.Orange);
        }

        private UIButton CreateRoundButton()
        {
            var button = new UIButton
            {
                UserInteractionEnabled = true,
                ContentMode = UIViewContentMode.ScaleAspectFill
            };
            button.Layer.CornerRadius = 20;
            return button;
        }

        private void SwitchTheme(Theme theme)
        {
            var buttonBGColor = theme.Equals(Theme.Dark) ? UIColor.Black.ColorWithAlpha(0.8f) : UIColor.Black.ColorWithAlpha(0.05f);

            _swapCameraButton.SetImage(theme.Equals(Theme.Dark) ? UIImage.FromBundle("ic_revert") : UIImage.FromBundle("ic_revert_dark"), UIControlState.Normal);
            _swapCameraButton.BackgroundColor = buttonBGColor;

            _closeButton.SetImage(theme.Equals(Theme.Dark) ? UIImage.FromBundle("ic_white_close") : UIImage.FromBundle("ic_close_black"), UIControlState.Normal);
            _closeButton.BackgroundColor = buttonBGColor;

            _photoTabButton.SetTitleColor(theme.Equals(Theme.Dark) ? UIColor.White : UIColor.Black, UIControlState.Normal);
            _videoTabButton.SetTitleColor(theme.Equals(Theme.Dark) ? UIColor.White : UIColor.Black, UIControlState.Normal);

            _bottomPanel.Hidden = theme.Equals(Theme.Dark);
            _flashButton.Hidden = !theme.Equals(Theme.Dark);
        }

        private void SwitchMode(MediaType targetMode)
        {
            _photoTabButton.Enabled = targetMode != MediaType.Photo;
            _videoTabButton.Enabled = targetMode != MediaType.Video;
            _photoConstraint.Active = targetMode != MediaType.Video;
            _videoConstraint.Active = targetMode != MediaType.Photo;

            ToggleShotButton(targetMode);

            UIView.Animate(0.2, 0, UIViewAnimationOptions.CurveEaseOut, View.LayoutIfNeeded, null);

            _currentMode = targetMode;
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

        private void SetGalleryButton()
        {
            PHPhotoLibrary.RequestAuthorizationAsync().ContinueWith((status) =>
            {
                InvokeOnMainThread(() =>
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
            });
        }

        private void ToggleShotButton(MediaType cameraMode)
        {
            var normalColor = cameraMode.Equals(MediaType.Photo) ? Constants.R255G255B255 : Constants.R255G28B5;
            var highlightedColor = cameraMode.Equals(MediaType.Photo) ? Constants.R240G240B240 : Constants.R188G0B0;
            var normalCircle = CircleBorder(50, normalColor);
            var highlightedCircle = CircleBorder(50, highlightedColor);

            _shotButton.BackgroundColor = cameraMode.Equals(MediaType.Photo) ? Constants.R217G217B217 : UIColor.White;
            _shotButton.SetImage(normalCircle, UIControlState.Normal);
            _shotButton.SetImage(highlightedCircle, UIControlState.Disabled);
            _shotButton.SetImage(highlightedCircle, UIControlState.Highlighted);
            _shotButton.Layer.BorderColor = normalColor.CGColor;
        }

        private void ToggleButtons(bool isEnabled)
        {
            _shotButton.Enabled = isEnabled;
            _closeButton.Enabled = isEnabled;
            _swapCameraButton.Enabled = isEnabled;
            _photoTabButton.Enabled = isEnabled;
            _videoTabButton.Enabled = isEnabled;
            _galleryButton.UserInteractionEnabled = isEnabled;
        }
    }
}
