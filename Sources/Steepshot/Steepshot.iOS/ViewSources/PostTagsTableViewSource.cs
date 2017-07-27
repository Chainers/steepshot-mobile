using System;
using System.Collections.Generic;
using Foundation;
using Steepshot.Core.Models.Responses;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public delegate void RowSelectedHandler(int row);
    public class PostTagsTableViewSource : UITableViewSource
    {
        public List<SearchResult> Tags = new List<SearchResult>();
        private const string CellIdentifier = "PostTagsCell";
        public event RowSelectedHandler RowSelectedEvent;

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return Tags.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (UITableViewCell)tableView.DequeueReusableCell(CellIdentifier, indexPath);
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			cell.TextLabel.Text = Tags[indexPath.Row].Name;
            return cell;
        }

        public override void RowHighlighted(UITableView tableView, NSIndexPath rowIndexPath)
        {
            RowSelectedEvent(rowIndexPath.Row);
        }
    }
}
