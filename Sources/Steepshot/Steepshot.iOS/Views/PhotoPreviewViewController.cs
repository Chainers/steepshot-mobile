using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
        private UIDeviceOrientation _rotation;
        private NSDictionary _metadata;
        private readonly PHImageManager _m;
        private UIScrollView _cropView;

        public PhotoPreviewViewController(UIImage imageAsset, UIDeviceOrientation rotation, NSDictionary metadata)
        {
            //ImageAssets = imageAsset;
            _rotation = rotation;
            _metadata = metadata;
            _m = new PHImageManager();
        }

        PhotoCollectionViewSource source;
        PhotoCollectionViewFlowDelegate delegateP;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var rotateTap = new UITapGestureRecognizer(ZoomTap);
            rotate.AddGestureRecognizer(rotateTap);

            var multiselectTap = new UITapGestureRecognizer(MultiSelectTap);
            multiSelect.AddGestureRecognizer(multiselectTap);

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
            //cropView.ContentSize = imageView.Image.Size;
            _cropView.AddSubview(imageView);
            _cropView.ContentSize = new CGSize(375, 375);
            _cropView.DidZoom += (t, u) =>
            {
                SetScrollViewInsets();
            };
            cropBackgroundView.AddSubview(_cropView);

            View.BringSubviewToFront(rotate);

            NavigationController.NavigationBar.Translucent = false;
            SetBackButton();
        }

        private UIImageView imageView;

        private CGSize originalImageSize;

        string previousPhotoLocalIdentifier;

        private void CellAction(ActionType type, Tuple<NSIndexPath, PHAsset> photo)
        {
            if (type == ActionType.Close)
            {
                //change text after merge
                ShowAlert(Core.Localization.LocalizationKeys.Error);
                return;
            }

            previousPhotoLocalIdentifier = source.CurrentlySelectedItem?.Item2?.LocalIdentifier;

            _m.RequestImageForAsset(photo.Item2, CalculateInSampleSize(new CGSize(photo.Item2.PixelWidth, photo.Item2.PixelHeight), 1200, 1200),
                                    PHImageContentMode.Default, new PHImageRequestOptions() { ResizeMode = PHImageRequestOptionsResizeMode.Exact, DeliveryMode = PHImageRequestOptionsDeliveryMode.HighQualityFormat }, (img, info) =>
                                                 {
                                                     var w = img.Size.Width / 1200f * UIScreen.MainScreen.Bounds.Width;
                                                     var h = img.Size.Height / 1200f * UIScreen.MainScreen.Bounds.Width;

                                                     var previousZoomScale = _cropView.ZoomScale;
                                                     var previousOffset = _cropView.ContentOffset;
                                                     var previousOriginalSize = originalImageSize;

                                                     originalImageSize = new CGSize(w, h);

                                                     if (originalImageSize.Width < UIScreen.MainScreen.Bounds.Width && originalImageSize.Height < UIScreen.MainScreen.Bounds.Width)
                                                     {
                                                         originalImageSize = CalculateInSampleSize(originalImageSize,
                                                                                                   (float)UIScreen.MainScreen.Bounds.Width,
                                                                                                   (float)UIScreen.MainScreen.Bounds.Width, true);
                                                     }

                                                     _cropView.ContentInset = new UIEdgeInsets(0, 0, 0, 0);
                                                     _cropView.MinimumZoomScale = 1;
                                                     _cropView.ZoomScale = 1;
                                                     _cropView.SetContentOffset(new CGPoint(), false);
                                                     _cropView.ContentSize = originalImageSize;
                                                     imageView.Frame = new CGRect(new CGPoint(0, 0), originalImageSize);

                                                     imageView.Image = img;

                                                     if (source.MultiPickMode)
                                                     {
                                                         var currentPhoto = source.ImageAssets.FirstOrDefault(a => a.Asset.LocalIdentifier == photo.Item2.LocalIdentifier);
                                                         if (previousPhotoLocalIdentifier != photo.Item2.LocalIdentifier || currentPhoto == null)
                                                         {
                                                             var lastPhoto = source.ImageAssets.FirstOrDefault(a => a.Asset.LocalIdentifier == previousPhotoLocalIdentifier);
                                                             if (lastPhoto != null)
                                                             {
                                                                 lastPhoto.Offset = previousOffset;
                                                                 lastPhoto.Scale = previousZoomScale;
                                                                 lastPhoto.OriginalImageSize = previousOriginalSize;
                                                             }

                                                             if (currentPhoto == null)
                                                             {
                                                                 ApplyRightScale();
                                                                 SetScrollViewInsets();
                                                                 source.ImageAssets.Add(new SavedPhoto(photo.Item2, img, _cropView.ContentOffset));
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
                                                             //source.CurrentlySelectedItem = new Tuple<NSIndexPath, PHAsset>(null, null);
                                                             source.ImageAssets.RemoveAll(a => a.Asset.LocalIdentifier == photo.Item2.LocalIdentifier);
                                                             ApplyRightScale();
                                                             SetScrollViewInsets();
                                                         }

                                                         photoCollection.ReloadData();
                                                     }
                                                     else
                                                     {
                                                         ApplyCriticalScale();
                                                         SetScrollViewInsets();
                                                         if (source.ImageAssets.Count == 0)
                                                             source.ImageAssets.Add(new SavedPhoto(photo.Item2, img, _cropView.ContentOffset));
                                                         else
                                                             source.ImageAssets[0] = new SavedPhoto(photo.Item2, img, _cropView.ContentOffset);
                                                         SetScrollViewInsets();
                                                     }
                                                 });
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
                if (imageRatio > 1.91f)
                scale = imageRatio / 1.91f;

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
            {
                delegateP.ItemSelected(photoCollection, NSIndexPath.FromItemSection(0, 0));
                //photoCollection.SelectItem(NSIndexPath.FromIndex(0), false, UICollectionViewScrollPosition.Top);
                //CellAction(ActionType.Preview, source.GetPHAsset(0));
                //ImageAsset = photoView.Image = await NormalizeImage(ImageAsset);
                RotatePhotoIfNeeded();
            }
        }

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            var rotatedButton = new UIImage(leftBarButton.Image.CGImage, leftBarButton.Image.CurrentScale, UIImageOrientation.UpMirrored);
            var rightBarButton = new UIBarButtonItem(rotatedButton, UIBarButtonItemStyle.Plain, GoForward);
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

        private void RotatePhotoIfNeeded()
        {
            if (_rotation == UIDeviceOrientation.Portrait || _rotation == UIDeviceOrientation.Unknown)
                return;

            UIImageOrientation orientation;

            switch (_rotation)
            {
                case UIDeviceOrientation.Portrait:
                    orientation = UIImageOrientation.Up;
                    break;
                case UIDeviceOrientation.PortraitUpsideDown:
                    orientation = UIImageOrientation.Down;
                    break;
                case UIDeviceOrientation.LandscapeLeft:
                    orientation = UIImageOrientation.Left;
                    break;
                case UIDeviceOrientation.LandscapeRight:
                    orientation = UIImageOrientation.Right;
                    break;
                default:
                    orientation = UIImageOrientation.Up;
                    break;
            }
            RotateImage(orientation);
        }

        private void RotateTap()
        {
            UIImageOrientation orientation;

            switch (imageView.Image.Orientation)
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
            RotateImage(orientation);
        }

        private CGSize CalculateInSampleSize(CGSize imageSize, float reqWidth, float reqHeight, bool increase = false)
        {
            var height = (float)imageSize.Height;
            var width = (float)imageSize.Width;
            var inSampleSize = 1f;
            if (increase)
            {
                if (height < reqHeight)
                {
                    inSampleSize = reqHeight / height;
                }
                if (width < reqWidth)
                {
                    inSampleSize = Math.Min(inSampleSize, reqWidth / width);
                }
            }
            else
            {
                if (height > reqHeight)
                {
                    inSampleSize = reqHeight / height;
                }
                if (width > reqWidth)
                {
                    inSampleSize = Math.Min(inSampleSize, reqWidth / width);
                }
            }

            return new CGSize(width * inSampleSize, height * inSampleSize);
        }

        private async Task<UIImage> NormalizeImage(UIImage sourceImage)
        {
            return await Task.Run(() =>
            {
                var imgSize = sourceImage.Size;
                var inSampleSize = CalculateInSampleSize(sourceImage.Size, 1200, 1200);
                UIGraphics.BeginImageContextWithOptions(inSampleSize, false, sourceImage.CurrentScale);

                var drawRect = new CGRect(0, 0, inSampleSize.Width, inSampleSize.Height);
                sourceImage.Draw(drawRect);
                var modifiedImage = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();

                return modifiedImage;
            });
        }

        private void RotateImage(UIImageOrientation orientation)
        {
            var rotated = new UIImage();// new UIImage(photoView.Image.CGImage, photoView.Image.CurrentScale, orientation);
            UIGraphics.BeginImageContextWithOptions(rotated.Size, false, rotated.CurrentScale);
            var drawRect = new CGRect(0, 0, rotated.Size.Width, rotated.Size.Height);
            rotated.Draw(drawRect);
            //ImageAsset = photoView.Image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
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

            if(scaledImageSize.Width > _cropView.Frame.Width)
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

            //var horizontalRatio = 1200 / cropped.Size.Width;
            //var verticalRatio = 1200 / cropped.Size.Height;

            //var ratio = Math.Min(horizontalRatio, verticalRatio);
            var newSize = CalculateInSampleSize(cropped.Size, 1200, 1200, true); //new CGSize(cropped.Size.Width * ratio, cropped.Size.Height * ratio);

            UIGraphics.BeginImageContextWithOptions(newSize, false, 1);
            cropped.Draw(new CGRect(new CGPoint(0,0), newSize));
            cropped = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return cropped;
        }
    }
}
