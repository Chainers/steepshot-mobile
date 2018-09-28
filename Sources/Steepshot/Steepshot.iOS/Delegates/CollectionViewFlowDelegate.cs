using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using Photos;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.Core.Models.Responses;
using Steepshot.iOS.Cells;
using Steepshot.iOS.Models;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.Helpers
{
    public class CollectionViewFlowDelegate : UICollectionViewDelegateFlowLayout
    {
        protected virtual int VariablesCount => Variables.Count;
        private nfloat profileHeight;
        private int prevPos;

        protected BasePostPresenter _presenter;
        protected UICollectionView _collection;

        public List<CellSizeHelper> Variables = new List<CellSizeHelper>();
        public Action<ActionType, Post> CellClicked;
        public Action ScrolledToBottom;
        public bool IsGrid = true;
        public bool IsProfile;

        public ProfileHeaderCellBuilder profileCell;
        public NSIndexPath TopCurrentPosition;
        public int Position => prevPos;

        public void ClearPosition()
        {
            prevPos = 0;
            Variables.Clear();
        }

        public CollectionViewFlowDelegate(UICollectionView collection, BasePostPresenter presenter = null)
        {
            _collection = collection;
            _presenter = presenter;
            profileCell = new ProfileHeaderCellBuilder();
        }

        protected DateTime previousScrollMoment;
        protected nfloat previousScrollY = 0;
        public double velocity;

        public override void Scrolled(UIScrollView scrollView)
        {
            var d = DateTime.Now;
            var y = scrollView.ContentOffset.Y;
            var elapsed = d.Subtract(previousScrollMoment).TotalMilliseconds;
            var distance = y - previousScrollY;
            velocity = Math.Abs(distance / elapsed);
            previousScrollMoment = d;
            previousScrollY = y;

            if (velocity < 0.8 && _collection.IndexPathsForVisibleItems.Length > 0)
            {
                var attributes = new List<UICollectionViewLayoutAttributes>();

                foreach (var item in _collection.IndexPathsForVisibleItems)
                    attributes.Add(_collection.GetLayoutAttributesForItem(item));

                var center = scrollView.ContentOffset.Y + scrollView.Frame.Height / 2;

                var closestToCenterCell = attributes.Aggregate(
                    (UICollectionViewLayoutAttributes arg1, UICollectionViewLayoutAttributes arg2) =>
                    Math.Abs(arg1.Center.Y - center) < Math.Abs(arg2.Center.Y - center) ? arg1 : arg2);

                foreach (var item in _collection.IndexPathsForVisibleItems)
                {
                    if (_collection.CellForItem(item) is NewFeedCollectionViewCell cell)
                        cell.Cell.Playback(item.Item == closestToCenterCell.IndexPath.Item);
                }

                if (_collection.IndexPathsForVisibleItems.Length > 0)
                {
                    var pos = _collection.IndexPathsForVisibleItems.Max(c => c.Row);
                    //TopCurrentPosition = _collection.IndexPathsForVisibleItems.Min();
                    if (pos > prevPos)
                    {
                        if (pos == (IsProfile ? _presenter.Count : _presenter.Count - 1))
                        {
                            prevPos = pos;
                            ScrolledToBottom?.Invoke();
                        }
                    }
                }
            }
        }

        public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            if (!IsGrid)
                return;

            if (IsProfile && indexPath.Row == 0)
                return;

            CellClicked?.Invoke(ActionType.Preview, IsProfile ? _presenter[(int)indexPath.Item - 1] : _presenter[(int)indexPath.Item]);
        }

        public void GenerateVariables()
        {
            if (Variables.Count == _presenter.Count)
                return;

            if (Variables.Count > _presenter.Count)
                Variables.Clear();

            for (int i = Variables.Count; i < _presenter.Count; i++)
            {
                var cellVariables = CellHeightCalculator.Calculate(_presenter[i]);
                Variables.Add(cellVariables);
            }
        }

        public void UpdateProfile(UserProfileResponse userData)
        {
            profileHeight = profileCell.UpdateProfile(userData);
        }

        public override CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
        {
            if (VariablesCount > indexPath.Item)
            {
                if (IsProfile && indexPath.Row == 0)
                    return new CGSize(UIScreen.MainScreen.Bounds.Width, profileHeight);

                if (IsGrid)
                    return Constants.CellSize;

                return new CGSize(UIScreen.MainScreen.Bounds.Width, Variables[IsProfile ? (int)indexPath.Item - 1 : (int)indexPath.Item].CellHeight);
            }
            //loader height
            return new CGSize(UIScreen.MainScreen.Bounds.Width, 80);
        }
    }

    public class ProfileCollectionViewFlowDelegate : CollectionViewFlowDelegate
    {
        public ProfileCollectionViewFlowDelegate(UICollectionView collection, BasePostPresenter presenter = null) : base(collection, presenter)
        {
        }

        protected override int VariablesCount => Variables.Count + 1;
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

            if (_collection.IndexPathsForVisibleItems.Length > 0)
            {
                var attributes = new List<UICollectionViewLayoutAttributes>();

                foreach (var item in _collection.IndexPathsForVisibleItems)
                    attributes.Add(_collection.GetLayoutAttributesForItem(item));

                var center = scrollView.ContentOffset.X + scrollView.Frame.Width / 2;

                var closestToCenterCell = attributes.Aggregate(
                    (UICollectionViewLayoutAttributes arg1, UICollectionViewLayoutAttributes arg2) =>
                    Math.Abs(arg1.Center.X - center) < Math.Abs(arg2.Center.X - center) ? arg1 : arg2);

                foreach (var item in _collection.IndexPathsForVisibleItems)
                {
                    if (_collection.CellForItem(item) is SliderFeedCollectionViewCell cell)
                        cell.Playback(item.Item == closestToCenterCell.IndexPath.Item);
                }

            }
            base.Scrolled(scrollView);
        }

        public override CGSize GetSizeForItem(UICollectionView collectionView, UICollectionViewLayout layout, NSIndexPath indexPath)
        {
            return new CGSize(UIScreen.MainScreen.Bounds.Width - 15 * 2, collectionView.Frame.Height);
        }
    }

    public class CardCollectionViewFlowDelegate : UICollectionViewDelegateFlowLayout
    {
        public Action CardsScrolled;

        public override void Scrolled(UIScrollView scrollView)
        {
            CardsScrolled?.Invoke();
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

    public class PhotoCollectionViewFlowDelegate : UICollectionViewDelegateFlowLayout
    {
        public Action<ActionType, Tuple<NSIndexPath, PHAsset>> CellClicked;
        private readonly PhotoCollectionViewSource _vs;
        private const byte postLimit = 7;

        public PhotoCollectionViewFlowDelegate(PhotoCollectionViewSource viewSource)
        {
            _vs = viewSource;
        }

        public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var pa = _vs.GetPHAsset((int)indexPath.Item);
            if (pa != null)
            {
                if (_vs.ImageAssets.Count >= postLimit && !_vs.ImageAssets.Any(a => a.Asset.LocalIdentifier == pa.LocalIdentifier))
                {
                    CellClicked?.Invoke(ActionType.Close, new Tuple<NSIndexPath, PHAsset>(indexPath, null));
                    return;
                }

                CellClicked?.Invoke(ActionType.Preview, new Tuple<NSIndexPath, PHAsset>(indexPath, pa));

                var index = _vs.ImageAssets.FindIndex(a => a.Asset.LocalIdentifier == pa.LocalIdentifier);

                if (_vs.CurrentlySelectedItem.Item1 != null)
                    ((PhotoCollectionViewCell)collectionView.CellForItem(_vs.CurrentlySelectedItem.Item1))?.ToggleCell(false);
                ((PhotoCollectionViewCell)collectionView.CellForItem(indexPath))?.ToggleCell(true);

                _vs.CurrentlySelectedItem = new Tuple<NSIndexPath, PHAsset>(indexPath, pa);
            }
        }
    }
}
