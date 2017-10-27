using System;
using System.Collections.Generic;
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
        public readonly List<string> LocalTags = new List<string>();
        public override int ItemCount => _presenter?.Count ?? LocalTags.Count;
        public bool Enabled = true;

        public TagsAdapter()
        {
            Click += (obj) =>
            {
                if (!Enabled)
                    return;
                LocalTags.RemoveAt(obj);
                NotifyDataSetChanged();
            };
        }

        public TagsAdapter(PostDescriptionPresenter presenter)
        {
            _presenter = presenter;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var tag = string.Empty;
            if (_presenter != null)
            {
                var result = _presenter[position];
                if (result == null)
                    return;
                tag = result.Name;
            }
            else
            {
                if (LocalTags.Count <= position)
                    return;
                tag = LocalTags[position];
            }

            ((TagViewHolder)holder).Tag.Text = tag;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(_presenter != null ? Resource.Layout.lyr_search_tag : Resource.Layout.lyt_local_tags_item, parent, false);
            var vh = new TagViewHolder(itemView, Click);
            vh.Tag.Typeface = Style.Semibold;
            return vh;
        }

        public class TagViewHolder : RecyclerView.ViewHolder
        {
            public TextView Tag { get; }
            private ViewGroup _tagLayout { get; }

            public TagViewHolder(View itemView, Action<int> click) : base(itemView)
            {
                Tag = itemView.FindViewById<TextView>(Resource.Id.tag);
                _tagLayout = itemView.FindViewById<ViewGroup>(Resource.Id.tag_layout);
                _tagLayout.Click += (sender, e) => click?.Invoke(AdapterPosition);
            }
        }
    }
}