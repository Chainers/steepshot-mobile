using System.Collections.Generic;
using Android.Graphics;
using Android.Media;
using Android.Provider;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Adapter
{
    public class GalleryMediaAdapter : RecyclerView.Adapter
    {
        private readonly List<GalleryMediaModel> _gallery;

        public GalleryMediaAdapter(List<GalleryMediaModel> gallery)
        {
            _gallery = gallery;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var galleryHolder = (GalleryMediaViewHolder)holder;
            galleryHolder?.Update(_gallery[position]);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var maxWidth = Style.GalleryHorizontalScreenWidth;
            var maxHeight = Style.GalleryHorizontalHeight;

            var previewSize = BitmapUtils.CalculateImagePreviewSize(_gallery[0].Parameters, maxWidth, maxHeight);

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
            return new GalleryMediaViewHolder(cardView);
        }

        public override int ItemCount => _gallery.Count;
    }

    internal class GalleryMediaViewHolder : RecyclerView.ViewHolder
    {
        private readonly ImageView _image;
        public GalleryMediaViewHolder(View itemView) : base(itemView)
        {
            _image = itemView.FindViewById<ImageView>(Resource.Id.photo);
        }

        public void Update(GalleryMediaModel model)
        {
            BitmapUtils.ReleaseBitmap(_image.Drawable);

            _image.SetImageBitmap(null);
            _image.SetImageResource(Style.R245G245B245);

            if (!string.IsNullOrEmpty(model.TempPath))
            {
                Bitmap bitmap = MimeTypeHelper.IsVideo(model.MimeType)
                    ? ThumbnailUtils.CreateVideoThumbnail(model.TempPath, ThumbnailKind.FullScreenKind)
                    : BitmapUtils.DecodeSampledBitmapFromFile(ItemView.Context, Android.Net.Uri.Parse(model.TempPath), Style.GalleryHorizontalScreenWidth, Style.GalleryHorizontalHeight);
                _image.SetImageBitmap(bitmap);
            }
        }
    }
}
