using System;
using CoreGraphics;
using Foundation;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class ProfileCollectionViewSource : FeedCollectionViewSource
    {
        public ProfileCollectionViewSource(BasePostPresenter presenter, CollectionViewFlowDelegate flowDelegate) : base(presenter, flowDelegate)
        {
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            var count = _presenter.Count + 1;
            return count == 1 || _presenter.IsLastReaded ? count : count + 1;
        }
    }

    public class FeedCollectionViewSource : UICollectionViewSource
    {
        public bool IsGrid = false;
        public event Action<ActionType, Post> CellAction;
        public event Action<ActionType> ProfileAction;
        public event Action<string> TagAction;
        public UserProfileResponse user;

        protected readonly BasePostPresenter _presenter;
        private CollectionViewFlowDelegate _flowDelegate;

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

                    _flowDelegate.UpdateProfile(null);

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
                        }
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
    }

    public class SliderCollectionViewSource : UICollectionViewSource
    {
        public event Action<ActionType, Post> CellAction;
        public event Action<string> TagAction;
        private readonly BasePostPresenter _presenter;
        private SliderCollectionViewFlowDelegate _flowDelegate;

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
    }
}
