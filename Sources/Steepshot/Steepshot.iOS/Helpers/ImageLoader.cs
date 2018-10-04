using System;
using CoreGraphics;
using FFImageLoading;
using FFImageLoading.Work;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Common;
using UIKit;

namespace Steepshot.iOS.Helpers
{
    public static class ImageLoader
    {
        public static IScheduledWork Load(string url, UIImageView view, int retry = 0, LoadingPriority priority = LoadingPriority.Normal, string placeHolder = "", string microUrl = null, CGSize size = new CGSize())
        {
            var width = (int)((size.Width == 0 ? view.Frame.Size.Width : size.Width) * UIScreen.MainScreen.Scale);

            return ImageService.Instance.LoadUrl(url.GetImageProxy(width, width), TimeSpan.FromDays(5))
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

        public static void Preload(MediaModel mediaModel, nfloat width)
        {
            ImageService.Instance.LoadUrl(mediaModel.GetImageProxy((int)(width * Constants.ScreenScale)), TimeSpan.FromDays(5))
                                 .WithCache(FFImageLoading.Cache.CacheType.All)
                                 .WithPriority(LoadingPriority.Low)
                                 .Error((error) =>
                                 {
                                     ImageService.Instance.LoadUrl(mediaModel.Thumbnails.Mini, TimeSpan.FromDays(5))
                                                 .WithCache(FFImageLoading.Cache.CacheType.All)
                                                 .DownSample((int)width)
                                                 .WithPriority(LoadingPriority.Lowest)
                                                 .Preload();
                                 })
                                 .Preload();
        }
    }
}
