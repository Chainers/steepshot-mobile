using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
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
            previousPhotoLocalIdentifier = source.CurrentlySelectedItem?.Item2?.LocalIdentifier;

            _m.RequestImageForAsset(photo.Item2, CalculateInSampleSize(new CGSize(photo.Item2.PixelWidth, photo.Item2.PixelHeight), 1200, 1200),
                                    PHImageContentMode.Default, new PHImageRequestOptions() { ResizeMode = PHImageRequestOptionsResizeMode.Exact, DeliveryMode = PHImageRequestOptionsDeliveryMode.HighQualityFormat }, (img, info) =>
                                                 {
                                                     var w = img.Size.Width / 1200f * UIScreen.MainScreen.Bounds.Width;
                                                     var h = img.Size.Height / 1200f * UIScreen.MainScreen.Bounds.Width;

                                                     var previousZoomScale = _cropView.ZoomScale;
                                                     var previousOffset = _cropView.ContentOffset;

                                                     _cropView.ContentInset = new UIEdgeInsets(0, 0, 0, 0);
                                                     _cropView.MinimumZoomScale = 1;
                                                     _cropView.ZoomScale = 1;
                                                     _cropView.SetContentOffset(new CGPoint(), false);
                                                     _cropView.ContentSize = new CGSize(w, h);
                                                     imageView.Frame = new CGRect(0, 0, w, h);

                                                     imageView.Image = img;

                                                     originalImageSize = new CGSize(w, h);

                                                     if (source.MultiPickMode)
                                                     {
                                                         var currentPhoto = source.ImageAssets.FirstOrDefault(a => a.Id == photo.Item2.LocalIdentifier);
                                                         if (previousPhotoLocalIdentifier != photo.Item2.LocalIdentifier || currentPhoto == null)
                                                         {
                                                             var lastPhoto = source.ImageAssets.FirstOrDefault(a => a.Id == previousPhotoLocalIdentifier);
                                                             if (lastPhoto != null)
                                                             {
                                                                 lastPhoto.Offset = previousOffset;
                                                                 lastPhoto.Scale = previousZoomScale;
                                                             }
                                                             
                                                             if (currentPhoto == null)
                                                             {
                                                                 ApplyRightScale();
                                                                 SetScrollViewInsets();
                                                                 source.ImageAssets.Add(new SavedPhoto(photo.Item2.LocalIdentifier, img, _cropView.ContentOffset));

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
                                                             source.ImageAssets.RemoveAll(a => a.Id == photo.Item2.LocalIdentifier);
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
                                                             source.ImageAssets.Add(new SavedPhoto(photo.Item2.LocalIdentifier, img, _cropView.ContentOffset));
                                                         else
                                                             source.ImageAssets[0] = new SavedPhoto(photo.Item2.LocalIdentifier, img, _cropView.ContentOffset);
                                                         SetScrollViewInsets();
                                                     }
                                                 });
        }

        private void CropPhoto()
        {
            //RotateImage(source.ImageAssets[0].Image);
            /*
            foreach (var item in collection)
            {

            }*/
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
                _cropView.SetNeedsLayout();
                _cropView.LayoutIfNeeded();
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
            var image = RotateImage(source.ImageAssets[0]);
            var descriptionViewController = new DescriptionViewController(image, "jpg", _metadata);
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
            /*
            UIImageOrientation orientation;

            switch (photoView.Image.Orientation)
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
            RotateImage(orientation);*/
        }

        private CGSize CalculateInSampleSize(CGSize imageSize, float reqWidth, float reqHeight)
        {
            var height = (float)imageSize.Height;
            var width = (float)imageSize.Width;
            var inSampleSize = 1f;
            if (height > reqHeight)
            {
                inSampleSize = reqHeight / height;
            }
            if (width > reqWidth)
            {
                inSampleSize = Math.Min(inSampleSize, reqWidth / width);
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

        private UIImage RotateImage(SavedPhoto photo)
        {
            //return image.CGImage.WithImageInRect().CreateResizableImage(new UIEdgeInsets(0, 186, 0, 1200 - 314 - 186));

            using (var cr = photo.Image.CGImage.WithImageInRect(new CGRect(photo.Offset.X, photo.OffsetY, photo.Image.Size.Height, photo.Image.Size.Height)))
            {
                var cropped = UIImage.FromImage(cr);
                return cropped;
            }

            /*
            var rotated = new UIImage(image.CGImage, image.CurrentScale, image.Orientation);
            UIGraphics.BeginImageContextWithOptions(rotated.Size, false, rotated.CurrentScale);
            var drawRect = new CGRect(160, 0, 314, 314);
            rotated.Draw(drawRect);
            image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return image;*/
        }
    }
}
