using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Extensions
{
    public static class MediaExtension
    {
        public static int OptimalPhotoSize(this MediaModel media, float screenWidth, float minHeight, float maxHeight)
        {
            float correction = screenWidth;
            if (media.Size != null && media.Size.Width != 0)
            {
                var height = screenWidth * media.Size.Height / media.Size.Width;
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
    }
}
