using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Steepshot.Core.Presenters;
using Steepshot.Utils;

namespace Steepshot.Adapter
{
    public class TagsAdapter : RecyclerView.Adapter
    {
        private readonly TagsPresenter _presenter;
        public Action<string> Click;

        public override int ItemCount => _presenter.Count;


        public TagsAdapter(TagsPresenter presenter)
        {
            _presenter = presenter;
        }


        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var result = _presenter[position];
            if (result == null)
                return;

            ((TagViewHolder)holder).UpdateData(result.Name);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_search_tag, parent, false);
            var vh = new TagViewHolder(itemView, Click);
            return vh;
        }
    }

    public class TagViewHolder : RecyclerView.ViewHolder
    {
        private readonly TextView _tag;
        private readonly ViewGroup _tagLayout;
        private readonly Action<string> _click;
        private string _text;

        public TagViewHolder(View itemView, Action<string> click) : base(itemView)
        {
            _click = click;
            _tag = itemView.FindViewById<TextView>(Resource.Id.tag);
            _tagLayout = itemView.FindViewById<ViewGroup>(Resource.Id.tag_layout);

            _tagLayout.Click += OnTagLayoutOnClick;
            _tag.Typeface = Style.Semibold;
        }


        public void UpdateData(string text)
        {
            _text = text;
            _tag.Text = text;
        }

        private void OnTagLayoutOnClick(object sender, EventArgs e)
        {
            _click?.Invoke(_text);
        }
    }
}
