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
        List<SearchResult> _items = new List<SearchResult>();
        private Context _context;

        public System.Action<int> Click;

        public CategoriesAdapter(Context context)
        {
            this._context = context;
        }

        public void Clear()
        {
            _items.Clear();
        }

        public void Reset(List<SearchResult> items)
        {
            this._items = items;
        }

        public SearchResult GetItem(int position)
        {
            return _items[position];
        }

        public override int ItemCount
        {
            get
            {
                return _items.Count;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
			((TagViewHolder)holder).Tag.Text = _items[position].Name;
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
            System.Action<int> _click;

            public TagViewHolder(Android.Views.View itemView, System.Action<int> click) : base(itemView)
            {
                this._click = click;
                Tag = itemView.FindViewById<TextView>(Resource.Id.tag);

                Tag.Clickable = true;
                Tag.Click += (sender, e) => click?.Invoke(AdapterPosition);
            }

        }
    }
}