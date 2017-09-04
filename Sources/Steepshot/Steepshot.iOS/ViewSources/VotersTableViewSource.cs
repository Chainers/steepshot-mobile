using Foundation;
using Steepshot.Core.Models.Responses;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
	public class VotersTableViewSource : BaseTableSource<VotersResult>
	{
		private const string CellIdentifier = nameof(UsersSearchViewCell);
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
