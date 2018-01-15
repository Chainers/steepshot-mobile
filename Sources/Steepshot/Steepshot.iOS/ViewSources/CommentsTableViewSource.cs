using System;
using Foundation;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class CommentsTableViewSource : BaseTableSource<CommentsPresenter>
    {
        string _cellIdentifier = nameof(CommentTableViewCell);
        public event Action<ActionType, Post> CellAction;

        public CommentsTableViewSource(BasePostPresenter presenter) : base(presenter)
        {
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (CommentTableViewCell)tableView.DequeueReusableCell(_cellIdentifier, indexPath);

            if (!cell.IsCellActionSet)
                cell.CellAction += CellAction;

            cell.UpdateCell(_presenter[indexPath.Row]);
            return cell;
        }
    }
}
