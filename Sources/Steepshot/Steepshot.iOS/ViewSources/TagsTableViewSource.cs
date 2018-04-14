using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using UIKit;

namespace Steepshot.iOS.ViewSources
{
    public class TagsTableViewSource : UITableViewSource
    {
        private readonly string _cellIdentifier = nameof(TagTableViewCell);
        private TagsPresenter _presenter;
        private List<string> _localTags;
        public Action<ActionType, string> CellAction;
        private List<SearchResult> _filteredTags;

        /*
        public List<SearchResult> filteredTags
        {
            get
            {
                if (_filteredTags == null)
                {
                    _filteredTags = _presenter.Where(tag => !_localTags.Any(localTag => localTag.Equals(tag.Name))).ToList();
                    return _filteredTags;
                }
                else
                    return _filteredTags;
            }
            //private set;
        }*/

        public TagsTableViewSource(TagsPresenter presenter, List<string> localTags = null)
        {
            _presenter = presenter;
            _localTags = localTags;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = (TagTableViewCell)tableView.DequeueReusableCell(_cellIdentifier, indexPath);

            if (!cell.IsCellActionSet)
                cell.CellAction += CellAction;

            if (_localTags == null || _filteredTags == null)
                cell.UpdateCell(_presenter[indexPath.Row].Name);
            else
                cell.UpdateCell(_filteredTags[indexPath.Row].Name);
            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            if (_localTags == null || _filteredTags == null)
                return _presenter.Count;
            else
            {
                return _filteredTags.Count;
            }
        }

        public NSIndexPath IndexOfTag(string obj, bool onRemove = false)
        {
            int ta;

            if (_localTags == null || _filteredTags == null)
                ta = _presenter.FindIndex(t => t.Name == obj);
            else
                ta = _filteredTags.FindIndex(t => t.Name == obj);
            
            return NSIndexPath.FromItemSection(ta, 0);
        }

        public void UpdateFilteredTags()
        {
            _filteredTags = _presenter.Where(tag => !_localTags.Any(localTag => localTag.Equals(tag.Name))).ToList();
        }
    }
}
