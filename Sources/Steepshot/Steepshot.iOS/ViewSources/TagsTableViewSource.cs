using System;
using Foundation;
using Steepshot.Core.Facades;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class TagsTableViewSource : UITableViewSource
    {
        private readonly string _cellIdentifier = nameof(TagTableViewCell);
        private readonly TagsPresenter _presenter;
        private readonly TagPickerFacade _tagPickerFacade;
        public Action<ActionType, string> CellAction;

        public TagsTableViewSource(TagPickerFacade tagPickerFacade)
        {
            _tagPickerFacade = tagPickerFacade;
        }

        public TagsTableViewSource(TagsPresenter presenter)
        {
            _presenter = presenter;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (TagTableViewCell)tableView.DequeueReusableCell(_cellIdentifier, indexPath);

            if (!cell.IsCellActionSet)
                cell.CellAction += CellAction;

            var tag = _presenter != null
                ? _presenter[indexPath.Row].Name
                : _tagPickerFacade[indexPath.Row];

            cell.UpdateCell(tag);
            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _presenter.Count;
        }

        public NSIndexPath IndexOfTag(string tag)
        {
            var index = _presenter != null
                ? _presenter.FindIndex(t => t.Name == tag)
                : _tagPickerFacade.IndexOf(tag);
            if (index == -1)
                return null;
            return NSIndexPath.FromItemSection(index, 0);
        }
    }
}
