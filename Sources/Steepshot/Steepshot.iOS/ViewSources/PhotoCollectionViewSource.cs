using System;
using CoreGraphics;
using Foundation;
using Photos;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class PhotoCollectionViewSource : UICollectionViewSource
    {
        readonly PHFetchResult _fetchResults;
        readonly PHImageManager _m;
        public PhotoCollectionViewSource()
        {
            PHFetchOptions options = new PHFetchOptions
            {
                SortDescriptors = new[] { new NSSortDescriptor("creationDate", false) },
            };
            _fetchResults = PHAsset.FetchAssets(PHAssetMediaType.Image, options);
            _m = new PHImageManager();
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return _fetchResults.Count;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var imageCell = (PhotoCollectionViewCell)collectionView.DequeueReusableCell(nameof(PhotoCollectionViewCell), indexPath);

            _m.RequestImageForAsset((PHAsset)_fetchResults[indexPath.Item], new CGSize(300, 300),
                                    PHImageContentMode.AspectFill, new PHImageRequestOptions(), (img, info) =>
              {
                imageCell.UpdateImage(img);//, (PHAsset)_fetchResults[indexPath.Item]);
              });
            return imageCell;
        }
    }
}
