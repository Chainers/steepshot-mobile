using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.Cells;
using Steepshot.iOS.Delegates;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class SliderCollectionViewSource : UICollectionViewSource
    {
        public event Action<ActionType, Post> CellAction;
        public event Action<string> TagAction;
        private readonly BasePostPresenter _presenter;
        private readonly SliderCollectionViewFlowDelegate _flowDelegate;
        private readonly List<SliderFeedCollectionViewCell> _feedCellsList = new List<SliderFeedCollectionViewCell>();

        public SliderCollectionViewSource(BasePostPresenter presenter, SliderCollectionViewFlowDelegate flowDelegate)
        {
            _presenter = presenter;
            _presenter.SourceChanged += SourceChanged;
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
                var post = _presenter[(int)indexPath.Item];

                var cell = (SliderFeedCollectionViewCell)collectionView.DequeueReusableCell(nameof(SliderFeedCollectionViewCell), indexPath);

                var offset = collectionView.GetLayoutAttributesForItem(indexPath).Frame.X - 15 - collectionView.ContentOffset.X;

                if (post != null)
                    cell.UpdateCell(post, _flowDelegate.Variables[(int)indexPath.Item], offset);

                if (!cell.IsCellActionSet)
                {
                    cell.CellAction += CellAction;
                    cell.TagAction += TagAction;
                }
                if (!_feedCellsList.Any(c => c.Handle == cell.Handle))
                    _feedCellsList.Add(cell);
                return cell;
            }
        }

        private void SourceChanged(Status obj)
        {
            foreach (var item in _presenter)
            {
                foreach (var mediaModel in item.Media)
                {
                    ImageLoader.Preload(mediaModel, Constants.ScreenWidth);
                }
            }
        }

        public void FreeAllCells()
        {
            foreach (var item in _feedCellsList)
            {
                item.CellAction -= CellAction;
                item.TagAction -= TagAction;
                item.ReleaseCell();
            }
            _presenter.SourceChanged -= SourceChanged;
        }
    }
}
