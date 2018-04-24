using System;
using System.Collections.ObjectModel;
using Android.Support.V7.Widget;
using Android.Views;
using Steepshot.Utils;

namespace Steepshot.Adapter
{
    public sealed class SelectedTagsAdapter : RecyclerView.Adapter
    {
        public readonly ObservableCollection<string> LocalTags = new ObservableCollection<string>();
        public bool Enabled = true;
        public Action<string> Click;


        public override int ItemCount => LocalTags.Count;


        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (LocalTags.Count <= position)
                return;
            var tag = LocalTags[position];

            ((TagViewHolder)holder).UpdateData(tag);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_local_tags_item, parent, false);
            var vh = new TagViewHolder(itemView, Click, TagType.Local);
            return vh;
        }
    }
}
