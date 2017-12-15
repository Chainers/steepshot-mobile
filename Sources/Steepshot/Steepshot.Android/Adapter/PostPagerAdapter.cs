using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Views;
using Android.Widget;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Object = Java.Lang.Object;

namespace Steepshot.Adapter
{
    public class PostPagerAdapter<T> : Android.Support.V4.View.PagerAdapter where T : BasePostPresenter
    {
        private const int CachedPagesCount = 5;
        private readonly T Presenter;
        private readonly Context Context;
        private readonly List<PostViewHolder> _viewHolders;
        private int _itemsCount;
        private View _loadingView;
        public Action<Post> LikeAction, UserAction, CommentAction, PhotoClick, FlagAction, HideAction;
        public Action<Post, VotersType> VotersClick;
        public Action<string> TagAction;
        public Action CloseAction;
        public int CurrentItem { get; set; }

        public PostPagerAdapter(Context context, T presenter)
        {
            Context = context;
            Presenter = presenter;
            _viewHolders = new List<PostViewHolder>(Presenter.Count);
            _viewHolders.AddRange(Enumerable.Repeat<PostViewHolder>(null, CachedPagesCount));
            _itemsCount = 0;
        }

        public override Object InstantiateItem(ViewGroup container, int position)
        {
            if (position == Presenter.Count)
            {
                if (_loadingView == null)
                {
                    _loadingView = LayoutInflater.From(Context)
                        .Inflate(Resource.Layout.lyt_postcard_loading, container, false);
                }
                container.AddView(_loadingView);
                return _loadingView;
            }
            var reusePosition = position % CachedPagesCount;
            PostViewHolder vh;
            if (_viewHolders[reusePosition] == null)
            {
                var itemView = LayoutInflater.From(Context)
                    .Inflate(Resource.Layout.lyt_post_view_item, container, false);
                vh = new PostViewHolder(itemView, LikeAction, UserAction, CommentAction, PhotoClick, VotersClick,
                    FlagAction, HideAction, TagAction, CloseAction, Context.Resources.DisplayMetrics.WidthPixels);
                _viewHolders[reusePosition] = vh;
                container.AddView(vh.ItemView);
            }
            else
                vh = _viewHolders[reusePosition];
            vh.UpdateData(Presenter[position], Context);
            container.Invalidate();
            return vh.ItemView;
        }

        public override void NotifyDataSetChanged()
        {
            if (Presenter.Count > 0)
            {
                if (Presenter[CurrentItem] != null)
                    _viewHolders[CurrentItem % CachedPagesCount]?.UpdateData(Presenter[CurrentItem], Context);
                _itemsCount = Presenter.IsLastReaded ? Presenter.Count : Presenter.Count + 1;
                base.NotifyDataSetChanged();
            }
        }

        public override bool IsViewFromObject(View view, Object @object)
        {
            return @object == view;
        }

        public override int GetItemPosition(Object @object)
        {
            if (@object != _loadingView)
                return PositionUnchanged;
            return PositionNone;
        }

        public override void DestroyItem(ViewGroup container, int position, Object @object)
        {
            if (@object == _loadingView)
            {
                container.RemoveView(_loadingView);
                container.Invalidate();
            }
        }

        public override int Count => _itemsCount;

        public void HideHeaderButtons()
        {
            _viewHolders.ForEach(vh => vh?.HideHeaderButtons());
        }

        public void ShowHeaderButtons(int position)
        {
            var reusePosition = position % CachedPagesCount;
            _viewHolders[reusePosition]?.ShowHeaderButtons();
        }
    }

    public class PostViewHolder : FeedViewHolder
    {
        private readonly Action _closeAction;
        private readonly ImageButton _closeButton;
        public PostViewHolder(View itemView, Action<Post> likeAction, Action<Post> userAction, Action<Post> commentAction, Action<Post> photoAction, Action<Post, VotersType> votersAction, Action<Post> flagAction, Action<Post> hideAction, Action<string> tagAction, Action closeAction, int height) : base(itemView, likeAction, userAction, commentAction, photoAction, votersAction, flagAction, hideAction, tagAction, height)
        {
            PhotoPagerType = PostPagerType.PostScreen;

            _closeAction = closeAction;
            _closeButton = itemView.FindViewById<ImageButton>(Resource.Id.close);
            _closeButton.Click += CloseButtonOnClick;
        }

        protected override void SetNsfwMaskLayout()
        {
            ((RelativeLayout.LayoutParams)_nsfwMask.LayoutParameters).AddRule(LayoutRules.AlignParentTop);
            ((RelativeLayout.LayoutParams)_nsfwMask.LayoutParameters).AddRule(LayoutRules.Above, Resource.Id.subtitle);
        }

        private void CloseButtonOnClick(object sender, EventArgs eventArgs)
        {
            _closeAction?.Invoke();
        }

        public void HideHeaderButtons()
        {
            _closeButton.Alpha = 0;
            _more.Alpha = 0;
        }

        public void ShowHeaderButtons()
        {
            _closeButton.Alpha = 100;
            _more.Alpha = 100;
        }
    }
}