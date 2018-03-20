using System;
using System.Collections.Generic;
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

            var rotateTap = new UITapGestureRecognizer(RotateTap);
            rotate.AddGestureRecognizer(rotateTap);

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

            NavigationController.NavigationBar.Translucent = false;
            SetBackButton();
        }

        private void CellAction(ActionType type, Tuple<NSIndexPath, PHAsset> photo)
        {
            _m.RequestImageForAsset(photo.Item2, CalculateInSampleSize(new CGSize(photo.Item2.PixelWidth, photo.Item2.PixelHeight), 1200, 1200),
                                    PHImageContentMode.Default, new PHImageRequestOptions() { ResizeMode = PHImageRequestOptionsResizeMode.Exact, DeliveryMode = PHImageRequestOptionsDeliveryMode.HighQualityFormat }, (img, info) =>
                                                 {
                                                     if (source.MultiPickMode)
                                                     {
                                                         if (!source.ImageAssets.Any(a => a.Item1 == photo.Item2.LocalIdentifier))
                                                             source.ImageAssets.Add(new Tuple<string, UIImage>(photo.Item2.LocalIdentifier, img));
                                                         else
                                                             source.ImageAssets.Remove(source.ImageAssets.First(a => a.Item1 == photo.Item2.LocalIdentifier));
                                                         photoCollection.ReloadData();
                                                         //photoCollection.ReloadItems(new NSIndexPath[1] { photo.Item1 });
                                                     }
                                                 });
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
            var descriptionViewController = new DescriptionViewController(source.ImageAssets[0].Item2 , "jpg", _metadata);
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
            RotateImage(orientation);
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

