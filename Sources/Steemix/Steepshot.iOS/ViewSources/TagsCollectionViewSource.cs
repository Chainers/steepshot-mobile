using System;
using System.Collections.Generic;
using UIKit;

namespace Steepshot.iOS
{
    public class TagsCollectionViewSource : UICollectionViewSource
    {
        public List<string> tagsCollection = new List<string>();
        public event RowSelectedHandler RowSelectedEvent;
        private bool _isButtonNeed;
        private EventHandler _buttonHandler;

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
            return tagsCollection.Count;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, Foundation.NSIndexPath indexPath)
        {
            var tagCell = (TagCollectionViewCell)collectionView.DequeueReusableCell(nameof(TagCollectionViewCell), indexPath);
            tagCell.TagText = tagsCollection[indexPath.Row];
            if (_isButtonNeed && indexPath.Item == 0)
                tagCell.SetButton(_buttonHandler);
            else
                tagCell.RefreshCell();
            return tagCell;
        }

        public override void ItemHighlighted(UICollectionView collectionView, Foundation.NSIndexPath indexPath)
        {
            if(RowSelectedEvent != null)
                RowSelectedEvent((int)indexPath.Item);
        }
    }
}
