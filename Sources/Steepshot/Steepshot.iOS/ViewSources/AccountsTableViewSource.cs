using System;
using System.Collections.Generic;
using Foundation;
using Steepshot.Core.Authority;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Enums;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class AccountsTableViewSource : UITableViewSource
    {
        public List<UserInfo> Accounts = new List<UserInfo>();
        public Action<ActionType, UserInfo> CellAction;

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (AccountTableViewCell)tableView.DequeueReusableCell(nameof(AccountTableViewCell), indexPath);

            if (!cell.IsCellActionSet)
                cell.CellAction += CellAction;

            cell.UpdateCell(Accounts[indexPath.Row]);
            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return Accounts.Count;
        }
    }
}
