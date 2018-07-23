using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Steepshot.Core.Extensions;
using Steepshot.Core.Localization;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Utils;
using Steepshot.Core.Utils;

namespace Steepshot.Adapter
{
    public class GridAdapter<T> : RecyclerView.Adapter where T : BasePostPresenter
    {
        protected readonly T Presenter;
        protected readonly Context Context;
        public Action<Post> Click;
        protected readonly int CellSize;
        public override int ItemCount
        {
            get
            {
                var count = Presenter.Count;
                return count == 0 || Presenter.IsLastReaded ? count : count + 1;
            }
        }

        public GridAdapter(Context context, T presenter)
        {
            Context = context;
            CellSize = Context.Resources.DisplayMetrics.WidthPixels / 3 - 2; // [x+2][1+x+1][2+x]   
            Presenter = presenter;
            Presenter.SourceChanged += PresenterOnSourceChanged;
        }

        private void PresenterOnSourceChanged(Status obj)
        {
            foreach (var post in Presenter)
            {
                if (!string.IsNullOrEmpty(post.Media[0].Url))
                    Picasso.With(Context).Load(post.Url.GetProxy(CellSize, CellSize)).Priority(Picasso.Priority.Low).MemoryPolicy(MemoryPolicy.NoCache).Fetch();
            }
        }

        public override void OnDetachedFromRecyclerView(RecyclerView recyclerView)
        {
            Presenter.SourceChanged -= PresenterOnSourceChanged;
            base.OnDetachedFromRecyclerView(recyclerView);
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

            var vh = (ImageViewHolder)holder;
            vh.UpdateData(post, Context, CellSize);
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
                    var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_grid_item, parent, false);
                    view.LayoutParameters = new ViewGroup.LayoutParams(CellSize, CellSize);
                    return new ImageViewHolder(view, Click);
            }
        }
    }

    public sealed class ImageViewHolder : RecyclerView.ViewHolder
    {
        private readonly Action<Post> _click;
        private readonly ImageView _photo;
        private readonly ImageView _gallery;
        private readonly RelativeLayout _nsfwMask;
        private readonly TextView _nsfwMaskMessage;
        private Post _post;
        private Context _context;
        private string _photoString;


        public ImageViewHolder(View itemView, Action<Post> click) : base(itemView)
        {
            _click = click;
            _photo = itemView.FindViewById<ImageView>(Resource.Id.grid_item_photo);
            _gallery = itemView.FindViewById<ImageView>(Resource.Id.gallery);
            _nsfwMask = itemView.FindViewById<RelativeLayout>(Resource.Id.grid_item_nsfw);
            _nsfwMaskMessage = itemView.FindViewById<TextView>(Resource.Id.grid_item_nsfw_message);

            _nsfwMaskMessage.Typeface = Style.Light;

            _photo.Clickable = true;
            _photo.Click += OnClick;
        }

        private void OnClick(object sender, EventArgs e)
        {
            _click.Invoke(_post);
        }

        public void UpdateData(Post post, Context context, int cellSize)
        {
            _post = post;
            _context = context;

            _photoString = post.Media[0].Url;

            if (_photoString != null)
            {
                Picasso.With(_context).Load(_photoString.GetProxy(cellSize, cellSize))
                    .Placeholder(Resource.Color.rgb244_244_246)
                    .NoFade()
                    .Priority(Picasso.Priority.High)
                    .Into(_photo, null, OnError);
            }

            _gallery.Visibility = post.Media.Length > 1 ? ViewStates.Visible : ViewStates.Gone;

            if (_post.ShowMask && (_post.IsNsfw || _post.IsLowRated) && _post.Author != AppSettings.User.Login)
            {
                _nsfwMaskMessage.Text = AppSettings.LocalizationManager.GetText(_post.IsLowRated ? LocalizationKeys.LowRated : LocalizationKeys.Nsfw);
                _nsfwMask.Visibility = ViewStates.Visible;
            }
            else
                _nsfwMask.Visibility = ViewStates.Gone;
        }

        private void OnError()
        {
            Picasso.With(_context).Load(_photoString).Placeholder(Resource.Color.rgb244_244_246).NoFade().Into(_photo);
        }
    }
}
