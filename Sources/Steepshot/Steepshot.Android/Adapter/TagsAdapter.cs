using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Steepshot.Core.Facades;
using Steepshot.Core.Presenters;
using Steepshot.Utils;

namespace Steepshot.Adapter
{
    public sealed class TagsAdapter : RecyclerView.Adapter
    {
        private readonly TagsPresenter _presenter;
        private readonly TagPickerFacade _tagPickerFacade;
        public Action<string> Click;

        public override int ItemCount => _presenter?.Count ?? _tagPickerFacade.Count;


        public TagsAdapter(TagsPresenter presenter)
        {
            _presenter = presenter;
        }

        public TagsAdapter(TagPickerFacade facade)
        {
            _tagPickerFacade = facade;
        }

        public int IndexOfTag(string tag)
        {
            return _presenter?.FindIndex(t => t.Name == tag) ?? _tagPickerFacade.IndexOf(tag);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var result = _presenter?[position].Name ?? _tagPickerFacade?[position];
            if (result == null)
                return;

            ((TagViewHolder)holder).UpdateData(result);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_search_tag, parent, false);
            var vh = new TagViewHolder(itemView, Click, _presenter == null ? TagType.PostSearch : TagType.BrowseSearch);
            return vh;
        }
    }

    public sealed class TagViewHolder : RecyclerView.ViewHolder
    {
        private readonly TextView _tag;
        private readonly ViewGroup _tagLayout;
        private readonly ImageView _buttonState;
        private readonly Action<string> _click;
        private readonly TagType _type;
        private string _text;

        public TagViewHolder(View itemView, Action<string> click, TagType type) : base(itemView)
        {
            _click = click;
            _tag = itemView.FindViewById<TextView>(Resource.Id.tag);
            _tagLayout = itemView.FindViewById<ViewGroup>(Resource.Id.tag_layout);
            _buttonState = itemView.FindViewById<ImageView>(Resource.Id.button_state);

            _type = type;
            if (_type == TagType.BrowseSearch)
                _buttonState.Visibility = ViewStates.Gone;

            _tagLayout.Touch += OnTagLayoutOnClick;
            _tag.Typeface = Style.Semibold;
        }


        public void UpdateData(string text)
        {
            _text = text;
            _tag.Text = text;
        }

        private void OnTagLayoutOnClick(object sender, View.TouchEventArgs e)
        {
            switch (e.Event.Action)
            {
                case MotionEventActions.Down:
                case MotionEventActions.Move:
                    _buttonState.SetImageResource(_type == TagType.Local ? Resource.Drawable.ic_close_tag_active : Resource.Drawable.ic_add_tag_active);
                    break;
                case MotionEventActions.Up:
                    _buttonState.SetImageResource(_type == TagType.Local ? Resource.Drawable.ic_close_tag : Resource.Drawable.ic_add_tag);
                    _click?.Invoke(_text);
                    break;
                default:
                    _buttonState.SetImageResource(_type == TagType.Local ? Resource.Drawable.ic_close_tag : Resource.Drawable.ic_add_tag);
                    break;
            }
        }
    }
}
