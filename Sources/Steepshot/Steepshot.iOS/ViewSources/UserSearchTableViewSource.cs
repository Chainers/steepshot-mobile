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
        private readonly FollowersPresenter _followersPresenter;
        public event RowSelectedHandler RowSelectedEvent;

        public UserSearchTableViewSource(FollowersPresenter followersPresenter)
        {
            _followersPresenter = followersPresenter;
        }
        
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _followersPresenter.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (UsersSearchViewCell)tableView.DequeueReusableCell(CellIdentifier, indexPath);
            var user = _followersPresenter[indexPath.Row]; //TODO:KOA: if null?
            cell.UpdateCell(user);
            return cell;
        }

        public override void RowHighlighted(UITableView tableView, NSIndexPath rowIndexPath)
        {
            if (RowSelectedEvent != null) RowSelectedEvent(rowIndexPath.Row);
        }
    }
}
