using System;
using FFImageLoading;
using FFImageLoading.Work;
using Steepshot.Core.Extensions;
using UIKit;

namespace Steepshot.iOS.Helpers
{
    public static class ImageLoader
    {
        public static IScheduledWork Load(string url, UIImageView view, int retry = 0, LoadingPriority priority = LoadingPriority.Normal, string placeHolder = "", string microUrl = null)
        {
            return ImageService.Instance.LoadUrl(url.GetProxy((int)(view.Frame.Size.Width * UIScreen.MainScreen.Scale), (int)(view.Frame.Width * UIScreen.MainScreen.Scale)), TimeSpan.FromDays(5))
                         .Retry(retry)
                         .FadeAnimation(false)
                         .LoadingPlaceholder(placeHolder)
                         .ErrorPlaceholder(placeHolder)
                         .WithCache(FFImageLoading.Cache.CacheType.All)
                         .WithPriority(priority)
                         .Error((error) =>
                         {
                             ImageService.Instance.LoadUrl(microUrl != null ? microUrl : url, TimeSpan.FromDays(5))
                                        .Retry(retry)
                                        .FadeAnimation(false)
                                        .WithCache(FFImageLoading.Cache.CacheType.All)
                                        .DownSample((int)view.Frame.Size.Width)
                                        .WithPriority(priority)
                                        .Into(view);
                         })
                         .Into(view);
        }
    }
}
