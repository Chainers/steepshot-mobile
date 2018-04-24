using System;
using CoreGraphics;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.CustomViews
{
    public class CropView : UIScrollView
    {
        public CGSize originalImageSize;
        public UIImageView imageView;
        public UIImageOrientation orientation = UIImageOrientation.Up;

        public CropView(CGRect _frame)
        {
            imageView = new UIImageView(_frame);
            imageView.ContentMode = UIViewContentMode.ScaleAspectFit;

            BackgroundColor = Constants.R245G245B245;
            Frame = _frame;
            Bounces = false;
            MinimumZoomScale = 1f;
            MaximumZoomScale = 4f;
            ViewForZoomingInScrollView += (UIScrollView sv) => { return imageView; };
            AddSubview(imageView);
            ContentSize = new CGSize(UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Width);
            DidZoom += (t, u) =>
            {
                SetScrollViewInsets();
            };
        }

        public void AdjustImageViewSize(UIImage img)
        {
            var w = img.Size.Width / Core.Constants.PhotoMaxSize * UIScreen.MainScreen.Bounds.Width;
            var h = img.Size.Height / Core.Constants.PhotoMaxSize * UIScreen.MainScreen.Bounds.Width;
            originalImageSize = new CGSize(w, h);

            if (originalImageSize.Width < UIScreen.MainScreen.Bounds.Width && originalImageSize.Height < UIScreen.MainScreen.Bounds.Width)
            {
                originalImageSize = ImageHelper.CalculateInSampleSize(originalImageSize,
                                                          (float)UIScreen.MainScreen.Bounds.Width,
                                                          (float)UIScreen.MainScreen.Bounds.Width, true);
            }

            ContentInset = new UIEdgeInsets(0, 0, 0, 0);
            MinimumZoomScale = 1;
            ZoomScale = 1;
            ContentSize = originalImageSize;
            imageView.Frame = new CGRect(new CGPoint(0, 0), originalImageSize);
        }

        public void SetScrollViewInsets()
        {
            nfloat shift;

            var shiftSide = originalImageSize.Height < originalImageSize.Width;
            if (shiftSide)
                shift = (Frame.Height - ContentSize.Height) / 2.0f;
            else
                shift = (Frame.Width - ContentSize.Width) / 2.0f;

            if (shift > 0)
                if (shiftSide)
                    ContentInset = new UIEdgeInsets(shift, 0, 0, 0);
                else
                    ContentInset = new UIEdgeInsets(0, shift, 0, 0);
            else
                ContentInset = new UIEdgeInsets(0, 0, 0, 0);
        }

        public void ZoomTap(bool isToSquareMode, bool isAnimated = true)
        {
            if (originalImageSize.Width < originalImageSize.Height)
            {
                if (isToSquareMode || ZoomScale != Frame.Width / originalImageSize.Width)
                    SetZoomScale(Frame.Width / originalImageSize.Width, isAnimated);
                else
                    SetZoomScale(1f, isAnimated);
            }
            else
            {
                if (isToSquareMode || ZoomScale != Frame.Height / originalImageSize.Height)
                    SetZoomScale(Frame.Height / originalImageSize.Height, isAnimated);
                else
                    SetZoomScale(1f, isAnimated);
            }
        }

        public void RotateTap()
        {
            imageView.Image = ImageHelper.RotateImage(imageView.Image, UIImageOrientation.Right);
            SaveOrientation();
            AdjustImageViewSize(imageView.Image);
            SetScrollViewInsets();
        }

        public void SaveOrientation()
        {
            switch (orientation)
            {
                case UIImageOrientation.Up:
                    orientation = UIImageOrientation.Right;
                    break;
                case UIImageOrientation.Right:
                    orientation = UIImageOrientation.Down;
                    break;
                case UIImageOrientation.Down:
                    orientation = UIImageOrientation.Left;
                    break;
                case UIImageOrientation.Left:
                    orientation = UIImageOrientation.Up;
                    break;
                default:
                    orientation = UIImageOrientation.Up;
                    break;
            }
        }

        public void ApplyCriticalScale()
        {
            nfloat scale = 1;

            var imageRatio = originalImageSize.Width / originalImageSize.Height;

            if (originalImageSize.Height > originalImageSize.Width)
            {
                if (imageRatio < 0.8f)
                    scale = 0.8f / imageRatio;
            }
            else
                if (imageRatio > 1.92f)
                scale = imageRatio / 1.92f;

            MinimumZoomScale = scale;
            SetZoomScale(scale, false);
        }

        public void ApplyRightScale(float zoom = float.MinValue)
        {
            nfloat scale = 0;

            if (originalImageSize.Height > originalImageSize.Width && Frame.Height <= originalImageSize.Height)
                scale = Frame.Width / originalImageSize.Width;

            if (originalImageSize.Height < originalImageSize.Width && Frame.Width <= originalImageSize.Width)
                scale = Frame.Height / originalImageSize.Height;

            if (scale > 1)
            {
                MinimumZoomScale = scale;
                SetZoomScale(scale, false);
            }
            else
                ApplyCriticalScale();

            if (zoom != float.MinValue)
            {
                SetZoomScale(zoom, false);
                return;
            }
        }

        public UIImage CropImage(SavedPhoto photo)
        {
            CGSize scaledImageSize;
            CGPoint offset;

            if (photo.OriginalImageSize.Width == 0 && photo.OriginalImageSize.Height == 0)
            {
                scaledImageSize = imageView.Frame.Size;
                offset = ContentOffset;
            }
            else
            {
                scaledImageSize = new CGSize(photo.OriginalImageSize.Width * photo.Scale, photo.OriginalImageSize.Height * photo.Scale);
                offset = photo.Offset;
            }

            var ratio2 = photo.Image.Size.Width / scaledImageSize.Width;

            nfloat cropWidth;
            nfloat cropHeight;
            nfloat cropX;
            nfloat cropY;

            if (scaledImageSize.Width > Frame.Width)
            {
                cropWidth = Frame.Width * ratio2;
            }
            else
            {
                cropWidth = imageView.Frame.Width * ratio2;
            }

            if (scaledImageSize.Height > Frame.Height)
            {
                cropHeight = Frame.Height * ratio2;
            }
            else
            {
                cropHeight = scaledImageSize.Height * ratio2;
            }

            if (offset.X < 0)
            {
                cropX = 0;
            }
            else
            {
                cropX = offset.X * ratio2;
            }

            if (offset.Y < 0)
            {
                cropY = 0;
            }
            else
            {
                cropY = offset.Y * ratio2;
            }

            var rect = new CGRect(cropX, cropY, cropWidth, cropHeight);

            UIImage cropped = new UIImage();

            using (var cr = photo.Image.CGImage.WithImageInRect(rect))
            {
                cropped = UIImage.FromImage(cr);
            }

            var newSize = ImageHelper.CalculateInSampleSize(cropped.Size, Core.Constants.PhotoMaxSize, Core.Constants.PhotoMaxSize, true);

            UIGraphics.BeginImageContextWithOptions(newSize, false, 1);
            cropped.Draw(new CGRect(new CGPoint(0, 0), newSize));
            cropped = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return cropped;
        }
    }
}
