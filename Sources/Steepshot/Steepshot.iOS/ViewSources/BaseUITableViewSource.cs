using System;
using System.Linq;
using Steepshot.Core.Presenters;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public abstract class BaseUiTableViewSource<T> : UITableViewSource
    {
        public Action ScrolledToBottom;
        protected readonly ListPresenter<T> Presenter;
        private UITableView _table;
        private int _prevPos;

        protected BaseUiTableViewSource(ListPresenter<T> presenter, UITableView table)
        {
            Presenter = presenter;
            _table = table;
        }

        public void ClearPosition()
        {
            _prevPos = 0;
        }

        public override void Scrolled(UIScrollView scrollView)
        {
            if (_table.IndexPathsForVisibleRows.Length > 0)
            {
                var pos = _table.IndexPathsForVisibleRows.Max(c => c.Row);
                if (!Presenter.IsLastReaded && pos > _prevPos)
                {
                    if (pos == Presenter.Count - 1)
                    {
                        _prevPos = pos;
                        ScrolledToBottom?.Invoke();
                    }
                }
            }
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return Presenter.Count;
        }
    }
}
