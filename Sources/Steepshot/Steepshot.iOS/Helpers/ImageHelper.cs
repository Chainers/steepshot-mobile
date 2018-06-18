using System;
using System.Drawing;
using CoreGraphics;
using UIKit;

namespace Steepshot.iOS.Helpers
{
    public static class ImageHelper
    {
        public static UIImage RotateImage(UIImage image, UIImageOrientation orientation)
        {
            var rotated = new UIImage(image.CGImage, image.CurrentScale, orientation);
            UIGraphics.BeginImageContextWithOptions(rotated.Size, false, rotated.CurrentScale);
            var drawRect = new CGRect(0, 0, rotated.Size.Width, rotated.Size.Height);
            rotated.Draw(drawRect);
            image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return image;
        }

        public static CGSize CalculateInSampleSize(CGSize imageSize, float reqWidth, float reqHeight, bool increase = false)
        {
            var height = (float)imageSize.Height;
            var width = (float)imageSize.Width;
            var inSampleSize = 1f;
            if (increase)
            {
                if (height < reqHeight)
                {
                    inSampleSize = reqHeight / height;
                }
                if (width < reqWidth)
                {
                    inSampleSize = Math.Min(inSampleSize, reqWidth / width);
                }
            }
            else
            {
                if (height > reqHeight)
                {
                    inSampleSize = reqHeight / height;
                }
                if (width > reqWidth)
                {
                    inSampleSize = Math.Min(inSampleSize, reqWidth / width);
                }
            }
            return new CGSize(width * inSampleSize, height * inSampleSize);
        }

        public static UIImage CropImage(UIImage sourceImage, float cropX, float cropY, float width, float height, CGSize inSampleSize)
        {
            var imgSize = sourceImage.Size;
            UIGraphics.BeginImageContext(new SizeF(width, height));
            var context = UIGraphics.GetCurrentContext();
            var clippedRect = new RectangleF(0, 0, width, height);
            context.ClipToRect(clippedRect);

            var drawRect = new CGRect(-cropX, -cropY, inSampleSize.Width, inSampleSize.Height);
            sourceImage.Draw(drawRect);
            var modifiedImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            context.Dispose();
            return modifiedImage;
        }
    }
}
