using System;
using System.Collections.Generic;
using Foundation;
using Sweetshot.Library.Models.Responses;
using UIKit;

namespace Steepshot.iOS
{
	public class VotersTableViewSource : BaseTableSource<VotersResult>
	{
		private const string CellIdentifier = "UsersSearchViewCell";
		public event RowSelectedHandler RowSelectedEvent;

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			var cell = (UsersSearchViewCell)tableView.DequeueReusableCell(CellIdentifier, indexPath);
			cell.UpdateCell(TableItems[indexPath.Row]);

			return cell;
		}

		public override void RowHighlighted(UITableView tableView, NSIndexPath rowIndexPath)
		{
			RowSelectedEvent(rowIndexPath.Row);
		}
	}
}
