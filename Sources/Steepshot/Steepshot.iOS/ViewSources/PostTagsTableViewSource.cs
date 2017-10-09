using System;
using Foundation;
using Steepshot.Core.Presenters;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public delegate void RowSelectedHandler(int row);
    public class PostTagsTableViewSource : UITableViewSource
    {
        private const string CellIdentifier = "PostTagsCell";
        private readonly TagsPresenter _tagsPresenter;
        public event RowSelectedHandler RowSelectedEvent;

        public PostTagsTableViewSource(TagsPresenter tagsPresenter)
        {
            _tagsPresenter = tagsPresenter;
        }
        
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _tagsPresenter.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (UITableViewCell)tableView.DequeueReusableCell(CellIdentifier, indexPath);
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;
            var tag = _tagsPresenter[indexPath.Row];  //TODO:KOA: if null?
            cell.TextLabel.Text = tag?.Name;
            return cell;
        }

        public override void RowHighlighted(UITableView tableView, NSIndexPath rowIndexPath)
        {
            RowSelectedEvent?.Invoke(rowIndexPath.Row);
        }
    }
}
