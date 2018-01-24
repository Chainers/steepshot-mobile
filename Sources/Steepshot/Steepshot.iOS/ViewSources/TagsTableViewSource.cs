using System;
using Foundation;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class TagsTableViewSource : UITableViewSource
    {
        private readonly string _cellIdentifier = nameof(TagTableViewCell);
        private TagsPresenter _presenter;
        public Action<ActionType, string> CellAction;

        public TagsTableViewSource(TagsPresenter presenter)
        {
            _presenter = presenter;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (TagTableViewCell)tableView.DequeueReusableCell(_cellIdentifier, indexPath);

            if (!cell.IsCellActionSet)
                cell.CellAction += CellAction;

            cell.UpdateCell(_presenter[indexPath.Row].Name);
            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _presenter.Count;
        }
    }
}
