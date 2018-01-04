using System;
using System.Collections.Generic;
using Steepshot.Core.Models;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class LocalTagsCollectionViewSource : UICollectionViewSource
    {
        public List<string> LocalTags = new List<string>();
        public Action<ActionType, string> CellAction;

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return LocalTags.Count;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, Foundation.NSIndexPath indexPath)
        {
            var tagCell = (LocalTagCollectionViewCell)collectionView.DequeueReusableCell(nameof(LocalTagCollectionViewCell), indexPath);
            if (!tagCell.IsCellActionSet)
            {
                tagCell.CellAction = CellAction;
            }
            tagCell.RefreshCell(LocalTags[indexPath.Row]);
            return tagCell;
        }
    }
}
