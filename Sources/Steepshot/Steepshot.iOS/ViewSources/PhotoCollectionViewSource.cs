using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Photos;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class PhotoCollectionViewSource : UICollectionViewSource
    {
        private readonly PHFetchResult _fetchResults;
        private readonly PHImageManager _m;
        //public Dictionary<string, UIImage> ImageAssets = new Dictionary<string, UIImage>();
        public List<Tuple<string, UIImage>> ImageAssets = new List<Tuple<string, UIImage>>();
        public bool MultiPickMode { get; set; } = true;
        public NSIndexPath CurrentlySelectedItem;

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
            if (MultiPickMode)
                imageCell.UpdateImage(_m, (PHAsset)_fetchResults[indexPath.Item], CurrentlySelectedItem == indexPath,
                                      ImageAssets.IndexOf(ImageAssets.FirstOrDefault(a => a.Item1 == ((PHAsset)_fetchResults[indexPath.Item]).LocalIdentifier)),
                                      ImageAssets.Any(a => a.Item1 == ((PHAsset)_fetchResults[indexPath.Item]).LocalIdentifier));
            else
                imageCell.UpdateImage(_m, (PHAsset)_fetchResults[indexPath.Item], CurrentlySelectedItem == indexPath);
            return imageCell;
        }

        public PHAsset GetPHAsset(int index)
        {
            return (PHAsset)_fetchResults[index];
        }
    }
}
