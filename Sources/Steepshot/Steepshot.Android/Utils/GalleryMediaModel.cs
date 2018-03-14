using System;
using Newtonsoft.Json;

namespace Steepshot.Utils
{
    public class GalleryMediaModel
    {
        [JsonIgnore]
        public Action ModelChanged;
        public string Id { get; set; }
        public string Path { get; set; }
        public string Thumbnail { get; set; }
        [JsonIgnore]
        private bool _selected;
        [JsonIgnore]
        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                ModelChanged?.Invoke();
            }
        }
        private int _selectionPosition = (int)GallerySelectionType.None;
        public int SelectionPosition
        {
            get => _selectionPosition;
            set
            {
                _selectionPosition = value;
                ModelChanged?.Invoke();
            }
        }
    }
}