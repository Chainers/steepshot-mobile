using System;
using Foundation;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class ProfileCollectionViewSource : UICollectionViewSource
    {
        public bool IsGrid = false;
        private readonly BasePostPresenter _presenter;

        public event Action<ActionType, Post> CellAction;

        public ProfileCollectionViewSource(BasePostPresenter presenter)
        {
            _presenter = presenter;
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
                BaseProfileCell cell;
                if (IsGrid)
                    cell = (PhotoCollectionViewCell)collectionView.DequeueReusableCell(nameof(PhotoCollectionViewCell), indexPath);
                else
                {
                    cell = (FeedCollectionViewCell)collectionView.DequeueReusableCell(nameof(FeedCollectionViewCell), indexPath);

                    if (!((FeedCollectionViewCell)cell).IsCellActionSet)
                        ((FeedCollectionViewCell)cell).CellAction += CellAction;
                }
                try
                {
                    var post = _presenter[(int)indexPath.Item];
                    if (post != null)
                        cell.UpdateCell(post);
                }
                //remove that
                catch (Exception ex)
                {
                    
                    //ignore ^^
#if DEBUG
                    Console.WriteLine(ex.Message + ex.StackTrace);
#endif
                }
                return cell;
            }
        }
    }
}
