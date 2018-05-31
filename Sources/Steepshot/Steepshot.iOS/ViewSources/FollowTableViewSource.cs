using System;
using Foundation;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class FollowTableViewSource : BaseUITableViewSource
    {
        string _cellIdentifier = nameof(FollowViewCell);
        string _loaderCellIdentifier = nameof(LoaderCell);
        public Action<ActionType, UserFriend> CellAction;

        public FollowTableViewSource(UserFriendPresenter presenter, UITableView table) : base(presenter, table) { }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCell cell;
            if (Presenter.Count == indexPath.Row)
            {
                cell = (LoaderCell)tableView.DequeueReusableCell(_loaderCellIdentifier, indexPath);
            }
            else
            {
                cell = (FollowViewCell)tableView.DequeueReusableCell(_cellIdentifier, indexPath);
                if (!((FollowViewCell)cell).IsCellActionSet)
                    ((FollowViewCell)cell).CellAction += CellAction;

                var user = ((UserFriendPresenter)Presenter)[indexPath.Row];
                if (user != null)
                    ((FollowViewCell)cell).UpdateCell(user);
            }
            return cell;
        }
    }
}
