using System;
using System.Collections.Generic;
using System.Linq;
using AVFoundation;
using CoreGraphics;
using Foundation;
using ImageIO;
using Photos;
using PureLayout.Net;
using Steepshot.Core.Models.Enums;
using Steepshot.iOS.Cells;
using Steepshot.iOS.CustomViews;
using Steepshot.iOS.Delegates;
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
        private UILabel _titleLabel;
        private UIView _modalFolderView = new UIView();
        private UIImageView _arrowImage;
        private readonly UIBarButtonItem leftBarButton = new UIBarButtonItem();
        private readonly UIBarButtonItem rightBarButton = new UIBarButtonItem();
        private readonly CustomTitle titleView = new CustomTitle();
        private UITapGestureRecognizer titleTapGesture;
        private FolderTableViewSource folderSource;
        private readonly UITapGestureRecognizer rotateTap;
        private readonly UITapGestureRecognizer zoomTap;
        private readonly UITapGestureRecognizer multiselectTap;
        private readonly PHAssetMediaType assetMediaType;

        public PhotoPreviewViewController(PHAssetMediaType mediaType)
        {
            assetMediaType = mediaType;
            _m = new PHImageManager();
            rotateTap = new UITapGestureRecognizer(RotateTap);
            zoomTap = new UITapGestureRecognizer(ZoomTap);
            multiselectTap = new UITapGestureRecognizer(MultiSelectTap);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            bottomArrow.Transform = CGAffineTransform.MakeRotation((float)(Math.PI));
            source = new PhotoCollectionViewSource(_m);
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

            _cropView = new CropView(new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Width));

            var albums = new List<PHAssetCollection>();
            var sortedAlbums = new List<Tuple<string, PHFetchResult>>();
            var fetchOptions = new PHFetchOptions();

            var allAlbums = PHAssetCollection.FetchAssetCollections(PHAssetCollectionType.Album, PHAssetCollectionSubtype.AlbumRegular, null)
                                                 .Cast<PHAssetCollection>();
            albums.AddRange(allAlbums);
            var smartAlbums = PHAssetCollection.FetchAssetCollections(PHAssetCollectionType.SmartAlbum, PHAssetCollectionSubtype.AlbumRegular, null)
                                               .Cast<PHAssetCollection>().Where(a => !a.LocalizedTitle.Equals("Recently Deleted"));
            albums.AddRange(smartAlbums);
            fetchOptions.Predicate = NSPredicate.FromFormat("mediaType == %d", FromObject(assetMediaType));

            foreach (var item in albums)
            {
                var firstAsset = PHAsset.FetchAssets(item, fetchOptions);
                if (firstAsset?.Count > 0)
                    sortedAlbums.Add(new Tuple<string, PHFetchResult>(item.LocalizedTitle, firstAsset));
            }

            sortedAlbums = sortedAlbums.OrderByDescending(a => a.Item2.Count).ToList();

            _modalFolderView.BackgroundColor = UIColor.White;

            var folderTable = new UITableView
            {
                Bounces = false,
                AllowsSelection = false,
                RowHeight = 90,
                SeparatorStyle = UITableViewCellSeparatorStyle.None
            };
            folderSource = new FolderTableViewSource(sortedAlbums);
            folderTable.Source = folderSource;
            _modalFolderView.AddSubview(folderTable);
            folderTable.RegisterClassForCellReuse(typeof(AlbumTableViewCell), nameof(AlbumTableViewCell));
            folderTable.AutoPinEdgesToSuperviewEdges();
            folderTable.ReloadData();

            cropBackgroundView.BackgroundColor = Constants.R245G245B245;
            cropBackgroundView.AddSubview(_cropView);
            NavigationController.NavigationBar.Translucent = false;
            SetBackButton();

            _titleLabel.Text = sortedAlbums.FirstOrDefault()?.Item1;
            source.UpdateFetchResult(sortedAlbums.FirstOrDefault()?.Item2);

            ButtonsHidden(true);
        }

        public override void ViewWillAppear(bool animated)
        {
            if (IsMovingToParentViewController)
            {
                delegateP.CellClicked = CellAction;
                folderSource.CellAction += FolderSource_CellAction;
                _cropView.ZoomingStarted += _cropView_ZoomingStarted;
                _cropView.ZoomingEnded += _cropView_ZoomingEnded;
                leftBarButton.Clicked += GoBack;
                rightBarButton.Clicked += GoForward;
                titleView.AddGestureRecognizer(titleTapGesture);
                rotate.AddGestureRecognizer(rotateTap);
                resize.AddGestureRecognizer(zoomTap);
                multiSelect.AddGestureRecognizer(multiselectTap);
            }
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            if (IsMovingFromParentViewController)
            {
                delegateP.CellClicked = null;
                folderSource.CellAction -= FolderSource_CellAction;
                folderSource.FreeAllCells();
                _cropView.ZoomingStarted -= _cropView_ZoomingStarted;
                _cropView.ZoomingEnded -= _cropView_ZoomingEnded;
                leftBarButton.Clicked -= GoBack;
                rightBarButton.Clicked -= GoForward;
                titleView.RemoveGestureRecognizer(titleTapGesture);
                rotate.RemoveGestureRecognizer(rotateTap);
                resize.RemoveGestureRecognizer(zoomTap);
                multiSelect.RemoveGestureRecognizer(multiselectTap);

            }
            base.ViewWillDisappear(animated);
        }

        public override void ViewDidLayoutSubviews()
        {
            _modalFolderView.Frame = new CGRect(0, View.Frame.Height, View.Frame.Height, View.Frame.Width);
            View.AddSubview(_modalFolderView);
        }

        private void FolderSource_CellAction(ActionType arg1, Tuple<string, PHFetchResult> arg2)
        {
            TitleTapped();
            _titleLabel.Text = arg2.Item1;
            source.UpdateFetchResult(arg2.Item2);
            photoCollection.SetContentOffset(new CGPoint(0, 0), false);
            photoCollection.ReloadData();
            if (!source.MultiPickMode)
                delegateP.ItemSelected(photoCollection, NSIndexPath.FromItemSection(0, 0));
        }

        private void _cropView_ZoomingStarted(object sender, UIScrollViewZoomingEventArgs e)
        {
            NavigationItem.RightBarButtonItem.Enabled = false;
        }

        private void _cropView_ZoomingEnded(object sender, ZoomingEndedEventArgs e)
        {
            NavigationItem.RightBarButtonItem.Enabled = true;
        }

        private void CellAction(ActionType type, Tuple<NSIndexPath, PHAsset> asset)
        {
            switch (type)
            {
                case ActionType.Close:
                    ShowAlert(Core.Localization.LocalizationKeys.PickedPhotosLimit);
                    return;
                case ActionType.Hide:
                    ButtonsHidden(true);
                    break;
                default:
                    ButtonsHidden(false);
                    break;
            }

            if (asset.Item2.MediaType == PHAssetMediaType.Image)
            {
                photoCollection.UserInteractionEnabled = false;
                NavigationItem.RightBarButtonItem.Enabled = false;
                pickedPhoto = asset;
                previousPhotoLocalIdentifier = source.CurrentlySelectedItem?.Item2?.LocalIdentifier;
                var pickOptions = new PHImageRequestOptions() { ResizeMode = PHImageRequestOptionsResizeMode.Exact, DeliveryMode = PHImageRequestOptionsDeliveryMode.HighQualityFormat, NetworkAccessAllowed = true };
                var imageSize = ImageHelper.CalculateInSampleSize(new CGSize(asset.Item2.PixelWidth, asset.Item2.PixelHeight), Core.Constants.PhotoMaxSize, Core.Constants.PhotoMaxSize);

                _m.RequestImageForAsset(asset.Item2, imageSize, PHImageContentMode.Default, pickOptions, PickImage);
            }
            else
            {
                _m.RequestAvAsset(asset.Item2, null, PickVideo);
            }
        }

        private void PickVideo(AVAsset asset, AVAudioMix audioMix, NSDictionary info)
        {
            var urlAsset = asset as AVUrlAsset;
            var track = asset.TracksWithMediaType(AVMediaType.Video).First();
            var dimensions = CGAffineTransform.CGRectApplyAffineTransform(new CGRect(0, 0, track.NaturalSize.Width, track.NaturalSize.Height), track.PreferredTransform);
            InvokeOnMainThread(() =>
            {
                _cropView.AdjustVideoViewSize(new CGSize(dimensions.Width, dimensions.Height));
                _cropView.VideoView.Hidden = false;
                _cropView.ImageView.Hidden = true;
                NavigationItem.RightBarButtonItem.Enabled = true;
                photoCollection.UserInteractionEnabled = true;
            });
            _cropView.VideoView.ChangeItem(urlAsset.Url);
            _cropView.VideoView.Play();
        }

        private void ButtonsHidden(bool value)
        {
            rotate.Hidden = value;
            resize.Hidden = value;
            multiSelect.Hidden = value;
            topArrow.Hidden = value;
            bottomArrow.Hidden = value;
        }

        private void PickImage(UIImage img, NSDictionary info)
        {
            var previousZoomScale = _cropView.ZoomScale;
            var previousOffset = _cropView.ContentOffset;
            var previousOriginalSize = _cropView.originalContentSize;
            var previousOrientation = _cropView.orientation;

            var currentPhoto = source.ImageAssets.FirstOrDefault(a => a.Asset.LocalIdentifier == pickedPhoto.Item2.LocalIdentifier);

            if (currentPhoto?.Orientation != null && currentPhoto?.Orientation != UIImageOrientation.Up)
            {
                currentPhoto.Image = img = ImageHelper.RotateImage(img, currentPhoto.Orientation);
                _cropView.orientation = currentPhoto.Orientation;
            }
            else
                _cropView.orientation = UIImageOrientation.Up;

            _cropView.VideoView.Stop();
            _cropView.AdjustImageViewSize(img);
            _cropView.ImageView.Hidden = false;
            _cropView.VideoView.Hidden = true;

            _cropView.ImageView.Image = img;

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
            photoCollection.UserInteractionEnabled = true;
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
                if (_cropView.ImageView.Frame.Width < _cropView.Frame.Width)
                    _cropView.Frame = new CGRect((_cropView.Frame.Width - _cropView.ImageView.Frame.Width) / 2, _cropView.Frame.Location.Y, _cropView.ImageView.Frame.Width, _cropView.Frame.Height);
                if (_cropView.ImageView.Frame.Height < _cropView.Frame.Height)
                    _cropView.Frame = new CGRect(_cropView.Frame.Location.X, (_cropView.Frame.Height - _cropView.ImageView.Frame.Height) / 2, _cropView.Frame.Width, _cropView.ImageView.Frame.Height);

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
            leftBarButton.Image = UIImage.FromBundle("ic_back_arrow");
            rightBarButton.Image = new UIImage(leftBarButton.Image.CGImage, leftBarButton.Image.CurrentScale, UIImageOrientation.UpMirrored);
            rightBarButton.Enabled = false;
            NavigationItem.LeftBarButtonItem = leftBarButton;
            NavigationItem.RightBarButtonItem = rightBarButton;
            NavigationController.NavigationBar.TintColor = Constants.R15G24B30;

            NavigationItem.TitleView = titleView;
            titleView.UserInteractionEnabled = true;
            titleTapGesture = new UITapGestureRecognizer(TitleTapped);
            titleView.AddGestureRecognizer(titleTapGesture);
            _titleLabel = new UILabel();
            titleView.AddSubview(_titleLabel);
            _titleLabel.AutoCenterInSuperview();

            _arrowImage = new UIImageView();
            var forwardImage = UIImage.FromFile("ic_forward");
            _arrowImage.Image = new UIImage(forwardImage.CGImage, forwardImage.CurrentScale, UIImageOrientation.LeftMirrored);
            titleView.AddSubview(_arrowImage);

            _arrowImage.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            _arrowImage.AutoPinEdge(ALEdge.Left, ALEdge.Right, _titleLabel, 10);
        }

        private void TitleTapped()
        {
            if (_modalFolderView.Frame.Y != 0)
            {
                UIView.Animate(0.2, 0, UIViewAnimationOptions.CurveEaseOut, () =>
                {
                    _modalFolderView.Frame = new CGRect(0, 0, View.Frame.Width, View.Frame.Height);
                    leftBarButton.TintColor = rightBarButton.TintColor = UIColor.Clear;
                    leftBarButton.Enabled = rightBarButton.Enabled = false;

                    _arrowImage.Transform = CGAffineTransform.MakeRotation((nfloat)Math.PI);
                }, null);
            }
            else
            {
                UIView.Animate(0.2, 0, UIViewAnimationOptions.CurveEaseIn, () =>
                {
                    _modalFolderView.Frame = new CGRect(0, View.Frame.Height, View.Frame.Width, View.Frame.Height);
                    leftBarButton.TintColor = rightBarButton.TintColor = Constants.R15G24B30;
                    leftBarButton.Enabled = rightBarButton.Enabled = true;

                    _arrowImage.Transform = CGAffineTransform.MakeRotation(-(nfloat)(Math.PI * 2));
                }, null);
            }
        }

        private void GoForward(object sender, EventArgs e)
        {
            var croppedPhotos = new List<Tuple<NSDictionary, UIImage>>();

            var currentPhoto = source.ImageAssets.FirstOrDefault(a => a.Asset.LocalIdentifier == source.CurrentlySelectedItem.Item2.LocalIdentifier);
            if (currentPhoto != null)
            {
                currentPhoto.Offset = _cropView.ContentOffset;
                currentPhoto.Scale = _cropView.ZoomScale;
                currentPhoto.OriginalImageSize = _cropView.originalContentSize;
                currentPhoto.Orientation = _cropView.orientation;
            }

            foreach (var item in source.ImageAssets)
            {
                NSDictionary metadata = null;
                var croppedPhoto = _cropView.CropImage(item);
                _m.RequestImageData(item.Asset, new PHImageRequestOptions() { Synchronous = true }, (data, dataUti, orientation, info) =>
                {
                    if (data != null)
                    {
                        var dataSource = CGImageSource.FromData(data);
                        metadata = dataSource?.GetProperties(0)?.Dictionary;
                    }
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
                    currentPhoto.OriginalImageSize = _cropView.originalContentSize;
                    currentPhoto.Orientation = _cropView.orientation;
                    currentPhoto.Image = _cropView.ImageView.Image;
                }
            }
            else
            {
                source.ImageAssets[0].Image = _cropView.ImageView.Image;
                _cropView.ApplyCriticalScale();
            }
        }
    }

    public class CustomTitle : UIView
    {
        public override CGSize IntrinsicContentSize => UILayoutFittingExpandedSize;
    }
}
