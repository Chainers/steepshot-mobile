using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Presenters
{
    public class TagPickerPresenter : TagsPresenter
    {
        private readonly ObservableCollection<string> _localTags;
        private List<SearchResult> _filteredTags = new List<SearchResult>();
        public override int Count => _filteredTags.Count;

        public override SearchResult this[int position]
        {
            get
            {
                if (position > -1 && position < _filteredTags.Count)
                    return _filteredTags[position];
                return default(SearchResult);
            }
        }

        public TagPickerPresenter(ObservableCollection<string> localTags)
        {
            _localTags = localTags;
            _localTags.CollectionChanged += UpdateFilteredTags;
        }

        private void UpdateFilteredTags(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _filteredTags = Items.Where(tag => !_localTags.Any(localTag => localTag.Equals(tag.Name))).ToList();
        }

        public override int FindIndex(Predicate<SearchResult> match)
        {
            var index = _filteredTags.FindIndex(match);
            if (index == -1)
                index = Items.FindIndex(match);
            return index;
        }

        internal override void NotifySourceChanged(string sender, bool isChanged)
        {
            UpdateFilteredTags(null, null);
            base.NotifySourceChanged(sender, isChanged);
        }
    }
}
