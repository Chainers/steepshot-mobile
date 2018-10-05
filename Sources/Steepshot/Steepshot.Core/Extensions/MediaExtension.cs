using Steepshot.Core.Models.Common;
using System;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Extensions
{
    public static class MediaExtension
    {
        public static int OptimalPhotoSize(this FrameSize size, float screenWidth, float minHeight, float maxHeight)
        {
            float correction = screenWidth;
            if (size != null && size.Width != 0)
            {
                var height = screenWidth * size.Height / size.Width;
                if (height >= minHeight && height <= maxHeight)
                {
                    correction = height;
                }
                else if (height > maxHeight)
                {
                    correction = maxHeight;
                }
                else if (height < minHeight)
                {
                    correction = minHeight;
                }
            }
            return (int)correction;
        }

        public static int OptimalPhotoSize(this MediaModel media, float screenWidth, float minHeight, float maxHeight)
        {
            return OptimalPhotoSize(media.Size, screenWidth, minHeight, maxHeight);
        }
    }
}
