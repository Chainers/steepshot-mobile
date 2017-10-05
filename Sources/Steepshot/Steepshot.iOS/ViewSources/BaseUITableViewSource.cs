using System;
using Foundation;
using Steepshot.Core.Presenters;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public abstract class BaseUiTableViewSource<T> : UITableViewSource
        where T : ListPresenter
    {
        public delegate void ScrolledToBottomHandler();
        public event ScrolledToBottomHandler ScrolledToBottom;
        protected readonly T Presenter;

        protected BaseUiTableViewSource(T presenter)
        {
            Presenter = presenter;
        }

        public override void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
        {
            if (ScrolledToBottom != null && indexPath.Row == Presenter.Count - 1)
                ScrolledToBottom();

        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return Presenter.Count;
        }
    }
}
