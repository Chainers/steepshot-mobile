using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Steepshot.Core.Presenters;
using Steepshot.Utils;

namespace Steepshot.Adapter
{
    public class TagsAdapter : RecyclerView.Adapter
    {
        private readonly PostDescriptionPresenter _presenter;
        public Action<int> Click;

        public override int ItemCount => _presenter.Count;


        public TagsAdapter(PostDescriptionPresenter presenter)
        {
            _presenter = presenter;
        }


        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var result = _presenter[position];
            if (result == null)
                return;
            var tag = result.Name;

            ((TagViewHolder)holder).Tag.Text = tag;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyr_search_tag, parent, false);
            var vh = new TagViewHolder(itemView, Click);
            vh.Tag.Typeface = Style.Semibold;
            return vh;
        }
    }

    public class TagViewHolder : RecyclerView.ViewHolder
    {
        public TextView Tag { get; }
        private ViewGroup TagLayout { get; }

        public TagViewHolder(View itemView, Action<int> click) : base(itemView)
        {
            Tag = itemView.FindViewById<TextView>(Resource.Id.tag);
            TagLayout = itemView.FindViewById<ViewGroup>(Resource.Id.tag_layout);
            TagLayout.Click += (sender, e) => click?.Invoke(AdapterPosition);
        }
    }
}