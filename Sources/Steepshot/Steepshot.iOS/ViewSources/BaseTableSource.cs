using System;
using System.Collections.Generic;
using Foundation;
using Steepshot.Core.Presenters;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public abstract class BaseTableSource<T> : UITableViewSource where T : BasePostPresenter
    {
        public Action ScrolledToBottom;
        protected readonly BasePostPresenter _presenter;

        IDictionary<NSIndexPath, nfloat> cellHeights = new Dictionary<NSIndexPath, nfloat>();

        public BaseTableSource(BasePostPresenter presenter)
        {
            _presenter = presenter;
        }

        public override void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
        {
            cellHeights[indexPath] = cell.Frame.Size.Height;
        }

        public override nfloat EstimatedHeight(UITableView tableView, NSIndexPath indexPath)
        {
            return cellHeights.ContainsKey(indexPath) ? cellHeights[indexPath] : 150;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _presenter.Count;
        }
    }
}
