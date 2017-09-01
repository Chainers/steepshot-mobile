using System;
using System.Collections.Generic;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class TagsCollectionViewSource : UICollectionViewSource
    {
        public List<string> TagsCollection = new List<string>();
        public event RowSelectedHandler RowSelectedEvent;
        private readonly bool _isButtonNeed;
        private readonly EventHandler _buttonHandler;

        public TagsCollectionViewSource(EventHandler buttonEventHandler = null)
        {
            if (buttonEventHandler != null)
            {
                _isButtonNeed = true;
                _buttonHandler = buttonEventHandler;
            }
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return TagsCollection.Count;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, Foundation.NSIndexPath indexPath)
        {
            var tagCell = (TagCollectionViewCell)collectionView.DequeueReusableCell(nameof(TagCollectionViewCell), indexPath);
            tagCell.TagText = TagsCollection[indexPath.Row];
            if (_isButtonNeed && indexPath.Item == 0)
                tagCell.SetButton(_buttonHandler);
            else
                tagCell.RefreshCell();
            return tagCell;
        }

        public override void ItemHighlighted(UICollectionView collectionView, Foundation.NSIndexPath indexPath)
        {
            RowSelectedEvent?.Invoke((int)indexPath.Item);
        }
    }
}
