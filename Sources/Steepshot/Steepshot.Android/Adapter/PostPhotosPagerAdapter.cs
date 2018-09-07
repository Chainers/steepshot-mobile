using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Provider;
using Android.Views;
using Steepshot.Core.Models.Common;
using Steepshot.Utils.Media;
using Object = Java.Lang.Object;

namespace Steepshot.Adapter
{
    public class PostPhotosPagerAdapter : Android.Support.V4.View.PagerAdapter
    {
        private const int CachedPagesCount = 5;
        private readonly List<MediaView> _mediaViews;
        private readonly Context _context;
        private readonly Action<Post> _photoAction;
        private Post _post;

        public PostPhotosPagerAdapter(Context context, Action<Post> photoAction)
        {
            _context = context;
            _photoAction = photoAction;
            _mediaViews = new List<MediaView>(Enumerable.Repeat<MediaView>(null, CachedPagesCount));
        }

        public void UpdateData(Post post)
        {
            _post = post;
            NotifyDataSetChanged();
        }

        private void LoadMedia(MediaModel mediaModel, MediaView mediaView)
        {
            if (mediaModel != null)
            {
                var parent = (View)mediaView.Parent;
                mediaView.LayoutParameters.Height = parent.LayoutParameters.Height;
                mediaView.LayoutParameters.Width = parent.LayoutParameters.Width;
                mediaView.MediaSource = mediaModel;
            }
        }

        public override Object InstantiateItem(ViewGroup container, int position)
        {
            var reusePosition = position % CachedPagesCount;
            var mediaView = _mediaViews[reusePosition];

            if (mediaView == null)
            {
                mediaView = new MediaView(_context) { LayoutParameters = container.LayoutParameters };
                mediaView.OnClick += MediaClick;
                _mediaViews[reusePosition] = mediaView;
            }

            container.AddView(mediaView);
            LoadMedia(_post.Media[position], mediaView);
            return mediaView;
        }

        private void MediaClick(MediaType mediaType)
        {
            switch (mediaType)
            {
                case MediaType.Image:
                    _photoAction?.Invoke(_post);
                    break;
            }
        }

        public override int GetItemPosition(Object @object) => PositionNone;

        public override void DestroyItem(ViewGroup container, int position, Object obj)
        {
            container.RemoveView((View)obj);
        }

        public override bool IsViewFromObject(View view, Object @object)
        {
            return view == @object;
        }

        public override int Count => _post?.Media.Length ?? 0;
    }
}