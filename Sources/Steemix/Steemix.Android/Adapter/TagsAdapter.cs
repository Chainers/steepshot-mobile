using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Android.Content;
using System.Collections.Generic;
using Java.IO;
using Sweetshot.Library.Models.Responses;

namespace Steemix.Droid.Adapter
{

	public class TagsAdapter : RecyclerView.Adapter
	{
		List<SearchResult> Items = new List<SearchResult>();
		private Context context;

		public System.Action<int> Click;

		public TagsAdapter(Context context)
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
			((TagViewHolder)holder).Tag.Text = string.Format("#{0}", Items[position].Name);
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyr_search_tag, parent, false);
			var vh = new TagViewHolder(itemView, Click);
			return vh;
		}

		public class TagViewHolder : RecyclerView.ViewHolder
		{
			public TextView Tag { get; private set; }
			System.Action<int> Click;

			public TagViewHolder(View itemView, System.Action<int> Click) : base(itemView)
			{
				this.Click = Click;
				Tag = itemView.FindViewById<TextView>(Resource.Id.tag);

				Tag.Clickable = true;
				Tag.Click += (sender, e) => Click?.Invoke(AdapterPosition);
			}

		}
	}
}