using System;
using Foundation;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class CommentsTableViewSource : BaseTableSource<CommentsPresenter>
    {
        private readonly string _cellIdentifier = nameof(CommentTableViewCell);
        private readonly string _descriptionCellIdentifier = nameof(DescriptionTableViewCell);
        public event Action<ActionType, Post> CellAction;
        public event Action<string> TagAction;
        private Post post;

        public CommentsTableViewSource(BasePostPresenter presenter, Post post) : base(presenter)
        {
            this.post = post;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            if (indexPath.Row == 0)
            {
                if (_presenter.IsLastReaded)
                {
                    var cell = (DescriptionTableViewCell)tableView.DequeueReusableCell(_descriptionCellIdentifier, indexPath);
                    cell.UpdateCell(post, TagAction);

                    return cell;
                }
                else
                    return new UITableViewCell();
            }
            else
            {
                var cell = (CommentTableViewCell)tableView.DequeueReusableCell(_cellIdentifier, indexPath);

                if (!cell.IsCellActionSet)
                    cell.CellAction += CellAction;

                cell.UpdateCell(_presenter[indexPath.Row - 1]);

                return cell;
            }
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _presenter.Count + 1;
        }
    }
}
