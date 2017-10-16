using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;

namespace Steepshot.Adapter
{

    public class TagsAdapter : RecyclerView.Adapter
    {
        private PostDescriptionPresenter _presenter;
        public Action<int> Click;
        public List<SearchResult> LocalTags = new List<SearchResult>();
        private readonly Typeface[] _fonts;
        public override int ItemCount => _presenter != null ? _presenter.Count : LocalTags.Count;

        public TagsAdapter(Typeface[] fonts)
        {
            _fonts = fonts;
            Click += (obj) =>
            {
                LocalTags.RemoveAt(obj);
                NotifyDataSetChanged();
            };
        }

        public TagsAdapter(PostDescriptionPresenter presenter, Typeface[] fonts)
        {
            _fonts = fonts;
            _presenter = presenter;
        }

        public IEnumerable<string> GetLocalTags()
        {
            foreach (var item in LocalTags)
            {
                yield return item.Name;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var tag = _presenter != null ?_presenter[position] : LocalTags[position];
            if (tag == null)
                return;
            ((TagViewHolder)holder).Tag.Text = tag.Name;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(_presenter != null ? Resource.Layout.lyr_search_tag : Resource.Layout.lyt_local_tags_item, parent, false);
            var vh = new TagViewHolder(itemView, Click);
            vh.Tag.Typeface = _fonts[1];
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