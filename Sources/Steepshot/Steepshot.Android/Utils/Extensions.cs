using Android.Support.V7.Widget;
using Square.Picasso;
using Steepshot.Core;
using Steepshot.Core.Models.Common;

namespace Steepshot.Utils
{
    public static class Extensions
    {
        public static string ToFilePath(this string val)
        {
            if (!val.StartsWith("http") && !val.StartsWith("file://") && !val.StartsWith("content://"))
                val = "file://" + val;
            return val;
        }

        public static void MoveToPosition(this RecyclerView recyclerView, int position)
        {
            if (position < 0)
                position = 0;
            recyclerView.SmoothScrollToPosition(position);
        }

        public static RequestCreator LoadWithProxy(this Picasso picasso, string link, int width, int height)
        {
            var url = string.Format(Constants.ProxyForAvatars, width, height, link);
            return picasso.Load(url);
        }

        public static RequestCreator LoadWithProxy(this Picasso picasso, Post post, int width)
        {
            return LoadWithProxy(picasso, post.Media[0], width);
        }

        public static RequestCreator LoadWithProxy(this Picasso picasso, MediaModel mediaModel, int width)
        {
            var url = mediaModel.Url;
            if (!string.IsNullOrEmpty(mediaModel.ContentType) && mediaModel.ContentType.StartsWith("video"))
                url = mediaModel.Thumbnails.Mini;

            return LoadWithProxy(picasso, url, width, width);
        }
    }
}
