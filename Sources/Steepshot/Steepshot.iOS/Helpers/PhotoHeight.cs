using System;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Utils;
using UIKit;

namespace Steepshot.iOS.Helpers
{
    public static class PhotoHeight
    {
        private const int minHeight = 200;
        private const int maxHeight = 450;

        public static nfloat Get(FrameSize frameSize)
        {
            var correction = UIScreen.MainScreen.Bounds.Width;
            if (frameSize.Width != 0)
            {
                var height = (float)Math.Ceiling(UIScreen.MainScreen.Bounds.Width * ((float)frameSize.Height / (float)frameSize.Width));

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
