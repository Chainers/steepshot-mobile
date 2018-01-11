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
    public class FollowTableViewSource : BaseUiTableViewSource<UserFriend>
    {
        string _cellIdentifier = nameof(FollowViewCell);
        public Action<ActionType, UserFriend> CellAction;

        public FollowTableViewSource(UserFriendPresenter presenter, UITableView table) : base(presenter, table) { }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (FollowViewCell)tableView.DequeueReusableCell(_cellIdentifier, indexPath);
            if (!cell.IsCellActionSet)
                cell.CellAction += CellAction;

            var user = Presenter[indexPath.Row];
            if (user != null)
                cell.UpdateCell(user);
            return cell;
        }
    }
}
