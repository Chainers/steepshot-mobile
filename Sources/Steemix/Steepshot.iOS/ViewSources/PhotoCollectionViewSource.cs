using System;
using Photos;
using UIKit;

namespace Steepshot.iOS
{
    public class PhotoCollectionViewSource : UICollectionViewSource
    {
        PHFetchResult fetchResults;
        PHImageManager m;
        public PhotoCollectionViewSource()
        {
            fetchResults = PHAsset.FetchAssets(PHAssetMediaType.Image, null);
            m = new PHImageManager();
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return fetchResults.Count;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, Foundation.NSIndexPath indexPath)
        {
            var imageCell = (PhotoCollectionViewCell)collectionView.DequeueReusableCell("PhotoCollectionViewCell", indexPath);

            m.RequestImageForAsset((PHAsset)fetchResults[indexPath.Item], new CoreGraphics.CGSize(150, 150),
                                   PHImageContentMode.AspectFit, new PHImageRequestOptions(), (img, info) =>
              {
                imageCell.UpdateImage(img, (PHAsset)fetchResults[indexPath.Item]);
              });
            return imageCell;
        }
    }
}
