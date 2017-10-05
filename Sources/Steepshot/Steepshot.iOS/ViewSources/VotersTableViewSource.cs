using Foundation;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class VotersTableViewSource : BaseUiTableViewSource<VotersPresenter>
    {
        private const string CellIdentifier = nameof(UsersSearchViewCell);
        public event RowSelectedHandler RowSelectedEvent;

        public VotersTableViewSource(VotersPresenter presenter) : base(presenter) { }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (UsersSearchViewCell)tableView.DequeueReusableCell(CellIdentifier, indexPath);
            cell.UpdateCell(Presenter[indexPath.Row]);
            return cell;
        }

        public override void RowHighlighted(UITableView tableView, NSIndexPath rowIndexPath)
        {
            RowSelectedEvent?.Invoke(rowIndexPath.Row);
        }
    }
}
