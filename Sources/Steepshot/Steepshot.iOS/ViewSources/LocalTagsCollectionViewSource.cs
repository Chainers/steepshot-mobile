using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using Steepshot.Core.Models.Enums;
using Steepshot.iOS.Cells;
using Steepshot.iOS.Helpers;
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

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
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

    public class LocalTagsCollectionViewFlowDelegate : UICollectionViewDelegateFlowLayout
    {
        private List<nfloat> _cellWidths = new List<nfloat>();
        private LocalTagsCollectionViewSource _collectionViewSource;
        private UILabel label;
        private nfloat _maxCellWidth;

        public LocalTagsCollectionViewFlowDelegate(LocalTagsCollectionViewSource collectionViewSource, nfloat maxCellWidth)
        {
            _collectionViewSource = collectionViewSource;
            label = new UILabel();
            label.Font = Constants.Semibold14;
            _maxCellWidth = maxCellWidth;
        }

        public void GenerateVariables()
        {
            _cellWidths.Clear();
            for (int i = 0; i < _collectionViewSource.LocalTags.Count; i++)
            {
                //36 + text width + 47 = 83
                label.Text = _collectionViewSource.LocalTags[i];
                var width = label.SizeThatFits(new CGSize(0, 40)).Width + 83;
                _cellWidths.Add(width > _maxCellWidth ? _maxCellWidth : width);
            }
        }

        public override CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
        {
            return new CGSize(_cellWidths[indexPath.Row], 40);
        }
    }
}
