using System.Collections.ObjectModel;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Adapter
{
	public class PostsGridAdapter : RecyclerView.Adapter
	{
		ObservableCollection<Post> Posts;
		private Context context;

		public System.Action<int> Click;

        public PostsGridAdapter(Context context, ObservableCollection<Post> Posts)
        {
            this.context = context;
            this.Posts = Posts;
        }

        public Post GetItem(int position)
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
			Picasso.With(context).Load(Posts[position].Body).NoFade().Resize(context.Resources.DisplayMetrics.WidthPixels / 3 - 2, context.Resources.DisplayMetrics.WidthPixels / 3 - 2).CenterCrop().Into(((ImageViewHolder)holder).Photo);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
			ImageView view = new ImageView(context);
			view.SetScaleType(ImageView.ScaleType.CenterInside);
			view.LayoutParameters = new ViewGroup.LayoutParams(context.Resources.DisplayMetrics.WidthPixels / 3 - 1, context.Resources.DisplayMetrics.WidthPixels / 3 - 1);

			ImageViewHolder vh = new ImageViewHolder(view,Click);
            return vh;
        }

		public class ImageViewHolder : RecyclerView.ViewHolder
        {
            public ImageView Photo { get; private set; }
			System.Action<int> Click;

			public ImageViewHolder(Android.Views.View itemView,System.Action<int> Click) : base(itemView)
            {
				this.Click = Click;
				Photo = (ImageView)itemView;
                Photo.Clickable = true;
                Photo.Click += OnClick;
            }

            private void OnClick(object sender, System.EventArgs e)
            {
                Click.Invoke(AdapterPosition);
            }

		}
    }
}