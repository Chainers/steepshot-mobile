using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CoreGraphics;
using Foundation;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using Steepshot.iOS.Models;
using UIKit;

namespace Steepshot.iOS.Helpers
{
    public class CollectionViewFlowDelegate : UICollectionViewDelegateFlowLayout
    {
        public Action ScrolledToBottom;
        public Action<ActionType, Post> CellClicked;
        public bool IsGrid = true;
        protected BasePostPresenter _presenter;
        protected UICollectionView _collection;
        private int _prevPos;
        public List<CellSizeHelper> Variables = new List<CellSizeHelper>();

        public int Position => _prevPos;
        public NSIndexPath TopCurrentPosition;

        public void ClearPosition()
        {
            _prevPos = 0;
            Variables.Clear();
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
                //TopCurrentPosition = _collection.IndexPathsForVisibleItems.Min();
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

            CellClicked?.Invoke(ActionType.Preview, _presenter[(int)indexPath.Item]);
        }

        public void GenerateVariables()
        {
            if (Variables.Count == _presenter.Count)
                return;
            for (int i = Variables.Count; i < _presenter.Count; i++)
            {
                var cellVariables = CellHeightCalculator.Calculate(_presenter[i]);
                Variables.Add(cellVariables);
            }
        }

        public override CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
        {
            if (Variables.Count > indexPath.Item)
            {
                if (IsGrid)
                    return Constants.CellSize;
                return new CGSize(UIScreen.MainScreen.Bounds.Width, Variables[(int)indexPath.Item].CellHeight);
            }
            //loader height
            return new CGSize(UIScreen.MainScreen.Bounds.Width, 80);
        }
    }

    public class SliderCollectionViewFlowDelegate : CollectionViewFlowDelegate
    {
        private nfloat prevOffset = 0;

        public SliderCollectionViewFlowDelegate(UICollectionView collection, BasePostPresenter presenter = null) : base(collection, presenter)
        {
        }

        public override void Scrolled(UIScrollView scrollView)
        {
            foreach (var cell in _collection.IndexPathsForVisibleItems)
            {
                var sliderCell = _collection.CellForItem(cell) as SliderFeedCollectionViewCell;
                sliderCell?.MoveData(prevOffset - scrollView.ContentOffset.X);
            }
            prevOffset = scrollView.ContentOffset.X;

            base.Scrolled(scrollView);
        }

        public override CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
        {
            return new CGSize(UIScreen.MainScreen.Bounds.Width - 15 * 2, collectionView.Frame.Height);
        }
    }

    public class SliderFlowLayout : UICollectionViewFlowLayout
    {
        private CGPoint mostRecentOffset = new CGPoint();

        public override CGPoint TargetContentOffset(CGPoint proposedContentOffset, CGPoint scrollingVelocity)
        {
            if (scrollingVelocity.X == 0)
                return mostRecentOffset;

            if (CollectionView != null)
            {
                var cv = CollectionView;
                var cvBounds = cv.Bounds;
                var halfWidth = cvBounds.Size.Width * 0.5;

                var attributesForVisibleCells = LayoutAttributesForElementsInRect(cvBounds);

                UICollectionViewLayoutAttributes candidateAttributes = null;

                foreach (var attributes in attributesForVisibleCells)
                {
                    // == Skip comparison with non-cell items (headers and footers) == //
                    if (attributes.RepresentedElementCategory != UICollectionElementCategory.Cell)
                        continue;

                    if ((attributes.Center.X == 0) || (attributes.Center.X > (cv.ContentOffset.X + halfWidth) && scrollingVelocity.X < 0))
                        continue;

                    candidateAttributes = attributes;
                }

                // Beautification step , I don't know why it works!
                if (proposedContentOffset.X == -(cv.ContentInset.Left))
                    return proposedContentOffset;

                if (candidateAttributes == null)
                    return mostRecentOffset;

                mostRecentOffset = new CGPoint(Math.Floor(candidateAttributes.Center.X - halfWidth), proposedContentOffset.Y);
                return mostRecentOffset;
            }

            return base.TargetContentOffset(proposedContentOffset, scrollingVelocity);
        }
    }
}
