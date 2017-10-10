using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Steepshot.Core.Presenters;


namespace Steepshot.Adapter
{
    public class CategoriesAdapter : RecyclerView.Adapter
    {
        private readonly TagsPresenter _tagsPresenter;
        public System.Action<int> Click;

        public CategoriesAdapter(TagsPresenter tagsPresenter)
        {
            _tagsPresenter = tagsPresenter;
        }

        public override int ItemCount => _tagsPresenter.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var tag = _tagsPresenter[position];
            if (tag == null)
                return;
            ((TagViewHolder)holder).Tag.Text = tag.Name;
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