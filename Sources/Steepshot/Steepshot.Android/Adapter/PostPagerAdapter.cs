using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.Utils;
using Object = Java.Lang.Object;

namespace Steepshot.Adapter
{
    public class PostPagerAdapter<T> : Android.Support.V4.View.PagerAdapter, ViewPager.IPageTransformer
        where T : BasePostPresenter
    {
        private const int CachedPagesCount = 5;
        private readonly float _pageOffset;
        private readonly T _presenter;
        private readonly Context _context;
        private readonly List<PostViewHolder> _viewHolders;
        private int _itemsCount;
        private View _loadingView;
        public Action<Post> LikeAction, UserAction, CommentAction, PhotoClick, FlagAction, HideAction, EditAction, DeleteAction;
        public Action<Post, VotersType> VotersClick;
        public Action<string> TagAction;
        public Action CloseAction;
        public int CurrentItem { get; set; }

        public PostPagerAdapter(Context context, T presenter)
        {
            _context = context;
            _presenter = presenter;
            _viewHolders = new List<PostViewHolder>(_presenter.Count);
            _viewHolders.AddRange(Enumerable.Repeat<PostViewHolder>(null, CachedPagesCount));
            _itemsCount = 0;
            _pageOffset = BitmapUtils.DpToPixel(20, _context.Resources);
        }

        public override Object InstantiateItem(ViewGroup container, int position)
        {
            if (position == _presenter.Count)
            {
                if (_loadingView == null)
                {
                    _loadingView = LayoutInflater.From(_context)
                        .Inflate(Resource.Layout.lyt_postcard_loading, container, false);
                }

                container.AddView(_loadingView);
                return _loadingView;
            }

            var reusePosition = position % CachedPagesCount;
            PostViewHolder vh;
            if (_viewHolders[reusePosition] == null)
            {
                var itemView = LayoutInflater.From(_context)
                    .Inflate(Resource.Layout.lyt_post_view_item, container, false);
                vh = new PostViewHolder(itemView, LikeAction, UserAction, CommentAction, PhotoClick, VotersClick,
                    FlagAction, HideAction, EditAction, DeleteAction, TagAction, CloseAction, _context.Resources.DisplayMetrics.WidthPixels);
                _viewHolders[reusePosition] = vh;
                container.AddView(vh.ItemView);
            }
            else
                vh = _viewHolders[reusePosition];

            vh.UpdateData(_presenter[position], _context);
            return vh.ItemView;
        }

        public override void NotifyDataSetChanged()
        {
            if (_presenter.Count > 0)
            {
                var reusePosition = CurrentItem % CachedPagesCount;
                if (_presenter[CurrentItem] != null)
                    _viewHolders[reusePosition]?.UpdateData(_presenter[CurrentItem], _context);
                _itemsCount = _presenter.IsLastReaded ? _presenter.Count : _presenter.Count + 1;
                base.NotifyDataSetChanged();
                ResetVisibleItems();
            }
        }

        private void ResetVisibleItems()
        {
            var pos = -_pageOffset;
            for (int i = CurrentItem - 1; i <= CurrentItem + 1; i++)
            {
                if (i < 0 || i == _presenter.Count)
                {
                    pos += _pageOffset;
                    continue;
                }
                TransformPage(_viewHolders[i % CachedPagesCount]?.ItemView, pos);
                pos += _pageOffset;
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
            }
        }

        public override int Count => _itemsCount;

        public void TransformPage(View page, float position)
        {
            if (page == _loadingView || page == null) return;
            var pageWidth = page.Width;
            var positionOffset = _pageOffset / pageWidth;

            var postHeader = page.FindViewById<RelativeLayout>(Resource.Id.title);
            var postFooter = page.FindViewById<RelativeLayout>(Resource.Id.subtitle);
            if (position == 1 + positionOffset * 1.5)
            {
                postHeader.TranslationX = _pageOffset;
                postFooter.TranslationX = _pageOffset;
            }
            else
            {
                var translation = (int)(position * _pageOffset);
                postHeader.TranslationX = translation;
                postFooter.TranslationX = translation;
            }
        }
    }

    public class PostViewHolder : FeedViewHolder
    {
        private readonly Action _closeAction;
        private readonly ImageButton _closeButton;
        private readonly RelativeLayout _postHeader;
        private readonly RelativeLayout _postFooter;
        public PostViewHolder(View itemView, Action<Post> likeAction, Action<Post> userAction, Action<Post> commentAction, Action<Post> photoAction, Action<Post, VotersType> votersAction, Action<Post> flagAction, Action<Post> hideAction, Action<Post> editAction, Action<Post> deleteAction, Action<string> tagAction, Action closeAction, int height) : base(itemView, likeAction, userAction, commentAction, photoAction, votersAction, flagAction, hideAction, editAction, deleteAction, tagAction, height)
        {
            PhotoPagerType = PostPagerType.PostScreen;
            _closeAction = closeAction;
            var closeButton = itemView.FindViewById<ImageButton>(Resource.Id.close);
            closeButton.Click += CloseButtonOnClick;

            var postHeader = itemView.FindViewById<RelativeLayout>(Resource.Id.title);
            var postFooter = itemView.FindViewById<RelativeLayout>(Resource.Id.subtitle);
            postHeader.SetLayerType(LayerType.Hardware, null);
            postFooter.SetLayerType(LayerType.Hardware, null);

            _nsfwMask.ViewTreeObserver.GlobalLayout += ViewTreeObserverOnGlobalLayout;
        }

        private void ViewTreeObserverOnGlobalLayout(object sender, EventArgs eventArgs)
        {
            if (_nsfwMask.Height < BitmapUtils.DpToPixel(200, _context.Resources))
                _nsfwMaskSubMessage.Visibility = ViewStates.Gone;
        }

        protected override void OnTitleOnClick(object sender, EventArgs e)
        {
            base.OnTitleOnClick(sender, e);
            UpdateData(_post, _context);
        }

        private void CloseButtonOnClick(object sender, EventArgs eventArgs)
        {
            _closeAction?.Invoke();
        }
    }
}