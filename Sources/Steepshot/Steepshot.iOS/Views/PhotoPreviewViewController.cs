using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using ImageIO;
using Photos;
using Steepshot.Core.Models.Enums;
using Steepshot.iOS.Cells;
using Steepshot.iOS.CustomViews;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class PhotoPreviewViewController : BaseViewController
    {
        private readonly PHImageManager _m;
        private CropView _cropView;
        private PhotoCollectionViewSource source;
        private PhotoCollectionViewFlowDelegate delegateP;
        private string previousPhotoLocalIdentifier;
        private Tuple<NSIndexPath, PHAsset> pickedPhoto;
        private bool _toSquareMode = true;

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

            _cropView = new CropView(new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Width));

            _cropView.ZoomingStarted += (object sender, UIScrollViewZoomingEventArgs e) => 
            {
                NavigationItem.RightBarButtonItem.Enabled = false;
            };

            _cropView.ZoomingEnded+= (object sender, ZoomingEndedEventArgs e) => 
            {
                NavigationItem.RightBarButtonItem.Enabled = true;
            };

            cropBackgroundView.BackgroundColor = Constants.R245G245B245;
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
            var imageSize = ImageHelper.CalculateInSampleSize(new CGSize(photo.Item2.PixelWidth, photo.Item2.PixelHeight), Core.Constants.PhotoMaxSize, Core.Constants.PhotoMaxSize);
            _m.RequestImageForAsset(photo.Item2, imageSize, PHImageContentMode.Default, pickOptions, PickImage);
        }

        private void PickImage(UIImage img, NSDictionary info)
        {
            var previousZoomScale = _cropView.ZoomScale;
            var previousOffset = _cropView.ContentOffset;
            var previousOriginalSize = _cropView.originalImageSize;
            var previousOrientation = _cropView.orientation;

            var currentPhoto = source.ImageAssets.FirstOrDefault(a => a.Asset.LocalIdentifier == pickedPhoto.Item2.LocalIdentifier);

            if (currentPhoto?.Orientation != null && currentPhoto?.Orientation != UIImageOrientation.Up)
            {
                currentPhoto.Image = img = ImageHelper.RotateImage(img, currentPhoto.Orientation);
                _cropView.orientation = currentPhoto.Orientation;
            }
            else
                _cropView.orientation = UIImageOrientation.Up;
            _cropView.AdjustImageViewSize(img);

            _cropView.imageView.Image = img;

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
                        _cropView.ApplyRightScale();
                        _cropView.SetScrollViewInsets();
                        source.ImageAssets.Add(new SavedPhoto(pickedPhoto.Item2, img, _cropView.ContentOffset));
                    }
                    else
                    {
                        _cropView.ApplyRightScale((float)currentPhoto.Scale);
                        _cropView.SetScrollViewInsets();
                        _cropView.SetContentOffset(currentPhoto.Offset, false);
                    }
                }
                else
                {
                    if (source.ImageAssets.Count != 1)
                        source.ImageAssets.RemoveAll(a => a.Asset.LocalIdentifier == pickedPhoto.Item2.LocalIdentifier);
                    _cropView.ApplyRightScale();
                    _cropView.SetScrollViewInsets();
                }

                photoCollection.ReloadData();
            }
            else
            {
                _cropView.ApplyCriticalScale();
                if (source.ImageAssets.Count == 0)
                    source.ImageAssets.Add(new SavedPhoto(pickedPhoto.Item2, img, _cropView.ContentOffset));
                else
                    source.ImageAssets[0] = new SavedPhoto(pickedPhoto.Item2, img, _cropView.ContentOffset);
                if (_toSquareMode)
                    _cropView.ZoomTap(_toSquareMode, false);
                _cropView.SetScrollViewInsets();
            }
            NavigationItem.RightBarButtonItem.Enabled = true;
        }

        private void ZoomTap()
        {
            UIView.Animate(0.15, () =>
            {
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
            _toSquareMode = !_toSquareMode;
            _cropView.ZoomTap(false);
        }

        private void MultiSelectTap()
        {
            source.MultiPickMode = !source.MultiPickMode;
            if (source.MultiPickMode)
            {
                multiSelect.Image = UIImage.FromBundle("ic_multiselect_active");
                if (_cropView.imageView.Frame.Width < _cropView.Frame.Width)
                    _cropView.Frame = new CGRect((_cropView.Frame.Width - _cropView.imageView.Frame.Width) / 2, _cropView.Frame.Location.Y, _cropView.imageView.Frame.Width, _cropView.Frame.Height);
                if (_cropView.imageView.Frame.Height < _cropView.Frame.Height)
                    _cropView.Frame = new CGRect(_cropView.Frame.Location.X, (_cropView.Frame.Height - _cropView.imageView.Frame.Height) / 2, _cropView.Frame.Width, _cropView.imageView.Frame.Height);

                _cropView.ApplyRightScale();
                _cropView.SetScrollViewInsets();
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
                currentPhoto.OriginalImageSize = _cropView.originalImageSize;
                currentPhoto.Orientation = _cropView.orientation;
            }

            foreach (var item in source.ImageAssets)
            {
                NSDictionary metadata = null;
                var croppedPhoto = _cropView.CropImage(item);
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

            _cropView.RotateTap();

            if (source.MultiPickMode)
            {
                _cropView.ApplyRightScale();
                var currentPhoto = source.ImageAssets.FirstOrDefault(a => a.Asset.LocalIdentifier == source.CurrentlySelectedItem.Item2.LocalIdentifier);
                if (currentPhoto != null)
                {
                    currentPhoto.Offset = _cropView.ContentOffset;
                    currentPhoto.Scale = _cropView.ZoomScale;
                    currentPhoto.OriginalImageSize = _cropView.originalImageSize;
                    currentPhoto.Orientation = _cropView.orientation;
                    currentPhoto.Image = _cropView.imageView.Image;
                }
            }
            else
            {
                source.ImageAssets[0].Image = _cropView.imageView.Image;
                _cropView.ApplyCriticalScale();
            }
        }
    }
}
