using System.Collections.Generic;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Steepshot.Core.Models.Responses;


namespace Steepshot.Adapter
{

    public class CategoriesAdapter : RecyclerView.Adapter
    {
        List<SearchResult> Items = new List<SearchResult>();
        private Context context;

        public System.Action<int> Click;

        public CategoriesAdapter(Context context)
        {
            this.context = context;
        }

        public void Clear()
        {
            Items.Clear();
        }

        public void Reset(List<SearchResult> Items)
        {
            this.Items = Items;
        }

        public SearchResult GetItem(int position)
        {
            return Items[position];
        }

        public override int ItemCount
        {
            get
            {
                return Items.Count;
            }
        }

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
            public TextView Tag { get; private set; }
            System.Action<int> Click;

            public TagViewHolder(Android.Views.View itemView, System.Action<int> Click) : base(itemView)
            {
                this.Click = Click;
                Tag = itemView.FindViewById<TextView>(Resource.Id.tag);

                Tag.Clickable = true;
                Tag.Click += (sender, e) => Click?.Invoke(AdapterPosition);
            }

        }
    }
}