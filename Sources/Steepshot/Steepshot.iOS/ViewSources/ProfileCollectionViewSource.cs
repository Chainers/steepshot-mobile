using System;
using Foundation;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class ProfileCollectionViewSource : UICollectionViewSource
    {
        public bool IsGrid = false;
        public event Action<ActionType, Post> CellAction;
        public event Action<string> TagAction;
        private readonly BasePostPresenter _presenter;
        private CollectionViewFlowDelegate _flowDelegate;

        public ProfileCollectionViewSource(BasePostPresenter presenter, CollectionViewFlowDelegate flowDelegate)
        {
            _presenter = presenter;
            _flowDelegate = flowDelegate;
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            var count = _presenter.Count;
            return count == 0 || _presenter.IsLastReaded ? count : count + 1;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            if (_presenter.Count == indexPath.Row && !_presenter.IsLastReaded)
            {
                var loader = (LoaderCollectionCell)collectionView.DequeueReusableCell(nameof(LoaderCollectionCell), indexPath);
                loader.SetLoader();
                return loader;
            }
            else
            {
                UICollectionViewCell cell;
                var post = _presenter[(int)indexPath.Item];
                if (IsGrid)
                {
                    cell = (PhotoCollectionViewCell)collectionView.DequeueReusableCell(nameof(PhotoCollectionViewCell), indexPath);
                    if (post != null)
                        ((PhotoCollectionViewCell)cell).UpdateCell(post);
                }
                else
                {
                    cell = (NewFeedCollectionViewCell)collectionView.DequeueReusableCell(nameof(NewFeedCollectionViewCell), indexPath);

                    if (post != null)
                        ((NewFeedCollectionViewCell)cell).Cell.UpdateCell(post, _flowDelegate.Variables[(int)indexPath.Item]);

                    if (!((NewFeedCollectionViewCell)cell).Cell.IsCellActionSet)
                    {
                        ((NewFeedCollectionViewCell)cell).Cell.CellAction += CellAction;
                        ((NewFeedCollectionViewCell)cell).Cell.TagAction += TagAction;
                    }
                }
                return cell;
            }
        }
    }

    public class SliderCollectionViewSource : UICollectionViewSource
    {
        public event Action<ActionType, Post> CellAction;
        public event Action<string> TagAction;
        private readonly BasePostPresenter _presenter;
        private CollectionViewFlowDelegate _flowDelegate;

        public SliderCollectionViewSource(BasePostPresenter presenter, CollectionViewFlowDelegate flowDelegate)
        {
            _presenter = presenter;
            _flowDelegate = flowDelegate;
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            var count = _presenter.Count;
            return count == 0 || _presenter.IsLastReaded ? count : count + 1;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            if (_presenter.Count == indexPath.Row && !_presenter.IsLastReaded)
            {
                var loader = (LoaderCollectionCell)collectionView.DequeueReusableCell(nameof(LoaderCollectionCell), indexPath);
                loader.SetLoader();
                return loader;
            }
            else
            {
                UICollectionViewCell cell;
                var post = _presenter[(int)indexPath.Item];

                cell = (SliderFeedCollectionViewCell)collectionView.DequeueReusableCell(nameof(SliderFeedCollectionViewCell), indexPath);

                if (post != null)
                    ((SliderFeedCollectionViewCell)cell).UpdateCell(post, _flowDelegate.Variables[(int)indexPath.Item]);

                if (!((SliderFeedCollectionViewCell)cell).IsCellActionSet)
                {
                    ((SliderFeedCollectionViewCell)cell).CellAction += CellAction;
                    ((SliderFeedCollectionViewCell)cell).TagAction += TagAction;
                }
                return cell;
            }
        }
    }
}
