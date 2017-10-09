using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Steepshot.Core.Presenters;

namespace Steepshot.Adapter
{

    public class TagsAdapter : RecyclerView.Adapter
    {
        public System.Action<int> Click;
        private TagsPresenter _presenter;

        public TagsAdapter(TagsPresenter presenter)
        {
            _presenter = presenter;
        }

        public override int ItemCount => _presenter.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ((TagViewHolder)holder).Tag.Text = _presenter[position]?.Name;
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