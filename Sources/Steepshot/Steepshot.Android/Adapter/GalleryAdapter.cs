using System.Collections.Generic;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Square.Picasso;

namespace Steepshot.Adapter
{

    public class GalleryAdapter : RecyclerView.Adapter
    {
        private List<string> _posts;
        private readonly Context _context;

        public System.Action<int> PhotoClick;

        public GalleryAdapter(Context context)
        {
            _context = context;
            _posts = new List<string>();
        }

        public void Clear()
        {
            _posts.Clear();
        }

        public void Reset(List<string> posts)
        {
            _posts.Clear();
            _posts = posts;
        }

        public string GetItem(int position)
        {
            return _posts[position];
        }

        public override int ItemCount => _posts.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var iHolder = (ImageViewHolder)holder;
            Picasso.With(_context)
                .Load(_posts[position])
                   .MemoryPolicy(MemoryPolicy.NoCache, MemoryPolicy.NoStore)
                   .Priority(Picasso.Priority.High)
                   .Resize(_context.Resources.DisplayMetrics.WidthPixels / 3, _context.Resources.DisplayMetrics.WidthPixels / 3)
                   .CenterCrop()
                   .NoFade()
                .Into(iHolder.Photo);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = new ImageView(_context);
            view.SetScaleType(ImageView.ScaleType.CenterInside);
            view.LayoutParameters = new ViewGroup.LayoutParams(_context.Resources.DisplayMetrics.WidthPixels / 3, _context.Resources.DisplayMetrics.WidthPixels / 3);

            return new ImageViewHolder(view, PhotoClick);
        }

        public class ImageViewHolder : RecyclerView.ViewHolder
        {
            public ImageView Photo { get; }

            public ImageViewHolder(View itemView, System.Action<int> click) : base(itemView)
            {
                Photo = (ImageView)itemView;
                Photo.Clickable = true;
                Photo.Click += (sender, e) => click.Invoke(AdapterPosition);
            }
        }
    }
}