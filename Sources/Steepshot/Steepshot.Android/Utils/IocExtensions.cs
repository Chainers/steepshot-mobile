using Android.Content;
using Autofac;
using Autofac.Core;
using Steepshot.Utils.Media;

namespace Steepshot.Utils
{
    public static class IocExtensions
    {
        public static VideoPlayerManager GetVideoPlayerManager(this IContainer container, Context context, long cacheSize)
        {
            var args = new Parameter[]
            {
                new TypedParameter(typeof(Context),context),
                new TypedParameter(typeof(long),cacheSize)
            };

            return container.Resolve<VideoPlayerManager>(args);
        }
    }
}