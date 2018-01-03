using System;
using System.Collections.Generic;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class LocalTagsCollectionViewSource : UICollectionViewSource
    {
        public List<string> LocalTags = new List<string>();

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return LocalTags.Count;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, Foundation.NSIndexPath indexPath)
        {
            var tagCell = (LocalTagCollectionViewCell)collectionView.DequeueReusableCell(nameof(LocalTagCollectionViewCell), indexPath);
            tagCell.RefreshCell(LocalTags[indexPath.Row]);
            return tagCell;
        }
    }

    /*
    public class GG : UICollectionViewFlowLayout
    {
        gets 
    }*/
}
