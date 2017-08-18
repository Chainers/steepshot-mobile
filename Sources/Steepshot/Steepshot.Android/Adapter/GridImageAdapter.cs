using System.Collections.Generic;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Adapter
{
    public class GridImageAdapter<T> : RecyclerView.Adapter
    {
        private IList<T> _posts;
        private readonly Context _context;
        private readonly int _rowCount;
        private readonly int _borderPx;
        public System.Action<int> Click;

        public GridImageAdapter(Context context, IList<T> posts, int rowCount = 3, int borderPx = 1)
        {
            _context = context;
            _posts = posts;
            _rowCount = rowCount;
            _borderPx = borderPx;
        }

        public T GetItem(int position)
        {
            return _posts[position];
        }

        public override int ItemCount => _posts.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var itm = _posts[position];
            var width = _context.Resources.DisplayMetrics.WidthPixels - _borderPx * (_rowCount + 1) / _rowCount;
            var iHolder = (ImageViewHolder)holder;
            Picasso.With(_context).Load(ToPath(itm))
               .NoFade()
               .Resize(width, width)
               .CenterCrop()
               .Priority(Picasso.Priority.Low)
               .Into(iHolder.Photo);
        }

        public virtual string ToPath(T itm)
        {
            var buf = string.Empty;

            var post = itm as Post;
            if (post != null)
            {
                buf = post.Body;
            }
            else
            {
                var str = itm as string;
                if (str != null)
                    buf = str;
            }

            if (!buf.StartsWith("http") && !buf.StartsWith("file://"))
                buf = "file://" + buf;

            return buf;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var titleSize = _context.Resources.DisplayMetrics.WidthPixels / 3 - 2;

            var view = new ImageView(_context);
            view.SetScaleType(ImageView.ScaleType.CenterInside);
            view.LayoutParameters = new ViewGroup.LayoutParams(titleSize, titleSize);

            return new ImageViewHolder(view, Click);
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

        public void Update(IList<T> imgs)
        {
            _posts.Clear();
            _posts = imgs;
        }
    }
}