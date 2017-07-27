using System.Collections.Generic;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Java.IO;
using Square.Picasso;

namespace Steepshot.Adapter
{

	public class GalleryAdapter : RecyclerView.Adapter
	{
		List<string> _posts = new List<string>();
		private Context _context;
		string _commentPattern ="<b>{0}</b> {1}";

		public System.Action<int> PhotoClick;

        public GalleryAdapter(Context context)
        {
            _context = context;
        }

		public void Clear()
		{
			_posts.Clear();
		}

		public void Reset(List<string> posts)
		{
			_posts = posts;
		}

        public string GetItem(int position)
        {
             return _posts[position];
        }

        public override int ItemCount
        {
            get
            {
                return _posts.Count;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
			Picasso.With(_context).Load(new File(_posts[position]))
			       .MemoryPolicy(MemoryPolicy.NoCache, MemoryPolicy.NoStore)
			       .Resize(_context.Resources.DisplayMetrics.WidthPixels / 3, _context.Resources.DisplayMetrics.WidthPixels / 3)
			       .CenterCrop()
			       .Into(((ImageViewHolder)holder).Photo);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
			ImageView view = new ImageView(_context);
			view.SetScaleType(ImageView.ScaleType.CenterInside);
			view.LayoutParameters = new ViewGroup.LayoutParams(_context.Resources.DisplayMetrics.WidthPixels / 3, _context.Resources.DisplayMetrics.WidthPixels / 3);

			ImageViewHolder vh = new ImageViewHolder(view,PhotoClick);
            return vh;
        }

		public class ImageViewHolder : RecyclerView.ViewHolder
        {
            public ImageView Photo { get; private set; }
			System.Action<int> _click;

			public ImageViewHolder(Android.Views.View itemView,System.Action<int> click) : base(itemView)
            {
				_click = click;
				Photo = (ImageView)itemView;

				Photo.Clickable = true;
				Photo.Click += (sender, e) => click.Invoke(AdapterPosition);
            }

		}
    }
}