using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Steepshot.Core.Facades;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class TagsTableViewSource : BaseUITableViewSource
    {
        private readonly string _cellIdentifier = nameof(TagTableViewCell);
        private readonly TagPickerFacade _tagPickerFacade; // need to call SetClient() after initializing
        public Action<ActionType, string> CellAction;
        private readonly bool _hidePlus;
        private readonly List<TagTableViewCell> _cellsList = new List<TagTableViewCell>();

        public TagsTableViewSource(TagsPresenter presenter, UITableView tableView, bool hidePlus = false) : base(presenter, tableView)
        {
            _hidePlus = hidePlus;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (TagTableViewCell)tableView.DequeueReusableCell(_cellIdentifier, indexPath);

            if (!cell.IsCellActionSet)
            {
                cell.CellAction += CellAction;
                cell.HidePlus = _hidePlus;
            }

            var tag = Presenter != null
                ? ((TagsPresenter)Presenter)[indexPath.Row].Name
                : _tagPickerFacade[indexPath.Row];

            cell.UpdateCell(tag);

            if (!_cellsList.Any(c => c.Handle == cell.Handle))
                _cellsList.Add(cell);
            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return Presenter != null ? Presenter.Count : _tagPickerFacade.Count;
        }

        public NSIndexPath IndexOfTag(string tag)
        {
            var index = Presenter != null
                ? ((TagsPresenter)Presenter).FindIndex(t => t.Name == tag)
                : _tagPickerFacade.IndexOf(tag);
            if (index == -1)
                return null;
            return NSIndexPath.FromItemSection(index, 0);
        }

        public void FreeAllCells()
        {
            foreach (var item in _cellsList)
            {
                item.CellAction = null;
                item.RemoveEvents();
            }
        }
    }
}
