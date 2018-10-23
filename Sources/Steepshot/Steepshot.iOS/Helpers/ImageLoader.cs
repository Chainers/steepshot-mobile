using System;
using CoreGraphics;
using FFImageLoading;
using FFImageLoading.Work;
using Steepshot.Core.Models.Common;
using UIKit;

namespace Steepshot.iOS.Helpers
{
    public static class ImageLoader
    {
        public static IScheduledWork Load(string link, UIImageView view, int retry = 0, LoadingPriority priority = LoadingPriority.Normal, string placeHolder = "", string microUrl = null, CGSize size = new CGSize())
        {
            var width = (int)((size.Width == 0 ? view.Frame.Size.Width : size.Width) * UIScreen.MainScreen.Scale);

            var url = string.Format(Steepshot.Core.Constants.ProxyForAvatars, width, width, link);

            return ImageService.Instance.LoadUrl(url, TimeSpan.FromDays(5))
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
            var link = mediaModel.Url;
            if (!string.IsNullOrEmpty(mediaModel.ContentType) && mediaModel.ContentType.StartsWith("video"))
                link = mediaModel.Thumbnails.Mini;

            var widthpx = (int)(width * Constants.ScreenScale);
            var url = string.Format(Steepshot.Core.Constants.ProxyForAvatars, widthpx, widthpx, link);

            ImageService.Instance.LoadUrl(url, TimeSpan.FromDays(5))
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
