using System;
using Steepshot.Core.Presenters;
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
}
