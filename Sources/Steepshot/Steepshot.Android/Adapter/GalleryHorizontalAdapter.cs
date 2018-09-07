using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Steepshot.Core.Models.Common;
using Steepshot.Utils;

namespace Steepshot.Adapter
{
    public class GalleryHorizontalAdapter : RecyclerView.Adapter
    {
        private readonly List<GalleryMediaModel> _gallery;
        private readonly MediaModel[] _postMedia;

        public GalleryHorizontalAdapter(List<GalleryMediaModel> gallery)
        {
            _gallery = gallery;
        }

        public GalleryHorizontalAdapter(Post post)
        {
            _postMedia = post.Media;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var galleryHolder = (GalleryViewHolder)holder;
            if (_gallery != null)
                galleryHolder?.Update(_gallery[position]);
            else
                galleryHolder?.Update(_postMedia[position]);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var maxWidth = Style.GalleryHorizontalScreenWidth;
            var maxHeight = Style.GalleryHorizontalHeight;

            var previewSize = _gallery == null ?
                BitmapUtils.CalculateImagePreviewSize(_postMedia[0].Size.Width, _postMedia[0].Size.Height, maxWidth, maxHeight) :
                BitmapUtils.CalculateImagePreviewSize(_gallery[0].Parameters, maxWidth, maxHeight);

            var cardView = new CardView(parent.Context)
            {
                LayoutParameters = new FrameLayout.LayoutParams(previewSize.Width, previewSize.Height),
                Radius = BitmapUtils.DpToPixel(5, parent.Resources)
            };
            var image = new ImageView(parent.Context)
            {
                Id = Resource.Id.photo,
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
            };
            image.SetScaleType(ImageView.ScaleType.FitXy);
            cardView.AddView(image);
            return new GalleryViewHolder(cardView);
        }

        public override int ItemCount => _gallery?.Count ?? _postMedia.Length;
    }

    class GalleryViewHolder : RecyclerView.ViewHolder
    {
        private readonly ImageView _image;
        public GalleryViewHolder(View itemView) : base(itemView)
        {
            _image = itemView.FindViewById<ImageView>(Resource.Id.photo);
        }

        public void Update(GalleryMediaModel model)
        {
            if (model.UploadState == UploadState.Saved)
                _image.SetImageBitmap(model.PreparedBitmap);
            else
                _image.SetBackgroundColor(Style.R245G245B245);
        }

        public void Update(MediaModel model)
        {
            var url = model.Thumbnails.Micro;
            Picasso.With(ItemView.Context).Load(url).Into(_image);
        }
    }
}