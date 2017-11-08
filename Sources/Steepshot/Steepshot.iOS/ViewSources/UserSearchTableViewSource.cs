using System;
using Foundation;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class UserSearchTableViewSource : UITableViewSource
    {
        private const string CellIdentifier = nameof(UsersSearchViewCell);
        private readonly UserFriendPresenter _presenter;
        public event RowSelectedHandler RowSelectedEvent;

        public UserSearchTableViewSource(UserFriendPresenter presenter)
        {
            _presenter = presenter;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _presenter.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (UsersSearchViewCell)tableView.DequeueReusableCell(CellIdentifier, indexPath);
            var user = _presenter[indexPath.Row]; //TODO:KOA: if null?
            if (user != null)
                cell.UpdateCell(user);
            return cell;
        }

        public override void RowHighlighted(UITableView tableView, NSIndexPath rowIndexPath)
        {
            if (RowSelectedEvent != null) RowSelectedEvent(rowIndexPath.Row);
        }
    }
}
