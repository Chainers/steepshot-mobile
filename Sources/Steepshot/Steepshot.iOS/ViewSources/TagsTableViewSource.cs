using System;
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
        private bool _hidePlus;

        public TagsTableViewSource(TagsPresenter presenter, UITableView tableView, bool hidePlus = false) : base(presenter, tableView)
        {
            _hidePlus = hidePlus;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(_cellIdentifier, indexPath);

            if (!((TagTableViewCell)cell).IsCellActionSet)
            {
                ((TagTableViewCell)cell).CellAction += CellAction;
                ((TagTableViewCell)cell).HidePlus = _hidePlus;
            }

            var tag = Presenter != null
                ? ((TagsPresenter)Presenter)[indexPath.Row].Name
                : _tagPickerFacade[indexPath.Row];

            ((TagTableViewCell)cell).UpdateCell(tag);
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
    }
}
