using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using Photos;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class SavedPhoto
    {
        public PHAsset Asset { get; set; }
        public UIImage Image { get; set; }
        public CGPoint Offset { get; set; }
        public nfloat Scale { get; set; }
        public CGSize OriginalImageSize { get; set; }
        public NSDictionary Metadata { get; set; }

        public SavedPhoto(PHAsset asset, UIImage image, CGPoint offset)
        {
            Asset = asset;
            Image = image;
            Offset = offset;
        }
    }

    public class PhotoCollectionViewSource : UICollectionViewSource
    {
        private readonly PHFetchResult _fetchResults;
        private readonly PHImageManager _m;
        //public Dictionary<string, UIImage> ImageAssets = new Dictionary<string, UIImage>();
        public List<SavedPhoto> ImageAssets = new List<SavedPhoto>();
        public bool MultiPickMode { get; set; }
        public Tuple<NSIndexPath, PHAsset> CurrentlySelectedItem = new Tuple<NSIndexPath, PHAsset>(null, null);

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
            var pa = (PHAsset)_fetchResults[indexPath.Item];
            if (MultiPickMode)
                imageCell.UpdateImage(_m, pa, CurrentlySelectedItem.Item2?.LocalIdentifier == pa.LocalIdentifier,
                                      ImageAssets.FindIndex(a => a.Asset.LocalIdentifier == pa.LocalIdentifier),
                                      ImageAssets.Any(a => a.Asset.LocalIdentifier == pa.LocalIdentifier));
            else
                imageCell.UpdateImage(_m, pa, CurrentlySelectedItem.Item2?.LocalIdentifier == pa.LocalIdentifier);
            return imageCell;
        }

        public PHAsset GetPHAsset(int index)
        {
            return (PHAsset)_fetchResults[index];
        }
    }
}
