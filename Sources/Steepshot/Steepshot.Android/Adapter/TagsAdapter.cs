using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Steepshot.Core.Models.Responses;


namespace Steepshot.Adapter
{

	public class TagsAdapter : RecyclerView.Adapter
	{
		List<SearchResult> _items = new List<SearchResult>();

		public System.Action<int> Click;
        
		public void Clear()
		{
			_items.Clear();
		}

		public void Reset(List<SearchResult> items)
		{
			_items = items;
		}

		public SearchResult GetItem(int position)
		{
            if (position > 0)
                return _items[position];
            else
                return _items[0];
		}

		public override int ItemCount => _items.Count;

	    public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			((TagViewHolder)holder).Tag.Text = _items[position].Name;
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyr_search_tag, parent, false);
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