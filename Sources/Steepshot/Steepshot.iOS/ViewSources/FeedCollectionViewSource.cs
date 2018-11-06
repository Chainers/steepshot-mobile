using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using Steepshot.iOS.Delegates;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class FeedCollectionViewSource : UICollectionViewSource
    {
        public bool IsGrid = false;
        public event Action<ActionType, Post> CellAction;
        public event Action<ActionType> ProfileAction;
        public event Action<string> TagAction;
        protected readonly BasePostPresenter _presenter;
        private readonly List<NewFeedCollectionViewCell> _feedCellsList = new List<NewFeedCollectionViewCell>();
        private readonly CollectionViewFlowDelegate _flowDelegate;

        public FeedCollectionViewSource(BasePostPresenter presenter, CollectionViewFlowDelegate flowDelegate)
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
            if (_presenter.Count == (_flowDelegate.IsProfile ? (int)indexPath.Item - 1 : (int)indexPath.Item) && !_presenter.IsLastReaded)
            {
                var loader = (LoaderCollectionCell)collectionView.DequeueReusableCell(nameof(LoaderCollectionCell), indexPath);
                loader.SetLoader();
                return loader;
            }
            else
            {
                if (indexPath.Row == 0 && _flowDelegate.IsProfile)
                {
                    var profile = (ProfileHeaderViewCell)collectionView.DequeueReusableCell(nameof(ProfileHeaderViewCell), indexPath);

                    if (profile.ContentView.Subviews.Length == 0)
                        profile.ContentView.AddSubview(_flowDelegate.profileCell);

                    if (!_flowDelegate.profileCell.IsProfileActionSet)
                        _flowDelegate.profileCell.ProfileAction += ProfileAction;

                    return profile;
                }
                else
                {
                    UICollectionViewCell cell;
                    var post = _presenter[_flowDelegate.IsProfile ? (int)indexPath.Item - 1 : (int)indexPath.Item];

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
                            ((NewFeedCollectionViewCell)cell).Cell.UpdateCell(post, _flowDelegate.Variables[_flowDelegate.IsProfile ? (int)indexPath.Item - 1 : (int)indexPath.Item]);

                        if (!((NewFeedCollectionViewCell)cell).Cell.IsCellActionSet)
                        {
                            ((NewFeedCollectionViewCell)cell).Cell.CellAction += CellAction;
                            ((NewFeedCollectionViewCell)cell).Cell.TagAction += TagAction;
                            ((NewFeedCollectionViewCell)cell).Cell.MuteAction += VolumeChanged;
                        }
                        if (!_feedCellsList.Any(c => c.Handle == cell.Handle))
                            _feedCellsList.Add((NewFeedCollectionViewCell)cell);
                    }

                    return cell;
                }
            }
        }

        private void SourceChanged(Status obj)
        {
            foreach (var item in _presenter)
            {
                if (IsGrid)
                    ImageLoader.Preload(item.Media[0], Constants.CellSize.Width);
                else
                {
                    foreach (var mediaModel in item.Media)
                    {
                        ImageLoader.Preload(mediaModel, Constants.ScreenWidth);
                    }
                }
            }
        }

        private void VolumeChanged()
        {
            foreach (var item in _feedCellsList)
                item.Cell.OnVolumeChanged();
        }

        public void FreeAllCells()
        {
            foreach (var item in _feedCellsList)
            {
                item.Cell.CellAction = null;
                item.Cell.TagAction -= TagAction;
                item.Cell.MuteAction -= VolumeChanged;
                item.Cell.ReleaseCell();
            }

            _flowDelegate.profileCell.ReleaseCell();
            _flowDelegate.profileCell.RemoveFromSuperview();
            _flowDelegate.profileCell.ProfileAction -= ProfileAction;
            _flowDelegate.profileCell = null;
            _presenter.SourceChanged -= SourceChanged;
        }
    }
}
