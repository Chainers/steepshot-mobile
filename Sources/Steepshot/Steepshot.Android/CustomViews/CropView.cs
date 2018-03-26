using System.Threading.Tasks;
using Android.Animation;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Java.IO;
using Square.Picasso;
using Steepshot.Utils;
using Math = System.Math;
using Uri = Android.Net.Uri;

namespace Steepshot.CustomViews
{
    public class CropView : View, GestureDetector.IOnGestureListener, ScaleGestureDetector.IOnScaleGestureListener, ITarget
    {
        private enum ScaleType
        {
            Bind,
            Square,
            Ratio,
            KeepScale,
            Undefined
        }

        private const float MinimumRatio = 0.8f;
        private const float MaximumRatio = 1.92f;
        private const float DefaultRatio = 1f;
        private const float MaximumOverscrollMultiplier = 150f;
        private const float MaximumOverscale = 0.2f;
        public const int MaxImageSize = 1600;

        private CropViewGrid _grid;
        private CropViewGrid Grid => _grid ?? (_grid = new CropViewGrid());
        private ImageParameters _drawableImageParameters;
        public ImageParameters DrawableImageParameters
        {
            get
            {
                _drawableImageParameters.CropBounds = Grid.Bounds;
                return _drawableImageParameters;
            }
            private set => _drawableImageParameters = value;
        }
        public float CornerRadius { get; set; }
        public bool UseStrictBounds
        {
            set
            {
                _useStrictBounds = value;
                _reloadImage = false;
                if (_useStrictBounds)
                {
                    _minimumRatio = _maximumRatio = _defaultRatio = Grid.Bounds.Width() / (float)Grid.Bounds.Height();
                    if (ImageRatio <= 1)
                        _targetWidth = (int)DrawableImageParameters.PreviewBounds.Width();
                    else
                        _targetHeight = (int)DrawableImageParameters.PreviewBounds.Height();
                }
                else
                    _currentScaleType = ScaleType.KeepScale;
            }
        }

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
                else if (_imageRawWidth < _width || _imageRawHeight < _height)
                {
                    if (ImageRatio < _maximumRatio)
                    {
                        GetBoundsForWidthAndRatio(_imageRawWidth, _minimumRatio, DrawableImageParameters.PreviewBounds);
                        scale = DrawableImageParameters.PreviewBounds.Height() / _height;
                    }
                    else
                    {
                        GetBoundsForHeightAndRatio(_imageRawHeight, _maximumRatio, DrawableImageParameters.PreviewBounds);
                        scale = DrawableImageParameters.PreviewBounds.Width() / _width;
                    }
                }
                else
                {
                    if (ImageRatio < _minimumRatio)
                    {
                        GetBoundsForHeightAndRatio(_height, _minimumRatio, DrawableImageParameters.PreviewBounds);
                        scale = DrawableImageParameters.PreviewBounds.Width() / _imageRawWidth;
                    }
                    else
                    {
                        GetBoundsForWidthAndRatio(_width, _maximumRatio, DrawableImageParameters.PreviewBounds);
                        scale = DrawableImageParameters.PreviewBounds.Height() / _imageRawHeight;
                    }
                }

                return scale;
            }
        }
        private float FitRatioScale
        {
            get
            {
                var widthScale = _imageRawWidth / (float)_width;
                var heightScale = _imageRawHeight / (float)_height;
                return Math.Max(_imageRawWidth <= _width ? Math.Max(1 / widthScale, widthScale) : Math.Min(1 / widthScale, widthScale), _imageRawHeight <= _height ? Math.Max(1 / heightScale, heightScale) : Math.Min(1 / heightScale, heightScale));
            }
        }
        private bool ImageRatioValid => ImageRatio >= _minimumRatio && ImageRatio <= _maximumRatio;
        private float ImageRatio => _imageRawWidth / (float)_imageRawHeight;
        private float ViewRatio => _width / (float)_height;
        private float DisplayDrawableWidth => DrawableImageParameters.Scale * _imageRawWidth;
        private float DisplayDrawableHeight => DrawableImageParameters.Scale * _imageRawHeight;
        private float MaximumAllowedScale => FitRatioScale * 3f;
        private float MinimumAllowedScale => _useStrictBounds ? FitRatioScale : FitDrawableScale;
        private long _backDuration => 400;

        private Uri _imageUri;
        private Drawable _drawable;
        private int _width;
        private int _height;
        private int _targetWidth;
        private int _targetHeight;
        private int _imageRawWidth;
        private int _imageRawHeight;
        private float _displayDrawableLeft;
        private float _displayDrawableTop;
        private float _focusedScaleX;
        private float _focusedScaleY;
        private float _minimumRatio;
        private float _maximumRatio;
        private float _defaultRatio;
        private float _maximumOverScroll;
        private float _maximumOverScale;
        private bool _useStrictBounds;
        private bool _reloadImage;

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

            ResetToDefaults(ScaleType.Square);
            _maximumOverScroll = Resources.DisplayMetrics.Density * MaximumOverscrollMultiplier;
            _maximumOverScale = MaximumOverscale;

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
        private void ResetToDefaults(ScaleType scaleType)
        {
            _displayDrawableLeft = _displayDrawableTop = 0;
            _minimumRatio = MinimumRatio;
            _maximumRatio = MaximumRatio;
            _defaultRatio = DefaultRatio;
            DrawableImageParameters = _drawableImageParameters ?? new ImageParameters();
            _currentScaleType = scaleType;
        }
        public void SetImageUri(Uri uri, ImageParameters parameters)
        {
            _reloadImage = !uri.Equals(_imageUri);
            if (_reloadImage)
            {
                _imageUri = uri;
                _drawable = null;
            }

            if (parameters != null)
            {
                _currentScaleType = ScaleType.Undefined;
                DrawableImageParameters = parameters.Copy();
                _displayDrawableLeft = parameters.PreviewBounds.Left;
                _displayDrawableTop = parameters.PreviewBounds.Top;
            }
            else
            {
                ResetToDefaults(_useStrictBounds ? ScaleType.Bind : _currentScaleType);
            }

            RequestLayout();
            Invalidate();
        }
        public void Rotate(float angle)
        {
            DrawableImageParameters.Rotation = angle;
            _currentScaleType = _useStrictBounds ? ScaleType.Bind : _currentScaleType;
            _reloadImage = true;
            RequestLayout();
        }
        private Task<BitmapDrawable> MakeDrawable(int targetWidth, int targetHeight, float angle = 0) => Task.Run(() =>
           {
               try
               {
                   using (var inputStream = new FileInputStream(_imageUri.Path))
                   {
                       using (var bitmap = BitmapUtils.DecodeSampledBitmapFromDescriptor(inputStream.FD, targetWidth, targetHeight))
                       {
                           var matrix = new Matrix();
                           matrix.PostRotate(angle);
                           var preparedBitmap = Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, false);
                           _imageRawWidth = preparedBitmap.Width;
                           _imageRawHeight = preparedBitmap.Height;

                           return new BitmapDrawable(Context.Resources, preparedBitmap);
                       }
                   }
               }
               catch
               {
                   return null;
               }
           });

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            if (_useStrictBounds)
            {
                SetMeasuredDimension(_targetWidth, _targetHeight);
                return;
            }

            var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
            var widthSize = MeasureSpec.GetSize(widthMeasureSpec);
            var heightMode = MeasureSpec.GetMode(heightMeasureSpec);
            var heightSize = MeasureSpec.GetSize(heightMeasureSpec);

            _targetWidth = 1;
            _targetHeight = 1;

            switch (widthMode)
            {
                case MeasureSpecMode.Exactly:
                    _targetWidth = widthSize;

                    switch (heightMode)
                    {
                        case MeasureSpecMode.Exactly:
                            _targetHeight = heightSize;
                            break;
                        case MeasureSpecMode.AtMost:
                            _targetHeight = Math.Min(heightSize, (int)(_targetWidth / _defaultRatio));
                            break;
                        case MeasureSpecMode.Unspecified:
                            _targetHeight = (int)(_targetWidth / _defaultRatio);
                            break;
                    }
                    break;
                case MeasureSpecMode.AtMost:
                    switch (heightMode)
                    {
                        case MeasureSpecMode.Exactly:
                            _targetHeight = heightSize;
                            _targetWidth = Math.Min(widthSize, (int)(_targetHeight * _defaultRatio));
                            break;
                        case MeasureSpecMode.AtMost:
                            float specRatio = widthSize / (float)heightSize;

                            if (specRatio == _defaultRatio)
                            {
                                _targetWidth = widthSize;
                                _targetHeight = heightSize;
                            }
                            else if (specRatio > _defaultRatio)
                            {
                                _targetHeight = heightSize;
                                _targetWidth = (int)(_targetHeight * _defaultRatio);
                            }
                            else
                            {
                                _targetWidth = widthSize;
                                _targetHeight = (int)(_targetWidth / _defaultRatio);
                            }
                            break;
                        case MeasureSpecMode.Unspecified:
                            _targetWidth = widthSize;
                            _targetHeight = (int)(_targetWidth / _defaultRatio);
                            break;
                    }
                    break;
                case MeasureSpecMode.Unspecified:
                    switch (heightMode)
                    {
                        case MeasureSpecMode.Exactly:
                            _targetHeight = heightSize;
                            _targetWidth = (int)(_targetHeight * _defaultRatio);
                            break;
                        case MeasureSpecMode.AtMost:
                            _targetHeight = heightSize;
                            _targetWidth = (int)(_targetHeight * _defaultRatio);
                            break;
                        case MeasureSpecMode.Unspecified:
                            _targetWidth = (int)_maximumOverScroll;
                            _targetHeight = (int)_maximumOverScroll;
                            break;
                    }
                    break;
            }

            SetMeasuredDimension(_targetWidth, _targetHeight);
        }
        protected override async void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            base.OnLayout(changed, left, top, right, bottom);
            _width = right - left;
            _height = bottom - top;

            if (_width == 0 || _height == 0)
                return;

            if (_reloadImage)
                _drawable = await MakeDrawable(MaxImageSize, MaxImageSize, DrawableImageParameters.Rotation);
            Reset(_currentScaleType);
        }
        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            if (_drawable == null)
                return;

            Path path = new Path();
            path.AddRoundRect(new RectF(0, 0, canvas.Width, canvas.Height), CornerRadius, CornerRadius, Path.Direction.Cw);
            canvas.ClipPath(path, Region.Op.Replace);

            GetDisplayDrawableBounds(DrawableImageParameters.PreviewBounds);

            _drawable.SetBounds((int)DrawableImageParameters.PreviewBounds.Left, (int)DrawableImageParameters.PreviewBounds.Top, (int)DrawableImageParameters.PreviewBounds.Right, (int)DrawableImageParameters.PreviewBounds.Bottom);
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
                    SetDrawableScale(FitDrawableScale);
                    _currentScaleType = ScaleType.Ratio;
                    break;
                case ScaleType.Bind:
                    SetDrawableScale(FitRatioScale);
                    _currentScaleType = ScaleType.Undefined;
                    break;
            }

            if (resetType != ScaleType.Undefined)
                PlaceDrawableInTheCenter();
            else
                SetScaleKeepingFocus(DrawableImageParameters.Scale, _focusedScaleX, _focusedScaleY);
            UpdateGrid();

            Invalidate();
        }
        private void SetDrawableScale(float scale)
        {
            DrawableImageParameters.Scale = scale;

            Invalidate();
        }
        private void GetBoundsForWidthAndRatio(float width, float ratio, RectF rect)
        {
            float height = width / ratio;
            rect.Set(0, 0, width, height);
        }
        private void GetBoundsForHeightAndRatio(float height, float ratio, RectF rect)
        {
            float width = height * ratio;
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
            GetDisplayDrawableBounds(DrawableImageParameters.PreviewBounds);

            DrawableImageParameters.PreviewBounds.Intersect(0, 0, _width, _height);

            DrawableImageParameters.PreviewBounds.Set(DrawableImageParameters.PreviewBounds.Left, DrawableImageParameters.PreviewBounds.Top, DrawableImageParameters.PreviewBounds.Left + DrawableImageParameters.PreviewBounds.Width(), DrawableImageParameters.PreviewBounds.Top + DrawableImageParameters.PreviewBounds.Height());
            SetGridBounds(DrawableImageParameters.PreviewBounds);

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

        public Bitmap Crop(Uri uri, ImageParameters parameters)
        {
            using (var fileInputStream = new FileInputStream(uri.Path))
            {
                using (var bitmap = BitmapUtils.DecodeSampledBitmapFromDescriptor(fileInputStream.FD, MaxImageSize, MaxImageSize))
                {
                    parameters.CropBounds.Offset(-(int)parameters.PreviewBounds.Left, -(int)parameters.PreviewBounds.Top);
                    var left = (int)Math.Round(parameters.CropBounds.Left / parameters.Scale);
                    var top = (int)Math.Round(parameters.CropBounds.Top / parameters.Scale);
                    var width = (int)Math.Round(parameters.CropBounds.Width() / parameters.Scale);
                    var height = (int)Math.Round(parameters.CropBounds.Height() / parameters.Scale);
                    var matrix = new Matrix();
                    matrix.PostScale(parameters.Scale, parameters.Scale);
                    matrix.PostRotate(parameters.Rotation);
                    var croppedBitmap = Bitmap.CreateBitmap(bitmap, left, top, width, height, matrix, false);
                    bitmap.Recycle();
                    return croppedBitmap;
                }
            }
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            if (_drawable == null)
                return false;

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
            float value = (float)animatorUpdateEventArgs.Animation.AnimatedValue;
            GetDisplayDrawableBounds(DrawableImageParameters.PreviewBounds);

            float overScrollX = MeasureOverScrollX(DrawableImageParameters.PreviewBounds);
            float overScrollY = MeasureOverScrollY(DrawableImageParameters.PreviewBounds);
            float overScale = MeasureOverScale();

            _displayDrawableLeft -= overScrollX * value;
            _displayDrawableTop -= overScrollY * value;

            float targetScale = DrawableImageParameters.Scale / overScale;
            float newScale = (1 - value) * DrawableImageParameters.Scale + value * targetScale;

            SetScaleKeepingFocus(newScale, _focusedScaleX, _focusedScaleY);

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

            GetDisplayDrawableBounds(DrawableImageParameters.PreviewBounds);

            float overScrollX = MeasureOverScrollX(DrawableImageParameters.PreviewBounds);
            float overScrollY = MeasureOverScrollY(DrawableImageParameters.PreviewBounds);

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
            bool drawableIsSmallerThanView = displayDrawableBounds.Height() < _height;

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

            float offRatio = Math.Abs(overScroll) / _maximumOverScroll;

            distance -= distance * (float)Math.Sqrt(offRatio);

            return distance;
        }
        public void OnShowPress(MotionEvent e) { }
        public bool OnSingleTapUp(MotionEvent e) => false;
        public bool OnScale(ScaleGestureDetector detector)
        {
            float overScale = MeasureOverScale();
            float scale = ApplyOverScaleFix(detector.ScaleFactor, overScale);

            _focusedScaleX = detector.FocusX;
            _focusedScaleY = detector.FocusY;

            SetScaleKeepingFocus(DrawableImageParameters.Scale * scale, _focusedScaleX, _focusedScaleY);

            return true;
        }
        private float MeasureOverScale()
        {
            float maximumAllowedScale = MaximumAllowedScale;
            float minimumAllowedScale = MinimumAllowedScale;

            if (maximumAllowedScale < minimumAllowedScale)
                maximumAllowedScale = minimumAllowedScale;

            if (DrawableImageParameters.Scale < minimumAllowedScale)
                return DrawableImageParameters.Scale / minimumAllowedScale;
            if (DrawableImageParameters.Scale > maximumAllowedScale)
                return DrawableImageParameters.Scale / maximumAllowedScale;
            return 1;
        }
        private float ApplyOverScaleFix(float scale, float overScale)
        {
            if (overScale == 1)
                return scale;

            if (overScale > 1)
                overScale = 1F / overScale;

            float wentOverScaleRatio = (overScale - _maximumOverScale) / (1 - _maximumOverScale);

            if (wentOverScaleRatio < 0)
                wentOverScaleRatio = 0;

            scale *= wentOverScaleRatio + (1 - wentOverScaleRatio) / scale;

            return scale;
        }
        private void SetScaleKeepingFocus(float scale, float focusX, float focusY)
        {
            GetDisplayDrawableBounds(DrawableImageParameters.PreviewBounds);

            float focusRatioX = (focusX - DrawableImageParameters.PreviewBounds.Left) / DrawableImageParameters.PreviewBounds.Width();
            float focusRatioY = (focusY - DrawableImageParameters.PreviewBounds.Top) / DrawableImageParameters.PreviewBounds.Height();

            DrawableImageParameters.Scale = scale;

            GetDisplayDrawableBounds(DrawableImageParameters.PreviewBounds);

            float scaledFocusX = DrawableImageParameters.PreviewBounds.Left + focusRatioX * DrawableImageParameters.PreviewBounds.Width();
            float scaledFocusY = DrawableImageParameters.PreviewBounds.Top + focusRatioY * DrawableImageParameters.PreviewBounds.Height();

            _displayDrawableLeft += focusX - scaledFocusX;
            _displayDrawableTop += focusY - scaledFocusY;

            UpdateGrid();
            Invalidate();
        }
        public bool OnScaleBegin(ScaleGestureDetector detector) => true;
        public void OnScaleEnd(ScaleGestureDetector detector) { }

        public void OnBitmapFailed(Drawable p0) { }
        public void OnBitmapLoaded(Bitmap p0, Picasso.LoadedFrom p1)
        {
            ResetToDefaults(ScaleType.Square);
            _drawable = new BitmapDrawable(p0);
            _imageRawWidth = p0.Width;
            _imageRawHeight = p0.Height;
            _reloadImage = false;
            RequestLayout();
            Invalidate();
        }
        public void OnPrepareLoad(Drawable p0) { }
    }

    public class ImageParameters
    {
        public ImageParameters()
        {
            PreviewBounds = new RectF();
        }
        public float Scale { get; set; }
        public float Rotation { get; set; }
        public RectF PreviewBounds { get; private set; }
        public Rect CropBounds { get; set; }
        public ImageParameters Copy() => new ImageParameters
        {
            Scale = Scale,
            PreviewBounds = new RectF(PreviewBounds),
            CropBounds = new Rect(CropBounds)
        };
    }
}