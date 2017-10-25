using System;
using System.Linq;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Steepshot.Core.Presenters;
using Steepshot.Utils;

namespace Steepshot.Adapter
{
    public class GridAdapter : RecyclerView.Adapter
    {
        protected readonly BasePostPresenter Presenter;
        protected readonly Context Context;
        public Action<int> Click;
        protected readonly int _cellSize;
        public override int ItemCount
        {
            get
            {
                var count = Presenter.Count;
                return count == 0 || Presenter.IsLastReaded ? count : count + 1;
            }
        }

        public GridAdapter(Context context, BasePostPresenter presenter)
        {
            Context = context;
            Presenter = presenter;
            _cellSize = Context.Resources.DisplayMetrics.WidthPixels / 3 - 2; // [x+2][1+x+1][2+x]
        }

        public override int GetItemViewType(int position)
        {
            if (Presenter.Count == position)
                return (int)ViewType.Loader;

            return (int)ViewType.Cell;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var post = Presenter[position];
            if (post == null)
                return;

            var photo = post.Photos?.FirstOrDefault();
            if (photo == null)
                return;

            Picasso.With(Context).Load(photo)
                .NoFade()
                .Resize(_cellSize, _cellSize)
                .CenterCrop()
                .Priority(Picasso.Priority.Low)
                .Into(((ImageViewHolder)holder).Photo);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            switch ((ViewType)viewType)
            {
                case ViewType.Loader:
                    var loaderView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.loading_item, parent, false);
                    var loaderVh = new LoaderViewHolder(loaderView);
                    return loaderVh;
                default:
                    var view = new ImageView(Context);
                    view.SetScaleType(ImageView.ScaleType.CenterInside);
                    view.LayoutParameters = new ViewGroup.LayoutParams(_cellSize, _cellSize);
                    return new ImageViewHolder(view, Click);
            }
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
