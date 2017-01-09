using Android.Support.V7.Widget;
using Android.Views;
using Steemix.Library.Models.Responses;
using Android.Widget;
using Square.Picasso;
using Android.Content;
using Android.Text;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Android.Graphics;
using Android.Net;
using Java.IO;

namespace Steemix.Android.Adapter
{

	public class GalleryAdapter : RecyclerView.Adapter
	{
		List<string> Posts;
		private Context context;
		string CommentPattern ="<b>{0}</b> {1}";

		public System.Action<int> Click;

        public GalleryAdapter(Context context, List<string> Posts)
        {
            this.context = context;
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
			Picasso.With(context).Load(new File(Posts[position])).Resize(context.Resources.DisplayMetrics.WidthPixels / 3, context.Resources.DisplayMetrics.WidthPixels / 3).CenterCrop().Into(((ImageViewHolder)holder).Photo);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
			ImageView view = new ImageView(context);
			view.SetScaleType(ImageView.ScaleType.CenterInside);
			view.LayoutParameters = new ViewGroup.LayoutParams(context.Resources.DisplayMetrics.WidthPixels / 3, context.Resources.DisplayMetrics.WidthPixels / 3);

			ImageViewHolder vh = new ImageViewHolder(view,Click);
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
            }

		}
    }
}