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
            cropView.Bounces = false;
            cropView.MinimumZoomScale = 1f;
            cropView.MaximumZoomScale = 4f;
            cropView.ViewForZoomingInScrollView += (UIScrollView sv) => { return imageView; };
            //cropView.ContentSize = imageView.Image.Size;
            cropView.AddSubview(imageView);
            cropView.ContentSize = new CGSize(375, 375);

            cropView.DidZoom += (t, u) =>
            {
                SetScrollViewInsets();
            };

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

                                                     var previousZoomScale = cropView.ZoomScale;
                                                     var previousOffset = cropView.ContentOffset;

                                                     cropView.ContentInset = new UIEdgeInsets(0, 0, 0, 0);
                                                     cropView.MinimumZoomScale = 1;
                                                     cropView.ZoomScale = 1;
                                                     cropView.SetContentOffset(new CGPoint(), false);
                                                     cropView.ContentSize = new CGSize(w, h);
                                                     imageView.Frame = new CGRect(0, 0, w, h);

                                                     imageView.Image = img;

                                                     originalImageSize = new CGSize(w, h);

                                                     if (source.MultiPickMode)
                                                     {
                                                         if (previousPhotoLocalIdentifier != photo.Item2.LocalIdentifier)
                                                         {
                                                             var lastPhoto = source.ImageAssets.FirstOrDefault(a => a.Id == previousPhotoLocalIdentifier);
                                                             if (lastPhoto != null)
                                                             {
                                                                 lastPhoto.Offset = previousOffset;
                                                                 lastPhoto.Scale = previousZoomScale;
                                                             }

                                                             var currentPhoto = source.ImageAssets.FirstOrDefault(a => a.Id == photo.Item2.LocalIdentifier);
                                                             if (currentPhoto == null)
                                                             {
                                                                 ApplyRightScale();
                                                                 SetScrollViewInsets();
                                                                 source.ImageAssets.Add(new SavedPhoto(photo.Item2.LocalIdentifier, img, cropView.ContentOffset));

                                                             }
                                                             else
                                                             {
                                                                 ApplyRightScale((float)currentPhoto.Scale);
                                                                 SetScrollViewInsets();
                                                                 cropView.SetContentOffset(currentPhoto.Offset, false);
                                                             }
                                                         }
                                                         else
                                                             source.ImageAssets.Remove(source.ImageAssets.First(a => a.Id == photo.Item2.LocalIdentifier));

                                                         photoCollection.ReloadData();
                                                     }
                                                     else
                                                     {
                                                         ApplyCriticalScale();
                                                         SetScrollViewInsets();
                                                         if (source.ImageAssets.Count == 0)
                                                             source.ImageAssets.Add(new SavedPhoto(photo.Item2.LocalIdentifier, img, cropView.ContentOffset));
                                                         else
                                                             source.ImageAssets[0] = new SavedPhoto(photo.Item2.LocalIdentifier, img, cropView.ContentOffset);
                                                     }
                                                 });
        }

        private void CropPhoto()
        {

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

            cropView.MinimumZoomScale = scale;
            cropView.SetZoomScale(scale, false);
        }

        private void ApplyRightScale(float zoom = float.MinValue)
        {
            nfloat scale = 0;

            if (originalImageSize.Height > originalImageSize.Width && cropView.Frame.Height <= originalImageSize.Height)
                scale = cropView.Frame.Width / originalImageSize.Width;

            if (originalImageSize.Height < originalImageSize.Width && cropView.Frame.Width <= originalImageSize.Width)
                scale = cropView.Frame.Height / originalImageSize.Height;

            if (scale != 0)
            {
                cropView.MinimumZoomScale = scale;
                cropView.SetZoomScale(scale, false);
            }
            else
                ApplyCriticalScale();

            if (zoom != float.MinValue)
            {
                cropView.SetZoomScale(zoom, false);
                return;
            }
        }

        private void SetScrollViewInsets()
        {
            nfloat shift;

            var shiftSide = originalImageSize.Height < originalImageSize.Width;
            if (shiftSide)
                shift = cropView.Frame.Height / 2.0f - cropView.ContentSize.Height / 2.0f;
            else
                shift = cropView.Frame.Width / 2.0f - cropView.ContentSize.Width / 2.0f;

            if (shift > 0)
                if (shiftSide)
                    cropView.ContentInset = new UIEdgeInsets(shift, 0, 0, 0);
                else
                    cropView.ContentInset = new UIEdgeInsets(0, shift, 0, 0);
            else
                cropView.ContentInset = new UIEdgeInsets(0, 0, 0, 0);
        }

        private void ZoomTap()
        {
            if (originalImageSize.Width < originalImageSize.Height)
            {
                if (cropView.ZoomScale != cropView.Frame.Width / originalImageSize.Width)
                    cropView.SetZoomScale(cropView.Frame.Width / originalImageSize.Width, true);
                else
                    cropView.SetZoomScale(1f, true);
            }
            else
                if (cropView.ZoomScale != cropView.Frame.Height / originalImageSize.Height)
                cropView.SetZoomScale(cropView.Frame.Height / originalImageSize.Height, true);
            else
                cropView.SetZoomScale(1f, true);
        }

        private void MultiSelectTap()
        {
            source.MultiPickMode = !source.MultiPickMode;
            if (source.MultiPickMode)
                multiSelect.Image = UIImage.FromBundle("ic_multiselect_active");
            else
                multiSelect.Image = UIImage.FromBundle("ic_multiselect");

            photoCollection.ReloadData();

            if (imageView.Frame.Width < cropView.Frame.Width)
                cropView.Frame = new CGRect((cropView.Frame.Width - imageView.Frame.Width) / 2, cropView.Frame.Location.Y, imageView.Frame.Width, cropView.Frame.Height);
            if (imageView.Frame.Height < cropView.Frame.Height)
                cropView.Frame = new CGRect(cropView.Frame.Location.X, (cropView.Frame.Height - imageView.Frame.Height) / 2, cropView.Frame.Width, imageView.Frame.Height);

            ApplyRightScale();
            SetScrollViewInsets();
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
            var descriptionViewController = new DescriptionViewController(source.ImageAssets[0].Image, "jpg", _metadata);
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
            var rotated = new UIImage(photoView.Image.CGImage, photoView.Image.CurrentScale, orientation);
            UIGraphics.BeginImageContextWithOptions(rotated.Size, false, rotated.CurrentScale);
            var drawRect = new CGRect(0, 0, rotated.Size.Width, rotated.Size.Height);
            rotated.Draw(drawRect);
            //ImageAsset = photoView.Image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
        }
    }
}
