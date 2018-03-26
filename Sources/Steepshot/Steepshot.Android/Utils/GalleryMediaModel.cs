using System;
using Android.Graphics;
using Steepshot.CustomViews;

namespace Steepshot.Utils
{
    public class GalleryMediaModel
    {
        public Action ModelChanged;
        public string Id { get; set; }
        public string Path { get; set; }
        public string Thumbnail { get; set; }
        private bool _selected;
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
        public ImageParameters Parameters { get; set; }
        public Bitmap PreparedBitmap { get; set; }
    }
}