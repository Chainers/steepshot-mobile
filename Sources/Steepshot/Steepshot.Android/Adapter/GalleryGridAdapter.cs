using System;
using Android.Support.V7.Widget;
using Android.Views;
using Steepshot.CustomViews;
using Steepshot.Utils;

namespace Steepshot.Adapter
{
    public class GalleryGridAdapter : RecyclerView.Adapter
    {
        public Action<GalleryMediaModel, int> OnItemSelected;
        private const int ColumnCount = 3;

        private readonly int _cellSize;
        private GalleryMediaModel[] _media;

        public GalleryGridAdapter()
        {
            _cellSize = Style.ScreenWidth / ColumnCount - 2;
        }

        public void SetMedia(GalleryMediaModel[] media)
        {
            _media = media;
            NotifyDataSetChanged();
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var galleryGridHolder = holder as GalleryGridViewHolder;
            galleryGridHolder?.Update(_media[position]);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = new SelectableImageView(parent.Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(_cellSize, _cellSize)
            };
            return new GalleryGridViewHolder(view, OnItemSelected);
        }

        public override int ItemCount => _media?.Length ?? 0;
    }

    public class GalleryGridViewHolder : RecyclerView.ViewHolder
    {
        private readonly Action<GalleryMediaModel, int> _onItemSelected;
        private readonly SelectableImageView _image;
        private GalleryMediaModel _media;

        public GalleryGridViewHolder(View itemView, Action<GalleryMediaModel, int> onItemSelected) : base(itemView)
        {
            _image = (SelectableImageView)itemView;
            _onItemSelected = onItemSelected;
            itemView.Click += ItemViewOnClick;
        }

        private void ItemViewOnClick(object sender, EventArgs eventArgs)
        {
            _onItemSelected?.Invoke(_media, AdapterPosition);
        }

        public void Update(GalleryMediaModel media)
        {
            _media = media;
            _image?.Bind(media);
        }
    }
}