using System;
using System.Linq;
using Steepshot.Core.Interfaces;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public abstract class BaseUITableViewSource : UITableViewSource
    {
        public Action ScrolledToBottom;
        protected readonly IListPresenter Presenter;
        private UITableView _table;
        private int _prevPos;
        protected string loaderCellIdentifier = nameof(LoaderCell);

        protected BaseUITableViewSource(IListPresenter presenter, UITableView table)
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
                var pos = _table.IndexPathsForVisibleRows.Last().Row;
                if (!Presenter.IsLastReaded && pos > _prevPos)
                {
                    if (pos + 1 == Presenter.Count)
                    {
                        _prevPos = pos;
                        ScrolledToBottom?.Invoke();
                    }
                }
            }
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            var count = Presenter.Count;
            return count == 0 || Presenter.IsLastReaded ? count : count + 1;
        }
    }
}
