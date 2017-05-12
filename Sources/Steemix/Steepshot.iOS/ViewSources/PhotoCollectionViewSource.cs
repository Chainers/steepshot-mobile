using System;
using Foundation;
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

			PHFetchOptions options = new PHFetchOptions();
			options.SortDescriptors = new NSSortDescriptor[] { new NSSortDescriptor("creationDate", false) };
            fetchResults = PHAsset.FetchAssets(PHAssetMediaType.Image, options);
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
