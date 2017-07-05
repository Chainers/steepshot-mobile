using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Android.Content;
using System.Collections.Generic;
using Java.IO;

namespace Steepshot
{

	public class GalleryAdapter : RecyclerView.Adapter
	{
		List<string> Posts = new List<string>();
		private Context context;
		string CommentPattern ="<b>{0}</b> {1}";

		public System.Action<int> PhotoClick;

        public GalleryAdapter(Context context)
        {
            this.context = context;
        }

		public void Clear()
		{
			Posts.Clear();
		}

		public void Reset(List<string> Posts)
		{
			this.Posts = Posts;
		}

        public string GetItem(int position)
        {
             return Posts[position];
        }

        public override int ItemCount
        {
            get
            {
                return Posts.Count;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
			Picasso.With(context).Load(new File(Posts[position]))
			       .MemoryPolicy(MemoryPolicy.NoCache, MemoryPolicy.NoStore)
			       .Resize(context.Resources.DisplayMetrics.WidthPixels / 3, context.Resources.DisplayMetrics.WidthPixels / 3)
			       .CenterCrop()
			       .Into(((ImageViewHolder)holder).Photo);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
			ImageView view = new ImageView(context);
			view.SetScaleType(ImageView.ScaleType.CenterInside);
			view.LayoutParameters = new ViewGroup.LayoutParams(context.Resources.DisplayMetrics.WidthPixels / 3, context.Resources.DisplayMetrics.WidthPixels / 3);

			ImageViewHolder vh = new ImageViewHolder(view,PhotoClick);
            return vh;
        }

		public class ImageViewHolder : RecyclerView.ViewHolder
        {
            public ImageView Photo { get; private set; }
			System.Action<int> Click;

			public ImageViewHolder(View itemView,System.Action<int> Click) : base(itemView)
            {
				this.Click = Click;
				Photo = (ImageView)itemView;

				Photo.Clickable = true;
				Photo.Click += (sender, e) => Click.Invoke(AdapterPosition);
            }

		}
    }
}