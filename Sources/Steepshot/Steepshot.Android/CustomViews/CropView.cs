using System;
using System.Threading.Tasks;
using Android.Animation;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Math = System.Math;
using Uri = Android.Net.Uri;

namespace Steepshot.CustomViews
{
    public class CropView : View, GestureDetector.IOnGestureListener, ScaleGestureDetector.IOnScaleGestureListener
    {
        private enum ScaleType
        {
            Custom,
            Square,
            Ratio,
            Undefined
        }

        public class FocusedScale
        {
            public float Scale { get; set; }
            public float FocusedScaleX { get; set; }
            public float FocusedScaleY { get; set; }
        }
        private CropViewGrid _grid;
        public CropViewGrid Grid
        {
            get => _grid ?? (_grid = new CropViewGrid());
            set => _grid = value;
        }
        public FocusedScale DrawableFocusedScale { get; private set; }

        private float SquareScale
        {
            get
            {
                var widthRatio = _imageRawWidth / (float)_width;
                var heightRatio = _imageRawHeight / (float)_height;
                if (widthRatio >= 1 && heightRatio >= 1)
                    return Math.Max(Math.Min(1 / widthRatio, widthRatio), Math.Min(1 / heightRatio, heightRatio));
                return Math.Max(Math.Max(1 / widthRatio, widthRatio), Math.Max(1 / heightRatio, heightRatio));
            }
        }
        private bool DrawableSuitsView
        {
            get
            {
                if (_drawable == null)
                    return false;

                var viewArea = _width * _height;
                var drawableArea = _drawable.IntrinsicWidth * _drawable.IntrinsicHeight;
                var areaRatio = viewArea / (float)drawableArea;

                return areaRatio >= 0.5F && areaRatio <= 2F;
            }
        }
        private float FitDrawableScale
        {
            get
            {
                float scale;

                if (ImageRatioValid)
                {
                    bool drawableIsWiderThanView = ImageRatio > ViewRatio;

                    if (drawableIsWiderThanView)
                        scale = _width / (float)_imageRawWidth;
                    else
                        scale = _height / (float)_imageRawHeight;
                }
                else if (_imageRawHeight < _width || _imageRawHeight < _height)
                {
                    if (ImageRatio < _maximumRatio)
                    {
                        GetBoundsForWidthAndRatio(_imageRawWidth, _minimumRatio, _helperRect);
                        scale = _helperRect.Height() / _height;
                    }
                    else
                    {
                        GetBoundsForHeightAndRatio(_imageRawHeight, _maximumRatio, _helperRect);
                        scale = _helperRect.Width() / _width;
                    }
                }
                else
                {
                    if (ImageRatio < _minimumRatio)
                    {
                        GetBoundsForHeightAndRatio(_height, _minimumRatio, _helperRect);
                        scale = _helperRect.Width() / _imageRawWidth;
                    }
                    else
                    {
                        GetBoundsForWidthAndRatio(_width, _maximumRatio, _helperRect);
                        scale = _helperRect.Height() / _imageRawHeight;
                    }
                }

                return scale;
            }
        }
        private bool ImageRatioValid => ImageRatio >= _minimumRatio && ImageRatio <= _maximumRatio;
        private float ImageRatio => _imageRawWidth / (float)_imageRawHeight;
        private float ViewRatio => _width / (float)_height;
        private float DisplayDrawableWidth => DrawableFocusedScale.Scale * _imageRawWidth;
        private float DisplayDrawableHeight => DrawableFocusedScale.Scale * _imageRawHeight;
        private float MaximumAllowedScale
        {
            get
            {
                var widthRatio = _imageRawWidth / (float)_width;
                var heightRatio = _imageRawHeight / (float)_height;
                return Math.Max(Math.Max(1 / widthRatio, widthRatio), Math.Max(1 / heightRatio, heightRatio)) * 1.2f;
            }
        }
        private float MinimumAllowedScale => FitDrawableScale;
        private long _backDuration => 400;

        private Uri _imageUri;
        private int _imageRawWidth;
        private int _imageRawHeight;
        private int _width;
        private int _height;
        private Drawable _drawable;
        private RectF _helperRect;
        private float _displayDrawableLeft;
        private float _displayDrawableTop;
        private float _minimumRatio;
        private float _maximumRatio;
        private float _defaultRatio;
        private float _maximumOverScroll;
        private float _maximumOverScale;
        //private float _drawableScale;
        //private float _scaleFocusX;
        //private float _scaleFocusY;

        private ScaleType _currentScaleType;
        private GestureDetector _gestureDetector;
        private ScaleGestureDetector _scaleGestureDetector;
        private ValueAnimator _gesturesAnimator;

        #region Init

        public CropView(Context context) : base(context)
        {
            Initialize(context);
        }

        public CropView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize(context);
        }

        public CropView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Initialize(context);
        }

        public CropView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Initialize(context);
        }
        #endregion

        private void Initialize(Context context)
        {
            _gestureDetector = new GestureDetector(Context, this);
            _scaleGestureDetector = new ScaleGestureDetector(context, this);

            _minimumRatio = 4f / 5f;
            _maximumRatio = 2.5f;
            _defaultRatio = 1f;
            _maximumOverScroll = Resources.DisplayMetrics.Density * 150f;
            _maximumOverScale = 0.2f;
            _helperRect = new RectF();
            DrawableFocusedScale = new FocusedScale();

            _gesturesAnimator = new ValueAnimator();
            _gesturesAnimator.SetDuration(_backDuration);
            _gesturesAnimator.SetFloatValues(0, 1);
            _gesturesAnimator.SetInterpolator(new DecelerateInterpolator(0.25F));
            _gesturesAnimator.Update += GesturesAnimatorOnUpdate;

            Grid.SetCallback(this);
        }

        public override void InvalidateDrawable(Drawable drawable)
        {
            Invalidate();
        }

        public void SetRatios(float defaultRatio, float minimumRatio, float maximumRatio)
        {
            _defaultRatio = defaultRatio;
            _minimumRatio = minimumRatio;
            _maximumRatio = maximumRatio;

            if (_gesturesAnimator.IsRunning)
                _gesturesAnimator.Cancel();

            _drawable = null;

            RequestLayout();
        }

        public void SetImageUri(Uri uri)
        {
            _imageUri = uri;
            _drawable = null;

            RequestLayout();
            Invalidate();
        }

        private Task<BitmapDrawable> MakeDrawable(int targetWidth, int targetHeight) => Task.Run(() =>
         {
             var options = new BitmapFactory.Options
             {
                 InSampleSize = 1,
                 InJustDecodeBounds = true
             };

             try
             {
                 BitmapFactory.DecodeStream(Context.ContentResolver.OpenInputStream(_imageUri), null, options);

                 _imageRawWidth = options.OutWidth;
                 _imageRawHeight = options.OutHeight;

                 var resultWidth = _imageRawWidth;
                 var resultHeight = _imageRawHeight;

                 GC.Collect(0);

                 var totalMemory = Java.Lang.Runtime.GetRuntime().MaxMemory();
                 var allowedMemoryToUse = totalMemory / 8;
                 var maximumAreaPossibleAccordingToAvailableMemory = (int)(allowedMemoryToUse / 4);

                 var targetArea = Math.Min(targetWidth * targetHeight * 4, maximumAreaPossibleAccordingToAvailableMemory);
                 var resultArea = resultWidth * resultHeight;

                 while (resultArea > targetArea)
                 {
                     options.InSampleSize *= 2;

                     resultWidth = _imageRawWidth / options.InSampleSize;
                     resultHeight = _imageRawHeight / options.InSampleSize;

                     resultArea = resultWidth * resultHeight;
                 }

                 options.InJustDecodeBounds = false;

                 var bitmap = GetBitmap(_imageUri, options);

                 if (bitmap == null)
                     return null;

                 var beforeRatio = _imageRawWidth / (float)_imageRawHeight;
                 var afterRatio = bitmap.Width / (float)bitmap.Height;

                 if (beforeRatio < 1 && afterRatio > 1 || beforeRatio > 1 && afterRatio < 1)
                 {
                     var rawWidth = _imageRawWidth;
                     _imageRawWidth = _imageRawHeight;
                     _imageRawHeight = rawWidth;
                 }

                 return new BitmapDrawable(Context.Resources, bitmap);
             }
             catch
             {
                 return null;
             }
         });

        private Bitmap GetBitmap(Uri uri, BitmapFactory.Options options)
        {
            Bitmap bitmap = null;

            while (true)
            {
                try
                {
                    bitmap = BitmapFactory.DecodeStream(Context.ContentResolver.OpenInputStream(uri), null, options);
                    break;
                }
                catch
                {
                    options.InSampleSize *= 2;

                    if (options.InSampleSize >= 1024)
                        break;
                }
            }

            return bitmap;
        }

        protected static Bitmap ResizeBitmap(Bitmap bitmap, int newWidth, int newHeight)
        {
            var resizedBitmap = Bitmap.CreateBitmap(newWidth, newHeight, Bitmap.Config.Argb8888);

            var scaleX = newWidth / (float)bitmap.Width;
            var scaleY = newHeight / (float)bitmap.Height;
            var pivotX = 0;
            var pivotY = 0;

            var scaleMatrix = new Matrix();
            scaleMatrix.SetScale(scaleX, scaleY, pivotX, pivotY);

            var canvas = new Canvas(resizedBitmap) { Matrix = scaleMatrix };
            canvas.DrawBitmap(bitmap, 0, 0, new Paint(PaintFlags.FilterBitmap));

            return resizedBitmap;
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
            var widthSize = MeasureSpec.GetSize(widthMeasureSpec);
            var heightMode = MeasureSpec.GetMode(heightMeasureSpec);
            var heightSize = MeasureSpec.GetSize(heightMeasureSpec);

            var targetWidth = 1;
            var targetHeight = 1;

            switch (widthMode)
            {
                case MeasureSpecMode.Exactly:
                    targetWidth = widthSize;

                    switch (heightMode)
                    {
                        case MeasureSpecMode.Exactly:
                            targetHeight = heightSize;
                            break;
                        case MeasureSpecMode.AtMost:
                            targetHeight = Math.Min(heightSize, (int)(targetWidth / _defaultRatio));
                            break;
                        case MeasureSpecMode.Unspecified:
                            targetHeight = (int)(targetWidth / _defaultRatio);
                            break;
                    }
                    break;
                case MeasureSpecMode.AtMost:
                    switch (heightMode)
                    {
                        case MeasureSpecMode.Exactly:
                            targetHeight = heightSize;
                            targetWidth = Math.Min(widthSize, (int)(targetHeight * _defaultRatio));
                            break;
                        case MeasureSpecMode.AtMost:
                            float specRatio = widthSize / (float)heightSize;

                            if (specRatio == _defaultRatio)
                            {
                                targetWidth = widthSize;
                                targetHeight = heightSize;
                            }
                            else if (specRatio > _defaultRatio)
                            {
                                targetHeight = heightSize;
                                targetWidth = (int)(targetHeight * _defaultRatio);
                            }
                            else
                            {
                                targetWidth = widthSize;
                                targetHeight = (int)(targetWidth / _defaultRatio);
                            }
                            break;
                        case MeasureSpecMode.Unspecified:
                            targetWidth = widthSize;
                            targetHeight = (int)(targetWidth / _defaultRatio);
                            break;
                    }
                    break;
                case MeasureSpecMode.Unspecified:
                    switch (heightMode)
                    {
                        case MeasureSpecMode.Exactly:
                            targetHeight = heightSize;
                            targetWidth = (int)(targetHeight * _defaultRatio);
                            break;
                        case MeasureSpecMode.AtMost:
                            targetHeight = heightSize;
                            targetWidth = (int)(targetHeight * _defaultRatio);
                            break;
                        case MeasureSpecMode.Unspecified:
                            targetWidth = (int)_maximumOverScroll;
                            targetHeight = (int)_maximumOverScroll;
                            break;
                    }
                    break;
            }

            SetMeasuredDimension(targetWidth, targetHeight);
        }
        protected override async void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            base.OnLayout(changed, left, top, right, bottom);
            _width = right - left;
            _height = bottom - top;

            if (_width == 0 || _height == 0 || _imageUri == null)
                return;

            if (DrawableSuitsView)
            {
                return;
            }

            _drawable = await MakeDrawable(_width, _height);
            Reset(ScaleType.Square);
        }
        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            if (_drawable == null)
                return;

            GetDisplayDrawableBounds(_helperRect);

            _drawable.SetBounds((int)_helperRect.Left, (int)_helperRect.Top, (int)_helperRect.Right, (int)_helperRect.Bottom);
            _drawable.Draw(canvas);

            Grid.Draw(canvas);
        }

        public void SwitchScale()
        {
            Reset(_currentScaleType == ScaleType.Square || _currentScaleType == ScaleType.Undefined ? ScaleType.Ratio : ScaleType.Square);
        }
        private void Reset(ScaleType resetType)
        {
            if (_gesturesAnimator.IsRunning)
                _gesturesAnimator.Cancel();

            switch (resetType)
            {
                case ScaleType.Square:
                    SetDrawableScale(SquareScale);
                    _currentScaleType = ScaleType.Square;
                    break;
                case ScaleType.Ratio:
                case ScaleType.Undefined:
                    SetDrawableScale(FitDrawableScale);
                    _currentScaleType = ScaleType.Ratio;
                    break;
            }

            if (resetType != ScaleType.Custom)
                PlaceDrawableInTheCenter();
            UpdateGrid();

            Invalidate();
        }
        private void SetDrawableScale(float scale)
        {
            DrawableFocusedScale.Scale = scale;

            Invalidate();
        }
        private void GetBoundsForWidthAndRatio(float width, float ratio, RectF rect)
        {
            var height = width / ratio;
            rect.Set(0, 0, width, height);
        }
        private void GetBoundsForHeightAndRatio(float height, float ratio, RectF rect)
        {
            var width = height * ratio;
            rect.Set(0, 0, width, height);
        }
        private void PlaceDrawableInTheCenter()
        {
            _displayDrawableLeft = (_width - DisplayDrawableWidth) / 2;
            _displayDrawableTop = (_height - DisplayDrawableHeight) / 2;

            Invalidate();
        }
        private void UpdateGrid()
        {
            GetDisplayDrawableBounds(_helperRect);

            _helperRect.Intersect(0, 0, _width, _height);

            _helperRect.Set(_helperRect.Left, _helperRect.Top, _helperRect.Left + _helperRect.Width(), _helperRect.Top + _helperRect.Height());
            SetGridBounds(_helperRect);

            Invalidate();
        }
        private void SetGridBounds(RectF bounds)
        {
            Grid.SetBounds((int)bounds.Left, (int)bounds.Top, (int)bounds.Right, (int)bounds.Bottom);

            Invalidate();
        }
        private void GetDisplayDrawableBounds(RectF bounds)
        {
            bounds.Left = _displayDrawableLeft;
            bounds.Top = _displayDrawableTop;
            bounds.Right = bounds.Left + DisplayDrawableWidth;
            bounds.Bottom = bounds.Top + DisplayDrawableHeight;
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            if (_drawable == null)
                return false;

            _currentScaleType = ScaleType.Undefined;
            _gestureDetector.OnTouchEvent(e);
            _scaleGestureDetector.OnTouchEvent(e);

            if (e.Action == MotionEventActions.Up || e.Action == MotionEventActions.Cancel || e.Action == MotionEventActions.Outside)
            {
                _gesturesAnimator.Start();
            }

            return true;
        }
        private void GesturesAnimatorOnUpdate(object sender, ValueAnimator.AnimatorUpdateEventArgs animatorUpdateEventArgs)
        {
            var value = (float)animatorUpdateEventArgs.Animation.AnimatedValue;
            GetDisplayDrawableBounds(_helperRect);

            var overScrollX = MeasureOverScrollX(_helperRect);
            var overScrollY = MeasureOverScrollY(_helperRect);
            var overScale = MeasureOverScale();

            _displayDrawableLeft -= overScrollX * value;
            _displayDrawableTop -= overScrollY * value;

            var targetScale = DrawableFocusedScale.Scale / overScale;
            var newScale = (1 - value) * DrawableFocusedScale.Scale + value * targetScale;

            SetScaleKeepingFocus(newScale, DrawableFocusedScale.FocusedScaleX, DrawableFocusedScale.FocusedScaleY);

            UpdateGrid();
            Invalidate();
        }
        public bool OnDown(MotionEvent e) => true;
        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY) => false;
        public void OnLongPress(MotionEvent e) { }
        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            distanceX = -distanceX;
            distanceY = -distanceY;

            GetDisplayDrawableBounds(_helperRect);

            var overScrollX = MeasureOverScrollX(_helperRect);
            var overScrollY = MeasureOverScrollY(_helperRect);

            distanceX = ApplyOverScrollFix(distanceX, overScrollX);
            distanceY = ApplyOverScrollFix(distanceY, overScrollY);

            _displayDrawableLeft += distanceX;
            _displayDrawableTop += distanceY;

            UpdateGrid();
            Invalidate();

            return true;
        }
        private float MeasureOverScrollX(RectF displayDrawableBounds)
        {
            var drawableIsSmallerThanView = displayDrawableBounds.Width() <= _width;

            if (drawableIsSmallerThanView)
                return displayDrawableBounds.CenterX() - _width / 2;

            if (displayDrawableBounds.Left <= 0 && displayDrawableBounds.Right >= _width)
                return 0;

            if (displayDrawableBounds.Left < 0)
                return displayDrawableBounds.Right - _width;

            if (displayDrawableBounds.Right > _width)
                return displayDrawableBounds.Left;

            return 0;
        }
        private float MeasureOverScrollY(RectF displayDrawableBounds)
        {
            var drawableIsSmallerThanView = displayDrawableBounds.Height() < _height;

            if (drawableIsSmallerThanView)
                return displayDrawableBounds.CenterY() - _height / 2;

            if (displayDrawableBounds.Top <= 0 && displayDrawableBounds.Bottom >= _height)
                return 0;

            if (displayDrawableBounds.Top < 0)
                return displayDrawableBounds.Bottom - _height;

            if (displayDrawableBounds.Bottom > _height)
                return displayDrawableBounds.Top;

            return 0;
        }
        private float ApplyOverScrollFix(float distance, float overScroll)
        {
            if (overScroll * distance <= 0)
                return distance;

            var offRatio = Math.Abs(overScroll) / _maximumOverScroll;

            distance -= distance * (float)Math.Sqrt(offRatio);

            return distance;
        }
        public void OnShowPress(MotionEvent e) { }
        public bool OnSingleTapUp(MotionEvent e) => false;
        public bool OnScale(ScaleGestureDetector detector)
        {
            float overScale = MeasureOverScale();
            float scale = ApplyOverScaleFix(detector.ScaleFactor, overScale);

            DrawableFocusedScale.FocusedScaleX = detector.FocusX;
            DrawableFocusedScale.FocusedScaleY = detector.FocusY;

            SetScaleKeepingFocus(DrawableFocusedScale.Scale * scale, DrawableFocusedScale.FocusedScaleX, DrawableFocusedScale.FocusedScaleY);

            return true;
        }
        private float MeasureOverScale()
        {
            var maximumAllowedScale = MaximumAllowedScale;
            var minimumAllowedScale = MinimumAllowedScale;

            if (maximumAllowedScale < minimumAllowedScale)
                maximumAllowedScale = minimumAllowedScale;

            if (DrawableFocusedScale.Scale < minimumAllowedScale)
                return DrawableFocusedScale.Scale / minimumAllowedScale;
            if (DrawableFocusedScale.Scale > maximumAllowedScale)
                return DrawableFocusedScale.Scale / maximumAllowedScale;
            return 1;
        }
        private float ApplyOverScaleFix(float scale, float overScale)
        {
            if (overScale == 1)
                return scale;

            if (overScale > 1)
                overScale = 1F / overScale;

            var wentOverScaleRatio = (overScale - _maximumOverScale) / (1 - _maximumOverScale);

            if (wentOverScaleRatio < 0)
                wentOverScaleRatio = 0;

            // 1 -> scale , 0 -> 1
            // scale * f(1) = scale
            // scale * f(0) = 1

            // f(1) = 1
            // f(0) = 1/scale

            scale *= wentOverScaleRatio + (1 - wentOverScaleRatio) / scale;

            return scale;
        }
        public void SetScaleKeepingFocus(float scale, float focusX, float focusY)
        {
            GetDisplayDrawableBounds(_helperRect);

            var focusRatioX = (focusX - _helperRect.Left) / _helperRect.Width();
            var focusRatioY = (focusY - _helperRect.Top) / _helperRect.Height();

            DrawableFocusedScale.Scale = scale;

            GetDisplayDrawableBounds(_helperRect);

            var scaledFocusX = _helperRect.Left + focusRatioX * _helperRect.Width();
            var scaledFocusY = _helperRect.Top + focusRatioY * _helperRect.Height();

            _displayDrawableLeft += focusX - scaledFocusX;
            _displayDrawableTop += focusY - scaledFocusY;

            UpdateGrid();
            Invalidate();
        }
        public bool OnScaleBegin(ScaleGestureDetector detector) => true;
        public void OnScaleEnd(ScaleGestureDetector detector) { }
    }
}