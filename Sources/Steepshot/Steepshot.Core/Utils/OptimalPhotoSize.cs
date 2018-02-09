using Steepshot.Core.Models;

namespace Steepshot.Core.Utils
{
    public static class OptimalPhotoSize
    {
        public static float Get(Size imageSize, float screenWidth, float minHeight, float maxHeight)
        {
            var correction = screenWidth;
            if (imageSize.Width != 0)
            {
                var height = screenWidth * (imageSize.Height / imageSize.Width);
                if (height >= minHeight && height <= maxHeight)
                {
                    correction = height;
                }
                else if (height >= maxHeight)
                {
                    correction = maxHeight;
                }
                else if (height <= minHeight)
                {
                    correction = minHeight;
                }
            }
            return correction;
        }
    }
}
