using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;


namespace Steepshot.Adapter
{
    public class CategoriesAdapter : RecyclerView.Adapter
    {
        public List<SearchResult> Items = new List<SearchResult>();

        public System.Action<int> Click;

        public void Clear()
        {
            Items.Clear();
        }

        public void Reset(List<SearchResult> items)
        {
            Items = items;
        }

        public SearchResult GetItem(int position)
        {
            return Items[position];
        }

        public override int ItemCount => Items.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ((TagViewHolder)holder).Tag.Text = Items[position].Name;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_category_item, parent, false);
            var vh = new TagViewHolder(itemView, Click);
            return vh;
        }

        public class TagViewHolder : RecyclerView.ViewHolder
        {
            public TextView Tag { get; }

            public TagViewHolder(View itemView, System.Action<int> click) : base(itemView)
            {
                Tag = itemView.FindViewById<TextView>(Resource.Id.tag);

                Tag.Clickable = true;
                Tag.Click += (sender, e) => click?.Invoke(AdapterPosition);
            }
        }
    }
}