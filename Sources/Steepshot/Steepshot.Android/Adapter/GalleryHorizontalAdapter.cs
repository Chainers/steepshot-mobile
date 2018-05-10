using System;
using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Steepshot.Core.Models.Common;
using Steepshot.Utils;
using ViewUtils = Steepshot.Utils.ViewUtils;

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
            var width = (int)Math.Round(((Android.App.Activity)parent.Context).WindowManager.DefaultDisplay.Width - BitmapUtils.DpToPixel(25, parent.Resources));
            var height = (int)Math.Round(BitmapUtils.DpToPixel(160, parent.Resources));
            var previewSize = _gallery == null ?
                ViewUtils.CalculateImagePreviewSize(_postMedia[0].Size.Width, _postMedia[0].Size.Height, width, height) :
                ViewUtils.CalculateImagePreviewSize(_gallery[0].PreparedBitmap.Width, _gallery[0].PreparedBitmap.Height, width, height);
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
            _image.SetImageBitmap(model.PreparedBitmap);
        }

        public void Update(MediaModel model)
        {
            var url = model.Thumbnails.Micro;
            Picasso.With(ItemView.Context).Load(url).Into(_image);
        }
    }
}