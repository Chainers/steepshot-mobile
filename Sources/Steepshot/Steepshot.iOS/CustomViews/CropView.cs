using System;
using CoreAnimation;
using CoreGraphics;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.CustomViews
{
    public class CropView : UIScrollView
    {
        public CGSize originalContentSize;
        public UIImageOrientation orientation = UIImageOrientation.Up;
        private UIBezierPath _path;
        private CAShapeLayer _shapeLayer = new CAShapeLayer();
        private readonly UIColor _strokeColor = UIColor.FromRGB(210, 210, 210);
        private const int _linesCount = 9;

        public UIImageView ImageView { get; set; }
        public VideoView VideoView { get; set; }

        public CropView(CGRect _frame)
        {
            ImageView = new UIImageView(_frame);
            ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            VideoView = new VideoView(_frame, false, false);
            VideoView.ContentMode = UIViewContentMode.ScaleAspectFit;
            VideoView.Hidden = true;

            BackgroundColor = Constants.R245G245B245;
            Frame = _frame;
            Bounces = false;
            MinimumZoomScale = 1f;
            MaximumZoomScale = 4f;
            ViewForZoomingInScrollView += (UIScrollView sv) => { return ImageView; };
            AddSubview(ImageView);
            AddSubview(VideoView);
            ContentSize = new CGSize(UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Width);
            DidZoom += (t, u) =>
            {
                SetScrollViewInsets();
                DrawGrid();
            };

            Scrolled += (object sender, EventArgs e) =>
            {
                DrawGrid();
            };

            BouncesZoom = false;
            _shapeLayer.LineWidth = 1;
            Layer.AddSublayer(_shapeLayer);
        }

        public void AdjustImageViewSize(UIImage img)
        {
            var w = img.Size.Width / Core.Constants.PhotoMaxSize * UIScreen.MainScreen.Bounds.Width;
            var h = img.Size.Height / Core.Constants.PhotoMaxSize * UIScreen.MainScreen.Bounds.Width;
            originalContentSize = new CGSize(w, h);

            if (originalContentSize.Width < UIScreen.MainScreen.Bounds.Width && originalContentSize.Height < UIScreen.MainScreen.Bounds.Width)
            {
                originalContentSize = ImageHelper.CalculateInSampleSize(originalContentSize,
                                                          (float)UIScreen.MainScreen.Bounds.Width,
                                                          (float)UIScreen.MainScreen.Bounds.Width, true);
            }

            ContentInset = new UIEdgeInsets(0, 0, 0, 0);
            MinimumZoomScale = 1;
            ZoomScale = 1;
            ContentSize = originalContentSize;
            ImageView.Frame = new CGRect(new CGPoint(0, 0), originalContentSize);
        }

        public void AdjustVideoViewSize(CGSize assetSize, UIInterfaceOrientation orientation)
        {
            var w = assetSize.Width / Core.Constants.PhotoMaxSize * UIScreen.MainScreen.Bounds.Width;
            var h = assetSize.Height / Core.Constants.PhotoMaxSize * UIScreen.MainScreen.Bounds.Width;
            var isPortrait = orientation == UIInterfaceOrientation.Portrait || orientation == UIInterfaceOrientation.PortraitUpsideDown;
            originalContentSize = isPortrait ? new CGSize(h, w) : new CGSize(w, h);

            if (originalContentSize.Width < UIScreen.MainScreen.Bounds.Width && originalContentSize.Height < UIScreen.MainScreen.Bounds.Width)
            {
                originalContentSize = ImageHelper.CalculateInSampleSize(originalContentSize,
                                                          (float)UIScreen.MainScreen.Bounds.Width,
                                                          (float)UIScreen.MainScreen.Bounds.Width, true);
            }

            if (!isPortrait && originalContentSize.Height < UIScreen.MainScreen.Bounds.Width)
            {
                var reqWidth = UIScreen.MainScreen.Bounds.Width / originalContentSize.Height * originalContentSize.Width;
                originalContentSize = new CGSize(reqWidth, UIScreen.MainScreen.Bounds.Width);
            }

            if (isPortrait && originalContentSize.Width < UIScreen.MainScreen.Bounds.Width)
            {
                var reqWidth = UIScreen.MainScreen.Bounds.Width / originalContentSize.Width * originalContentSize.Height;
                originalContentSize = new CGSize(UIScreen.MainScreen.Bounds.Width, reqWidth);
            }

            ContentSize = originalContentSize;
            MinimumZoomScale = 1;
            ContentInset = new UIEdgeInsets(0, 0, 0, 0);
            VideoView.Frame = new CGRect(new CGPoint(0, 0), originalContentSize);
        }

        public void SetScrollViewInsets()
        {
            nfloat shift;

            var shiftSide = originalContentSize.Height < originalContentSize.Width;
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
            if (originalContentSize.Width < originalContentSize.Height)
            {
                if (isToSquareMode || ZoomScale != Frame.Width / originalContentSize.Width)
                    SetZoomScale(Frame.Width / originalContentSize.Width, isAnimated);
                else
                    SetZoomScale(1f, isAnimated);
            }
            else
            {
                if (isToSquareMode || ZoomScale != Frame.Height / originalContentSize.Height)
                    SetZoomScale(Frame.Height / originalContentSize.Height, isAnimated);
                else
                    SetZoomScale(1f, isAnimated);
            }
        }

        public void RotateTap()
        {
            ImageView.Image = ImageHelper.RotateImage(ImageView.Image, UIImageOrientation.Right);
            SaveOrientation();
            AdjustImageViewSize(ImageView.Image);
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

            var imageRatio = originalContentSize.Width / originalContentSize.Height;

            if (originalContentSize.Height > originalContentSize.Width)
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

            if (originalContentSize.Height > originalContentSize.Width && Frame.Height <= originalContentSize.Height)
                scale = Frame.Width / originalContentSize.Width;

            if (originalContentSize.Height < originalContentSize.Width && Frame.Width <= originalContentSize.Width)
                scale = Frame.Height / originalContentSize.Height;

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
                scaledImageSize = ImageView.Frame.Size;
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

            if ((int)scaledImageSize.Width >= (int)Frame.Width)
            {
                cropWidth = Frame.Width * ratio2;
            }
            else
            {
                cropWidth = ImageView.Frame.Width * ratio2;
            }

            if ((int)scaledImageSize.Height >= (int)Frame.Height)
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

            var shouldIncrease = cropped.Size.Width < Core.Constants.PhotoMaxSize && cropped.Size.Height < Core.Constants.PhotoMaxSize;
            var newSize = ImageHelper.CalculateInSampleSize(rect.Size, Core.Constants.PhotoMaxSize, Core.Constants.PhotoMaxSize, shouldIncrease);
            newSize = new CGSize((int)newSize.Width, (int)newSize.Height);

            UIGraphics.BeginImageContextWithOptions(newSize, false, 1);
            cropped.Draw(new CGRect(new CGPoint(0, 0), newSize));
            cropped = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return cropped;
        }

        private void DrawGrid()
        {
            var size = new CGRect()
            {
                Width = ContentSize.Width > Frame.Width ? Frame.Width : ContentSize.Width,
                Height = ContentSize.Height > Frame.Height ? Frame.Height : ContentSize.Height,
                X = ContentOffset.X < 0 ? 0 : ContentOffset.X,
                Y = ContentOffset.Y < 0 ? 0 : ContentOffset.Y,
            };

            var gridHeight = size.Height / (_linesCount + 1);
            var gridWidth = size.Width / (_linesCount + 1);

            _path = UIBezierPath.Create();
            _path.LineWidth = 1;

            for (int i = 1; i < _linesCount + 1; i++)
            {
                var start = new CGPoint(x: i * gridWidth + size.X, y: size.Y);
                var end = new CGPoint(x: i * gridWidth + size.X, y: size.Height + size.Y);
                _path.MoveTo(start);
                _path.AddLineTo(end);
            }

            for (int i = 1; i < _linesCount + 1; i++)
            {
                var start = new CGPoint(x: size.X, y: i * gridHeight + size.Y);
                var end = new CGPoint(x: size.Width + size.X, y: i * gridHeight + size.Y);
                _path.MoveTo(start);
                _path.AddLineTo(end);
            }
            _shapeLayer.RemoveAllAnimations();
            _shapeLayer.StrokeColor = _strokeColor.ColorWithAlpha(0.15f).CGColor;

            var animation = CABasicAnimation.FromKeyPath("strokeColor");
            animation.BeginTime = CAAnimation.CurrentMediaTime() + 0.2; //delay
            animation.Duration = 0.2;
            animation.SetTo(_strokeColor.ColorWithAlpha(0).CGColor);
            animation.RemovedOnCompletion = false;
            animation.FillMode = CAFillMode.Forwards;

            _shapeLayer.AddAnimation(animation, "flashStrokeColor");
            _shapeLayer.Path = _path.CGPath;
            _path.ClosePath();
        }
    }
}
