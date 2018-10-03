using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Steepshot.Core.Authorization;
using Steepshot.Core.Models.Enums;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class AccountsTableViewSource : UITableViewSource
    {
        public List<UserInfo> Accounts = new List<UserInfo>();
        public Action<ActionType, UserInfo> CellAction;
        private readonly List<AccountTableViewCell> _cellsList = new List<AccountTableViewCell>();

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (AccountTableViewCell)tableView.DequeueReusableCell(nameof(AccountTableViewCell), indexPath);

            if (!cell.IsCellActionSet)
                cell.CellAction += CellAction;
            if (!_cellsList.Any(c => c.Handle == cell.Handle))
                _cellsList.Add(cell);
            cell.UpdateCell(Accounts[indexPath.Row]);
            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return Accounts.Count;
        }

        public void FreeAllCells()
        {
            foreach (var item in _cellsList)
            {
                item.CellAction = null;
                item.ReleaseCell();
            }
        }
    }
}
