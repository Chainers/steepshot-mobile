using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Steepshot.Core.Models.Common;

namespace Steepshot.Adapter
{
    public class PostsGridAdapter : RecyclerView.Adapter
    {
        protected readonly List<Post> _posts;
        protected readonly Context _context;
        public Action<int> Click;
        private int _cellSize;

        public PostsGridAdapter(Context context, List<Post> posts)
        {
            _context = context;
            _posts = posts;
            _cellSize = (_context.Resources.DisplayMetrics.WidthPixels - 2 * 4) / 3;
        }

        public Post GetItem(int position)
        {
            return _posts[position];
        }

        public override int ItemCount => _posts.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var photo = _posts[position].Photos?.FirstOrDefault();
            if (photo != null)
            {
                Picasso.With(_context).Load(photo)
                    .NoFade()
                    .Resize(_context.Resources.DisplayMetrics.WidthPixels / 3 - 2, _context.Resources.DisplayMetrics.WidthPixels / 3 - 2)
                    .CenterCrop()
                    .Priority(Picasso.Priority.Low)
                    .Into(((ImageViewHolder)holder).Photo);
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = new ImageView(_context);
            view.SetScaleType(ImageView.ScaleType.CenterInside);
            view.LayoutParameters = new ViewGroup.LayoutParams(_cellSize, _cellSize);

            var vh = new ImageViewHolder(view, Click);
            return vh;
        }
    }

    public class ImageViewHolder : RecyclerView.ViewHolder
    {
        public ImageView Photo { get; }
        protected readonly Action<int> _click;

        public ImageViewHolder(View itemView, Action<int> click) : base(itemView)
        {
            _click = click;
            Photo = (ImageView)itemView;
            Photo.Clickable = true;
            Photo.Click += OnClick;
        }

        protected virtual void OnClick(object sender, EventArgs e)
        {
            _click.Invoke(AdapterPosition);
        }
    }
}
