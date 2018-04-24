using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Steepshot.CustomViews;
using Steepshot.Utils;

namespace Steepshot.Adapter
{
    public class GalleryGridAdapter : RecyclerView.Adapter
    {
        public Action<GalleryMediaModel> OnItemSelected;

        private readonly int _cellSize;
        private GalleryMediaModel[] _media;

        public GalleryGridAdapter(Context context)
        {
            _cellSize = context.Resources.DisplayMetrics.WidthPixels / 3 - 2;
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
        private readonly Action<GalleryMediaModel> _onItemSelected;
        private readonly SelectableImageView _image;
        private GalleryMediaModel _media;

        public GalleryGridViewHolder(View itemView, Action<GalleryMediaModel> onItemSelected) : base(itemView)
        {
            _image = (SelectableImageView)itemView;
            _onItemSelected = onItemSelected;
            itemView.Click += ItemViewOnClick;
        }

        private void ItemViewOnClick(object sender, EventArgs eventArgs)
        {
            _onItemSelected?.Invoke(_media);
        }

        public void Update(GalleryMediaModel media)
        {
            _media = media;
            _image?.Bind(media);
        }
    }
}