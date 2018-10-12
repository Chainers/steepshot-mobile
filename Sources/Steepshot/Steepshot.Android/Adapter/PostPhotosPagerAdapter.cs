#define MediaViewMode

using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Provider;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Utils;
using Steepshot.Utils;
using Steepshot.Utils.Media;
using Object = Java.Lang.Object;

namespace Steepshot.Adapter
{
    public class PostPhotosPagerAdapter : Android.Support.V4.View.PagerAdapter
    {
        private const int CachedPagesCount = 5;
        private readonly Context _context;
        private readonly Action<Post> _photoAction;
        private Post _post;

#if MediaViewMode
            private readonly List<MediaView> _mediaViews;

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

        public void Playback(bool shouldPlay)
        {
            var mediaView = _mediaViews[0];
            if (shouldPlay)
                mediaView.Play();
            else
                mediaView.Pause();
        }
#else

        private readonly List<ImageView> _mediaViews;

        public PostPhotosPagerAdapter(Context context, Action<Post> photoAction)
        {
            _context = context;
            _photoAction = photoAction;
            _mediaViews = new List<ImageView>(Enumerable.Repeat<ImageView>(null, CachedPagesCount));
        }

        public void UpdateData(Post post)
        {
            _post = post;
            NotifyDataSetChanged();
        }

        private void LoadMedia(MediaModel mediaModel, ImageView mediaView)
        {
            if (mediaModel != null)
            {
                var parent = (View)mediaView.Parent;
                mediaView.LayoutParameters.Height = parent.LayoutParameters.Height;
                mediaView.LayoutParameters.Width = parent.LayoutParameters.Width;

                Picasso.With(_context).Load(mediaModel.GetImageProxy(Style.ScreenWidth))
                    .Placeholder(new ColorDrawable(Style.R245G245B245))
                    .NoFade()
                    .Priority(Picasso.Priority.High)
                    .Into(mediaView, null, () =>
                    {
                        Picasso.With(_context).Load(mediaModel.Url).Placeholder(new ColorDrawable(Style.R245G245B245)).NoFade().Priority(Picasso.Priority.High).Into(mediaView);
                    });
            }
        }

        public override Object InstantiateItem(ViewGroup container, int position)
        {
            var reusePosition = position % CachedPagesCount;
            var mediaView = _mediaViews[reusePosition];

            if (mediaView == null)
            {
                mediaView = new ImageView(_context) { LayoutParameters = container.LayoutParameters };
                mediaView.SetImageDrawable(null);
                mediaView.SetScaleType(ImageView.ScaleType.CenterCrop);
                mediaView.Click += MediaClick;
                _mediaViews[reusePosition] = mediaView;
            }

            container.AddView(mediaView);
            LoadMedia(_post.Media[position], mediaView);
            return mediaView;
        }

        private void MediaClick(object sender, EventArgs eventArgs)
        {
            _photoAction?.Invoke(_post);
        }
#endif

        public override int GetItemPosition(Object @object) => PositionNone;

        public override void DestroyItem(ViewGroup container, int position, Object obj)
        {
            container.RemoveView((View)obj);
        }

        public override bool IsViewFromObject(View view, Object obj)
        {
            return view == obj;
        }

        public override int Count => _post?.Media.Length ?? 0;
    }
}
