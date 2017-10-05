using System;
using System.Linq;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Steepshot.Core.Presenters;

namespace Steepshot.Adapter
{
    public class PostsGridAdapter : RecyclerView.Adapter
    {
        protected readonly BaseFeedPresenter Presenter;
        protected readonly Context Context;
        public Action<int> Click;
        private int _cellSize;

        public PostsGridAdapter(Context context, BaseFeedPresenter presenter)
        {
            Context = context;
            Presenter = presenter;
            _cellSize = (Context.Resources.DisplayMetrics.WidthPixels - 2 * 4) / 3;
        }
        
        public override int ItemCount => Presenter.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var post = Presenter[position];
            if (post == null)
                return;

            var photo = post.Photos?.FirstOrDefault();//.Photos?.FirstOrDefault();
            if (photo == null)
                return;
            Picasso.With(Context).Load(photo)
                .NoFade()
                .Resize(Context.Resources.DisplayMetrics.WidthPixels / 3 - 2, Context.Resources.DisplayMetrics.WidthPixels / 3 - 2)
                .CenterCrop()
                .Priority(Picasso.Priority.Low)
                .Into(((ImageViewHolder)holder).Photo);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = new ImageView(Context);
            view.SetScaleType(ImageView.ScaleType.CenterInside);
            view.LayoutParameters = new ViewGroup.LayoutParams(_cellSize, _cellSize);

            var vh = new ImageViewHolder(view, Click);
            return vh;
        }
    }

    public class ImageViewHolder : RecyclerView.ViewHolder
    {
        public ImageView Photo { get; }
        protected readonly Action<int> Click;

        public ImageViewHolder(View itemView, Action<int> click) : base(itemView)
        {
            Click = click;
            Photo = (ImageView)itemView;
            Photo.Clickable = true;
            Photo.Click += OnClick;
        }

        protected virtual void OnClick(object sender, EventArgs e)
        {
            Click.Invoke(AdapterPosition);
        }
    }
}
