using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Steepshot.Core.Errors;
using Steepshot.Core.Models;
using Steepshot.Core.Presenters;

namespace Steepshot.Core.Facades
{
    public class TagPickerFacade
    {
        public event Action<Status> SourceChanged;

        private readonly TagsPresenter _tagsPresenter = new TagsPresenter();
        private readonly ObservableCollection<string> _localTags;
        private List<string> _filteredTags = new List<string>();

        public string this[int position]
        {
            get
            {
                if (position > -1 && position < _filteredTags.Count)
                    return _filteredTags[position];
                return string.Empty;
            }
        }

        public TagPickerFacade(ObservableCollection<string> localTags)
        {
            _localTags = localTags;
            _localTags.CollectionChanged += UpdateFilteredTags;
        }

        private void UpdateFilteredTags(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _filteredTags = _tagsPresenter.Where(tag => !_localTags.Any(localTag => localTag.Equals(tag.Name))).Select(i => i.Name).ToList();
            NotifySourceChanged(nameof(Clear), true);
        }

        public int IndexOf(string tag)
        {
            return _filteredTags.IndexOf(tag);
        }

        internal void NotifySourceChanged(string sender, bool isChanged)
        {
            SourceChanged?.Invoke(new Status(sender, isChanged));
        }

        public void Clear()
        {
            var isEmpty = _filteredTags.Count == 0;
            _filteredTags.Clear();
            _tagsPresenter.Clear();

            NotifySourceChanged(nameof(Clear), isEmpty);
        }

        public async Task<ErrorBase> TryGetTopTags()
        {
            var isAdded = false;

            do
            {
                var result = await _tagsPresenter.TryGetTopTags();

                if (result != null)
                    return result;

                foreach (var item in _tagsPresenter)
                {
                    if (!_localTags.Contains(item.Name) && !_filteredTags.Contains(item.Name))
                    {
                        _filteredTags.Add(item.Name);
                        isAdded = true;
                    }
                }

            } while (!(isAdded || _tagsPresenter.IsLastReaded));

            NotifySourceChanged(nameof(TryGetTopTags), isAdded);

            return null;
        }

        public async Task<ErrorBase> TryLoadNext(string tagFieldText)
        {
            var isAdded = false;

            do
            {
                var result = await _tagsPresenter.TryLoadNext(tagFieldText);

                if (result != null)
                    return result;

                foreach (var item in _tagsPresenter)
                {
                    if (!_localTags.Contains(item.Name) && !_filteredTags.Contains(item.Name))
                    {
                        _filteredTags.Add(item.Name);
                        isAdded = true;
                    }
                }

            } while (!(isAdded || _tagsPresenter.IsLastReaded));

            NotifySourceChanged(nameof(TryLoadNext), isAdded);

            return null;
        }
    }
}
