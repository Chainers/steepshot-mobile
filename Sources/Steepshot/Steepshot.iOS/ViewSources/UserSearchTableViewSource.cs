using System;
using System.Collections.Generic;
using Foundation;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
	public class UserSearchTableViewSource : UITableViewSource
	{
		public List<UserFriend> Users = new List<UserFriend>();
		private const string CellIdentifier = nameof(UsersSearchViewCell);
		public event RowSelectedHandler RowSelectedEvent;

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			return Users.Count;
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			var cell = (UsersSearchViewCell)tableView.DequeueReusableCell(CellIdentifier, indexPath);
			cell.UpdateCell(Users[indexPath.Row]);

			return cell;
		}

		public override void RowHighlighted(UITableView tableView, NSIndexPath rowIndexPath)
		{
			RowSelectedEvent(rowIndexPath.Row);
		}
	}
}
