using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
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
        private readonly Post _post;
        private readonly List<CommentTableViewCell> _cellsList = new List<CommentTableViewCell>();
        private DescriptionTableViewCell _descriptionCell;

        public CommentsTableViewSource(BasePostPresenter presenter, Post post) : base(presenter)
        {
            _post = post;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            if (indexPath.Row == 0)
            {
                if (_presenter.IsLastReaded)
                {
                    _descriptionCell = (DescriptionTableViewCell)tableView.DequeueReusableCell(_descriptionCellIdentifier, indexPath);
                    _descriptionCell.Initialize(_post, TagAction);
                    return _descriptionCell;
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
                if (!_cellsList.Any(c => c.Handle == cell.Handle))
                    _cellsList.Add(cell);
                return cell;
            }
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _presenter.Count + 1;
        }

        public void FreeAllCells()
        {
            foreach (var item in _cellsList)
            {
                item.CellAction = null;
                item.ReleaseCell();
            }
            TagAction = null;
            _descriptionCell?.ReleaseCell();
        }
    }
}
