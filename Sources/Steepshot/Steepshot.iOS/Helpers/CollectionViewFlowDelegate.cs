using System;
using System.Linq;
using Foundation;
using Steepshot.Core.Presenters;
using UIKit;

namespace Steepshot.iOS.Helpers
{
    public class CollectionViewFlowDelegate : UICollectionViewDelegateFlowLayout
    {
        public Action ScrolledToBottom;
        public Action<NSIndexPath> CellClicked;
        public bool IsGrid = true;
        private BasePostPresenter _presenter;
        private UICollectionView _collection;
        private int _prevPos;

        public int Position => _prevPos;

        public void ClearPosition()
        {
            _prevPos = 0;
        }

        public CollectionViewFlowDelegate(UICollectionView collection, BasePostPresenter presenter = null)
        {
            _collection = collection;
            _presenter = presenter;
        }

        public override void Scrolled(UIScrollView scrollView)
        {
            if (_collection.IndexPathsForVisibleItems.Length > 0)
            {
                var pos = _collection.IndexPathsForVisibleItems.Max(c => c.Row);
                if (pos > _prevPos)
                {
                    if (pos == _presenter.Count - 1)
                    {
                        _prevPos = pos;
                        ScrolledToBottom?.Invoke();
                    }
                }
            }
        }

        public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            if (!IsGrid)
                return;

            CellClicked?.Invoke(indexPath);
        }

        /*
        public override CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
        {
            if (!IsGrid)
            {
                var post = _presenter[indexPath.Row];
                if (post != null)
                {
                    var correction = PhotoHeight.Get(post.ImageSize);
                    //30 - margins sum
                    CGRect textSize = new CGRect();
                    //if (_commentString.Any())
                    //textSize = _commentString[indexPath.Row].GetBoundingRect(new CGSize(UIScreen.MainScreen.Bounds.Width - 30, 1000), NSStringDrawingOptions.UsesLineFragmentOrigin, null);

                    //192 => 512-320 cell height without image size
                    var cellHeight = 192 + correction;
                    return new CGSize(UIScreen.MainScreen.Bounds.Width, cellHeight + textSize.Size.Height);
                }
            }
            return Helpers.Constants.CellSize;//CGSize(UIScreen.MainScreen.Bounds.Width, cellHeight + textSize.Size.Height);
        } */
    }
}
