using System;
using Foundation;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class TagsTableViewSource : UITableViewSource
    {
        private readonly string _cellIdentifier = nameof(TagTableViewCell);

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (TagTableViewCell)tableView.DequeueReusableCell(_cellIdentifier, indexPath);
            /*
            if (!cell.IsCellActionSet)
                cell.CellAction += CellAction;

            cell.UpdateCell(_presenter[indexPath.Row]);*/
            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return 20;
            //throw new NotImplementedException();
        }
    }
}
