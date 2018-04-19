using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using ImageIO;
using Photos;
using Steepshot.Core.Models.Enums;
using Steepshot.iOS.Cells;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class PhotoPreviewViewController : BaseViewController
    {
        private readonly PHImageManager _m;
        private UIScrollView _cropView;
        private PhotoCollectionViewSource source;
        private PhotoCollectionViewFlowDelegate delegateP;
        private UIImageView imageView;
        private CGSize originalImageSize;
        private string previousPhotoLocalIdentifier;
        private Tuple<NSIndexPath, PHAsset> pickedPhoto;
        private UIImageOrientation orientation = UIImageOrientation.Up;

        public PhotoPreviewViewController()
        {
            _m = new PHImageManager();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var rotateTap = new UITapGestureRecognizer(RotateTap);
            rotate.AddGestureRecognizer(rotateTap);

            var zoomTap = new UITapGestureRecognizer(ZoomTap);
            resize.AddGestureRecognizer(zoomTap);

            var multiselectTap = new UITapGestureRecognizer(MultiSelectTap);
            multiSelect.AddGestureRecognizer(multiselectTap);

            bottomArrow.Transform = CGAffineTransform.MakeRotation((float)(Math.PI));

            source = new PhotoCollectionViewSource();
            photoCollection.Source = source;
            photoCollection.RegisterClassForCell(typeof(PhotoCollectionViewCell), nameof(PhotoCollectionViewCell));

            photoCollection.SetCollectionViewLayout(new UICollectionViewFlowLayout()
            {
                ItemSize = Constants.CellSize,
                MinimumLineSpacing = 1,
                MinimumInteritemSpacing = 1,
            }, false);

            delegateP = new PhotoCollectionViewFlowDelegate(source);
            photoCollection.Delegate = delegateP;

            delegateP.CellClicked += CellAction;

            imageView = new UIImageView(new CGRect(0, 0, 375, 375));
            imageView.ContentMode = UIViewContentMode.ScaleAspectFit;

            _cropView = new UIScrollView(new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Width));
            _cropView.Bounces = false;
            _cropView.MinimumZoomScale = 1f;
            _cropView.MaximumZoomScale = 4f;
            _cropView.ViewForZoomingInScrollView += (UIScrollView sv) => { return imageView; };
            _cropView.AddSubview(imageView);
            _cropView.ContentSize = new CGSize(375, 375);
            _cropView.DidZoom += (t, u) =>
            {
                SetScrollViewInsets();
            };
            cropBackgroundView.AddSubview(_cropView);
            NavigationController.NavigationBar.Translucent = false;
            SetBackButton();
        }

        private void CellAction(ActionType type, Tuple<NSIndexPath, PHAsset> photo)
        {
            if (type == ActionType.Close)
            {
                ShowAlert(Core.Localization.LocalizationKeys.PickedPhotosLimit);
                return;
            }
            NavigationItem.RightBarButtonItem.Enabled = false;
            pickedPhoto = photo;
            previousPhotoLocalIdentifier = source.CurrentlySelectedItem?.Item2?.LocalIdentifier;
            var pickOptions = new PHImageRequestOptions() { ResizeMode = PHImageRequestOptionsResizeMode.Exact, DeliveryMode = PHImageRequestOptionsDeliveryMode.HighQualityFormat };
            var imageSize = ImageHelper.CalculateInSampleSize(new CGSize(photo.Item2.PixelWidth, photo.Item2.PixelHeight), 1200, 1200);
            _m.RequestImageForAsset(photo.Item2, imageSize, PHImageContentMode.Default, pickOptions, PickImage);
        }

        private void PickImage(UIImage img, NSDictionary info)
        {
            var previousZoomScale = _cropView.ZoomScale;
            var previousOffset = _cropView.ContentOffset;
            var previousOriginalSize = originalImageSize;
            var previousOrientation = orientation;

            var currentPhoto = source.ImageAssets.FirstOrDefault(a => a.Asset.LocalIdentifier == pickedPhoto.Item2.LocalIdentifier);

            if (currentPhoto?.Orientation != null && currentPhoto?.Orientation != UIImageOrientation.Up)
            {
                currentPhoto.Image = img = ImageHelper.RotateImage(img, currentPhoto.Orientation);
                orientation = currentPhoto.Orientation;
            }
            else
                orientation = UIImageOrientation.Up;
            AdjustImageViewSize(img);

            imageView.Image = img;

            if (source.MultiPickMode)
            {
                if (previousPhotoLocalIdentifier != pickedPhoto.Item2.LocalIdentifier || currentPhoto == null)
                {
                    var lastPhoto = source.ImageAssets.FirstOrDefault(a => a.Asset.LocalIdentifier == previousPhotoLocalIdentifier);
                    if (lastPhoto != null)
                    {
                        lastPhoto.Offset = previousOffset;
                        lastPhoto.Scale = previousZoomScale;
                        lastPhoto.OriginalImageSize = previousOriginalSize;
                        lastPhoto.Orientation = previousOrientation;
                    }

                    if (currentPhoto == null)
                    {
                        ApplyRightScale();
                        SetScrollViewInsets();
                        source.ImageAssets.Add(new SavedPhoto(pickedPhoto.Item2, img, _cropView.ContentOffset));
                    }
                    else
                    {
                        ApplyRightScale((float)currentPhoto.Scale);
                        SetScrollViewInsets();
                        _cropView.SetContentOffset(currentPhoto.Offset, false);
                    }
                }
                else
                {
                    if (source.ImageAssets.Count != 1)
                        source.ImageAssets.RemoveAll(a => a.Asset.LocalIdentifier == pickedPhoto.Item2.LocalIdentifier);
                    ApplyRightScale();
                    SetScrollViewInsets();
                }

                photoCollection.ReloadData();
            }
            else
            {
                ApplyCriticalScale();
                if (source.ImageAssets.Count == 0)
                    source.ImageAssets.Add(new SavedPhoto(pickedPhoto.Item2, img, _cropView.ContentOffset));
                else
                    source.ImageAssets[0] = new SavedPhoto(pickedPhoto.Item2, img, _cropView.ContentOffset);
                SetScrollViewInsets();
            }
            NavigationItem.RightBarButtonItem.Enabled = true;
        }

        private void AdjustImageViewSize(UIImage img)
        {
            var w = img.Size.Width / 1200f * UIScreen.MainScreen.Bounds.Width;
            var h = img.Size.Height / 1200f * UIScreen.MainScreen.Bounds.Width;
            originalImageSize = new CGSize(w, h);

            if (originalImageSize.Width < UIScreen.MainScreen.Bounds.Width && originalImageSize.Height < UIScreen.MainScreen.Bounds.Width)
            {
                originalImageSize = ImageHelper.CalculateInSampleSize(originalImageSize,
                                                          (float)UIScreen.MainScreen.Bounds.Width,
                                                          (float)UIScreen.MainScreen.Bounds.Width, true);
            }

            _cropView.ContentInset = new UIEdgeInsets(0, 0, 0, 0);
            _cropView.MinimumZoomScale = 1;
            _cropView.ZoomScale = 1;
            _cropView.ContentSize = originalImageSize;
            imageView.Frame = new CGRect(new CGPoint(0, 0), originalImageSize);
        }

        private void ApplyCriticalScale()
        {
            nfloat scale = 1;

            var imageRatio = originalImageSize.Width / originalImageSize.Height;

            if (originalImageSize.Height > originalImageSize.Width)
            {
                if (imageRatio < 0.8f)
                    scale = 0.8f / imageRatio;
            }
            else
                if (imageRatio > 1.92f)
                scale = imageRatio / 1.92f;

            _cropView.MinimumZoomScale = scale;
            _cropView.SetZoomScale(scale, false);
        }

        private void ApplyRightScale(float zoom = float.MinValue)
        {
            nfloat scale = 0;

            if (originalImageSize.Height > originalImageSize.Width && _cropView.Frame.Height <= originalImageSize.Height)
                scale = _cropView.Frame.Width / originalImageSize.Width;

            if (originalImageSize.Height < originalImageSize.Width && _cropView.Frame.Width <= originalImageSize.Width)
                scale = _cropView.Frame.Height / originalImageSize.Height;

            if (scale > 0)
            {
                _cropView.MinimumZoomScale = scale;
                _cropView.SetZoomScale(scale, false);
            }
            else
                ApplyCriticalScale();

            if (zoom != float.MinValue)
            {
                _cropView.SetZoomScale(zoom, false);
                return;
            }
        }

        private void SetScrollViewInsets()
        {
            nfloat shift;

            var shiftSide = originalImageSize.Height < originalImageSize.Width;
            if (shiftSide)
                shift = _cropView.Frame.Height / 2.0f - _cropView.ContentSize.Height / 2.0f;
            else
                shift = _cropView.Frame.Width / 2.0f - _cropView.ContentSize.Width / 2.0f;

            if (shift > 0)
                if (shiftSide)
                    _cropView.ContentInset = new UIEdgeInsets(shift, 0, 0, 0);
                else
                    _cropView.ContentInset = new UIEdgeInsets(0, shift, 0, 0);
            else
                _cropView.ContentInset = new UIEdgeInsets(0, 0, 0, 0);
        }

        private void ZoomTap()
        {
            UIView.Animate(0.15,() => {
                if (topArrow.Transform.xx == 1)
                {
                    topArrow.Transform = CGAffineTransform.MakeRotation((float)(Math.PI));
                    bottomArrow.Transform = CGAffineTransform.MakeRotation(0);
                }
                else
                {
                    topArrow.Transform = CGAffineTransform.MakeRotation(0);
                    bottomArrow.Transform = CGAffineTransform.MakeRotation((float)(Math.PI));
                }
            });

            if (originalImageSize.Width < originalImageSize.Height)
            {
                if (_cropView.ZoomScale != _cropView.Frame.Width / originalImageSize.Width)
                    _cropView.SetZoomScale(_cropView.Frame.Width / originalImageSize.Width, true);
                else
                    _cropView.SetZoomScale(1f, true);
            }
            else
                if (_cropView.ZoomScale != _cropView.Frame.Height / originalImageSize.Height)
                _cropView.SetZoomScale(_cropView.Frame.Height / originalImageSize.Height, true);
            else
                _cropView.SetZoomScale(1f, true);
        }

        private void MultiSelectTap()
        {
            source.MultiPickMode = !source.MultiPickMode;
            if (source.MultiPickMode)
            {
                multiSelect.Image = UIImage.FromBundle("ic_multiselect_active");
                if (imageView.Frame.Width < _cropView.Frame.Width)
                    _cropView.Frame = new CGRect((_cropView.Frame.Width - imageView.Frame.Width) / 2, _cropView.Frame.Location.Y, imageView.Frame.Width, _cropView.Frame.Height);
                if (imageView.Frame.Height < _cropView.Frame.Height)
                    _cropView.Frame = new CGRect(_cropView.Frame.Location.X, (_cropView.Frame.Height - imageView.Frame.Height) / 2, _cropView.Frame.Width, imageView.Frame.Height);

                ApplyRightScale();
                SetScrollViewInsets();
            }
            else
            {
                multiSelect.Image = UIImage.FromBundle("ic_multiselect");
                _cropView.Frame = new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Width);
                source.ImageAssets.Clear();
                delegateP.ItemSelected(photoCollection, source.CurrentlySelectedItem.Item1);
            }
            photoCollection.ReloadData();
        }

        public override void ViewDidAppear(bool animated)
        {
            if (IsMovingToParentViewController)
                delegateP.ItemSelected(photoCollection, NSIndexPath.FromItemSection(0, 0));
        }

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            var rotatedButton = new UIImage(leftBarButton.Image.CGImage, leftBarButton.Image.CurrentScale, UIImageOrientation.UpMirrored);
            var rightBarButton = new UIBarButtonItem(rotatedButton, UIBarButtonItemStyle.Plain, GoForward);
            rightBarButton.Enabled = false;
            NavigationItem.LeftBarButtonItem = leftBarButton;
            NavigationItem.RightBarButtonItem = rightBarButton;
            NavigationController.NavigationBar.TintColor = Helpers.Constants.R15G24B30;
            NavigationItem.Title = "Photo preview";
        }

        private void GoBack(object sender, EventArgs e)
        {
            NavigationController.PopViewController(true);
        }

        private void GoForward(object sender, EventArgs e)
        {
            var croppedPhotos = new List<Tuple<NSDictionary, UIImage>>();

            var currentPhoto = source.ImageAssets.FirstOrDefault(a => a.Asset.LocalIdentifier == source.CurrentlySelectedItem.Item2.LocalIdentifier);
            if (currentPhoto != null)
            {
                currentPhoto.Offset = _cropView.ContentOffset;
                currentPhoto.Scale = _cropView.ZoomScale;
                currentPhoto.OriginalImageSize = originalImageSize;
                currentPhoto.Orientation = orientation;
            }

            foreach (var item in source.ImageAssets)
            {
                NSDictionary metadata = null;
                var croppedPhoto = CropImage(item);
                _m.RequestImageData(item.Asset, new PHImageRequestOptions() { Synchronous = true }, (data, dataUti, orientation, info) =>
                {
                    var dataSource = CGImageSource.FromData(data);
                    metadata = dataSource.GetProperties(0).Dictionary;
                });

                croppedPhotos.Add(new Tuple<NSDictionary, UIImage>(metadata, croppedPhoto));
            }

            var descriptionViewController = new DescriptionViewController(croppedPhotos, "jpg");
            NavigationController.PushViewController(descriptionViewController, true);
        }

        private void RotateTap()
        {
            UIView.Animate(0.15, () =>
            {
                rotate.Alpha = 0.6f;
            }, () =>
            {
                UIView.Animate(0.15, () =>
                {
                    rotate.Alpha = 1f;
                }, null);
            });

            imageView.Image = ImageHelper.RotateImage(imageView.Image, UIImageOrientation.Right);
            SaveOrientation();
            AdjustImageViewSize(imageView.Image);
            if (source.MultiPickMode)
            {
                ApplyRightScale();
                var currentPhoto = source.ImageAssets.FirstOrDefault(a => a.Asset.LocalIdentifier == source.CurrentlySelectedItem.Item2.LocalIdentifier);
                if (currentPhoto != null)
                {
                    currentPhoto.Offset = _cropView.ContentOffset;
                    currentPhoto.Scale = _cropView.ZoomScale;
                    currentPhoto.OriginalImageSize = originalImageSize;
                    currentPhoto.Orientation = orientation;
                    currentPhoto.Image = imageView.Image;
                }
            }
            else
            {
                source.ImageAssets[0].Image = imageView.Image;
                ApplyCriticalScale();
            }
            SetScrollViewInsets();
        }

        private void SaveOrientation()
        {
            switch (orientation)
            {
                case UIImageOrientation.Up:
                    orientation = UIImageOrientation.Right;
                    break;
                case UIImageOrientation.Right:
                    orientation = UIImageOrientation.Down;
                    break;
                case UIImageOrientation.Down:
                    orientation = UIImageOrientation.Left;
                    break;
                case UIImageOrientation.Left:
                    orientation = UIImageOrientation.Up;
                    break;
                default:
                    orientation = UIImageOrientation.Up;
                    break;
            }
        }

        private UIImage CropImage(SavedPhoto photo)
        {
            CGSize scaledImageSize;
            CGPoint offset;

            if (photo.OriginalImageSize.Width == 0 && photo.OriginalImageSize.Height == 0)
            {
                scaledImageSize = imageView.Frame.Size;
                offset = _cropView.ContentOffset;
            }
            else
            {
                scaledImageSize = new CGSize(photo.OriginalImageSize.Width * photo.Scale, photo.OriginalImageSize.Height * photo.Scale);
                offset = photo.Offset;
            }

            var ratio2 = photo.Image.Size.Width / scaledImageSize.Width;

            nfloat cropWidth;
            nfloat cropHeight;
            nfloat cropX;
            nfloat cropY;

            if (scaledImageSize.Width > _cropView.Frame.Width)
            {
                cropWidth = _cropView.Frame.Width * ratio2;
            }
            else
            {
                cropWidth = imageView.Frame.Width * ratio2;
            }

            if (scaledImageSize.Height > _cropView.Frame.Height)
            {
                cropHeight = _cropView.Frame.Height * ratio2;
            }
            else
            {
                cropHeight = scaledImageSize.Height * ratio2;
            }

            if (offset.X < 0)
            {
                cropX = 0;
            }
            else
            {
                cropX = offset.X * ratio2;
            }

            if (offset.Y < 0)
            {
                cropY = 0;
            }
            else
            {
                cropY = offset.Y * ratio2;
            }

            var rect = new CGRect(cropX, cropY, cropWidth, cropHeight);

            UIImage cropped = new UIImage();

            using (var cr = photo.Image.CGImage.WithImageInRect(rect))
            {
                cropped = UIImage.FromImage(cr);
            }

            var newSize = ImageHelper.CalculateInSampleSize(cropped.Size, 1200, 1200, true);

            UIGraphics.BeginImageContextWithOptions(newSize, false, 1);
            cropped.Draw(new CGRect(new CGPoint(0, 0), newSize));
            cropped = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return cropped;
        }
    }
}
