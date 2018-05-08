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
        private List<string> _urlCollection;

        public PhotoGalleryViewSource(List<Tuple<NSDictionary, UIImage>> photoCollection)
        {
            _photoCollection = photoCollection;
        }

        public PhotoGalleryViewSource(List<string> urlCollection)
        {
            _urlCollection = urlCollection;
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            if (_photoCollection != null)
                return _photoCollection.Count;
            else
                return _urlCollection.Count;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var tagCell = (PhotoGalleryCell)collectionView.DequeueReusableCell(nameof(PhotoGalleryCell), indexPath);

            if (_photoCollection != null)
                tagCell.UpdateImage(_photoCollection[indexPath.Row].Item2);
            else
                tagCell.UpdateImage(_urlCollection[indexPath.Row]);
            
            return tagCell;
        }
    }
}
