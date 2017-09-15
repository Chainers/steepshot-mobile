using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Steepshot.Utils;

namespace Steepshot.Adapter
{
    public class GridImageAdapter : RecyclerView.Adapter
    {
        private List<string> _posts;
        private readonly Context _context;
        public Action<int> Click;
        public override int ItemCount => _posts.Count;
        public static Bitmap[] bitmaps;
        private readonly int cellSize;

        public GridImageAdapter(Context context, List<string> posts, int rowCount = 3, int borderPx = 1)
        {
            _context = context;
            _posts = posts;
            cellSize = _context.Resources.DisplayMetrics.WidthPixels / rowCount - borderPx;
        }

        public string GetItem(int position)
        {
            return _posts[position];
        }

        public override async void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var photoUrl = _posts[position];
            var iHolder = (GalleryImageViewHolder)holder;
            iHolder.CurrentPhotoId = position;
            if (bitmaps[position] == null)
            {
                iHolder.Photo.SetImageDrawable(null);
                var bitmap = await PrepareImage(photoUrl);
                bitmaps[position] = bitmap;
                if (iHolder.CurrentPhotoId == position)
                {
                    iHolder.Photo.SetImageBitmap(bitmap);
                }
            }
            else
                iHolder.Photo.SetImageBitmap(bitmaps[position]);
        }

        private Task<Bitmap> PrepareImage(string post)
        {
            return Task.Run(() =>
            {
                var bitmap = BitmapUtils.DecodeSampledBitmapFromResource(post, 150, 150);
                return BitmapUtils.RotateImageIfRequired(bitmap, post);
            });
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = new ImageView(_context);
            view.SetScaleType(ImageView.ScaleType.CenterCrop);
            view.LayoutParameters = new ViewGroup.LayoutParams(cellSize, cellSize);

            return new GalleryImageViewHolder(view, Click);
        }

        public void ClearCache()
        {
            bitmaps = new Bitmap[_posts.Count];
        }
    }

    public class GalleryImageViewHolder : ImageViewHolder
    {
        public int CurrentPhotoId;

        public GalleryImageViewHolder(View itemView, Action<int> click) : base(itemView, click)
        {
        }
    }
}
