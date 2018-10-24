using System;
using Android.Content;
using Android.Views;
using Steepshot.Core.Models.Common;
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

        private readonly MediaView[] _mediaViews;

        public PostPhotosPagerAdapter(Context context, Action<Post> photoAction)
        {
            _context = context;
            _photoAction = photoAction;
            _mediaViews = new MediaView[CachedPagesCount];
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
                mediaView.Click += MediaClick;
                _mediaViews[reusePosition] = mediaView;
            }

            container.AddView(mediaView);
            LoadMedia(_post.Media[position], mediaView);
            return mediaView;
        }

        private void MediaClick(object sender, EventArgs e)
        {
            _photoAction?.Invoke(_post);
        }

        public void Playback(bool shouldPlay)
        {
            var mediaView = _mediaViews[0];
            if (mediaView == null)
                return;

            if (shouldPlay)
                mediaView.Play();
            else
                mediaView.Pause();
        }

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
