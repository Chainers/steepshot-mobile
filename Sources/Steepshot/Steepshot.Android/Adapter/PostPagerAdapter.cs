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
using Steepshot.Core.Utils;
using Steepshot.CustomViews;
using Steepshot.Utils;
using Object = Java.Lang.Object;

namespace Steepshot.Adapter
{
    public sealed class PostPagerAdapter<T> : Android.Support.V4.View.PagerAdapter, ViewPager.IPageTransformer
        where T : BasePostPresenter
    {
        private const int CachedPagesCount = 5;

        private readonly float _pageOffset;
        private readonly T _presenter;
        private readonly ViewPager _pager;
        private readonly Context _context;
        private readonly List<PostViewHolder> _viewHolders;
        
        private int _itemsCount;
        private View _loadingView;
        public Action<ActionType, Post> PostAction;
        public Action<AutoLinkType, string> AutoLinkAction;
        public Action CloseAction;

        public int CurrentItem => _pager.CurrentItem;

        public PostPagerAdapter(ViewPager pager, Context context, T presenter)
        {
            _pager = pager;
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
                vh = new PostViewHolder(itemView, PostAction, AutoLinkAction, CloseAction, Style.ScreenWidth, Style.PagerScreenWidth);
                _viewHolders[reusePosition] = vh;
                container.AddView(vh.ItemView);
            }
            else
            {
                vh = _viewHolders[reusePosition];
            }

            vh.UpdateData(_presenter[position], _context);
            return vh.ItemView;
        }

        public override void NotifyDataSetChanged()
        {
            if (_presenter.Count > 0)
            {
                for (int i = CurrentItem - 2; i <= CurrentItem + 2; i++)
                {
                    if (i < 0 || i >= _presenter.Count || _presenter[i] == null) continue;
                    _viewHolders?[i % CachedPagesCount]?.UpdateData(_presenter[i], _context);
                }

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

                var itemView = _viewHolders[i % CachedPagesCount]?.ItemView;
                if (itemView != null)
                    TransformPage(_viewHolders[i % CachedPagesCount].ItemView, pos + _pageOffset / itemView.Width);
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
            var postFooter = page.FindViewById<LinearLayout>(Resource.Id.footer);
            if (position == 1 + positionOffset * 1.5)
            {
                postHeader.TranslationX = _pageOffset;
                postFooter.TranslationX = _pageOffset;
            }
            else
            {
                var translation = (int)((position - positionOffset) * _pageOffset);
                postHeader.TranslationX = translation;
                postFooter.TranslationX = translation;
            }
        }
    }

    public sealed class PostViewHolder : FeedViewHolder
    {
        private readonly Action _closeAction;
        public PostViewHolder(View itemView, Action<ActionType, Post> postAction, Action<AutoLinkType, string> autoLinkAction, Action closeAction, int height, int width)
            : base(itemView, postAction, autoLinkAction, height, width)
        {
            PhotoPagerType = PostPagerType.PostScreen;
            _closeAction = closeAction;
            var closeButton = itemView.FindViewById<ImageButton>(Resource.Id.close);
            closeButton.Click += CloseButtonOnClick;

            var postHeader = itemView.FindViewById<RelativeLayout>(Resource.Id.title);
            var postFooter = itemView.FindViewById<LinearLayout>(Resource.Id.footer);
            postHeader.SetLayerType(LayerType.Hardware, null);
            postFooter.SetLayerType(LayerType.Hardware, null);

            ((MediaPager)PhotosViewPager).Radius = (int)BitmapUtils.DpToPixel(10, Context.Resources);

            NsfwMask.ViewTreeObserver.GlobalLayout += ViewTreeObserverOnGlobalLayout;
        }

        protected override void SetNsfwMaskLayout()
        {
            base.SetNsfwMaskLayout();
            ((RelativeLayout.LayoutParams)NsfwMask.LayoutParameters).AddRule(LayoutRules.AlignParentTop);
            ((RelativeLayout.LayoutParams)NsfwMask.LayoutParameters).AddRule(LayoutRules.Above, Resource.Id.footer);
        }

        private void ViewTreeObserverOnGlobalLayout(object sender, EventArgs eventArgs)
        {
            if (NsfwMask.Height < BitmapUtils.DpToPixel(200, Context.Resources))
                NsfwMaskSubMessage.Visibility = ViewStates.Gone;
        }

        protected override void OnTitleOnClick(object sender, EventArgs e)
        {
            base.OnTitleOnClick(sender, e);
            UpdateData(Post, Context);
        }

        private void CloseButtonOnClick(object sender, EventArgs eventArgs)
        {
            _closeAction?.Invoke();
        }
    }
}