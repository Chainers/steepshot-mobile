using System;
using System.Collections.Generic;
using Foundation;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class PhotoGalleryViewSource : UICollectionViewSource
    {
        private List<Tuple<NSDictionary, UIImage>> _photoCollection;

        public PhotoGalleryViewSource(List<Tuple<NSDictionary, UIImage>> photoCollection)
        {
            _photoCollection = photoCollection;
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return _photoCollection.Count;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var tagCell = (PhotoGalleryCell)collectionView.DequeueReusableCell(nameof(PhotoGalleryCell), indexPath);
            tagCell.UpdateImage(_photoCollection[indexPath.Row].Item2);
            return tagCell;
        }
    }
}
