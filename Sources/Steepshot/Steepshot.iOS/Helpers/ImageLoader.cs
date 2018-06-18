using System;
using CoreGraphics;
using FFImageLoading;
using FFImageLoading.Work;
using Steepshot.Core.Extensions;
using UIKit;

namespace Steepshot.iOS.Helpers
{
    public static class ImageLoader
    {
        public static IScheduledWork Load(string url, UIImageView view, int retry = 0, LoadingPriority priority = LoadingPriority.Normal, string placeHolder = "", string microUrl = null, CGSize size = new CGSize())
        {
            var width = (int)((size.Width == 0 ? view.Frame.Size.Width : size.Width) * UIScreen.MainScreen.Scale);

            return ImageService.Instance.LoadUrl(url.GetProxy(width, width), TimeSpan.FromDays(5))
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

        public static void Preload(string url, CGSize gSize, string microUrl = null)
        {
            ImageService.Instance.LoadUrl(url.GetProxy((int)(gSize.Width * UIScreen.MainScreen.Scale), (int)(gSize.Width * UIScreen.MainScreen.Scale)), TimeSpan.FromDays(5))
                                 .WithCache(FFImageLoading.Cache.CacheType.All)
                                 .WithPriority(LoadingPriority.Low)
                                 .Error((error) =>
                                 {
                                     ImageService.Instance.LoadUrl(microUrl != null ? microUrl : url, TimeSpan.FromDays(5))
                                                 .WithCache(FFImageLoading.Cache.CacheType.All)
                                                 .DownSample((int)gSize.Width)
                                                 .WithPriority(LoadingPriority.Lowest)
                                                 .Preload();
                                 })
                                 .Preload();
        }
    }
}
