using System;
using System.Threading.Tasks;
using Android.Animation;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Newtonsoft.Json;
using Square.Picasso;
using Steepshot.Core.Utils;
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
            Undefined
        }

        #region Fields

        private CancellationSignal _cancellationSignal;

        private const float MinimumRatio = 0.8f;
        private const float MaximumRatio = 1.92f;
        private const float DefaultRatio = 1f;
        private const float MaximumOverscrollMultiplier = 150f;
        private const float MaximumOverscale = 0.2f;

        private CropViewGrid _grid;
        private ImageParameters _drawableImageParameters;

        private GalleryMediaModel _media;
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

        #endregion

        #region MyRegion

        private CropViewGrid Grid => _grid ?? (_grid = new CropViewGrid());
        public ImageParameters DrawableImageParameters
        {
            get
            {
                _drawableImageParameters.CropBounds = new Rect(Grid.Bounds);
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

                if (ImageRatio <= 1)
                {
                    _drawableImageParameters.PreviewBounds.Offset(-Grid.Bounds.Left, 0);
                    _targetWidth = Grid.Bounds.Width();
                }
                else
                {
                    _drawableImageParameters.PreviewBounds.Offset(0, -Grid.Bounds.Top);
                    _targetHeight = Grid.Bounds.Height();
                }
            }
        }
        private void Configure(ScaleType scaleType, ImageParameters parameters, float minRatio, float maxRatio, float defaultRatio)
        {
            _minimumRatio = minRatio;
            _maximumRatio = maxRatio;
            _defaultRatio = defaultRatio;
            _currentScaleType = scaleType;
            if (parameters == null)
            {
                _displayDrawableLeft = 0;
                _displayDrawableTop = 0;
                _drawableImageParameters = new ImageParameters();

                if (_media != null)
                    _drawableImageParameters.Rotation = _media.Orientation;
            }
            else
            {
                _drawableImageParameters = parameters;
                _displayDrawableLeft = parameters.PreviewBounds.Left;
                _displayDrawableTop = parameters.PreviewBounds.Top;
            }
        }
        public bool IsBitmapReady { get; private set; }
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
                else
                {
                    if (ImageRatio < _minimumRatio)
                    {
                        GetBoundsForHeightAndRatio(_height, _minimumRatio, _drawableImageParameters.PreviewBounds);
                        scale = _drawableImageParameters.PreviewBounds.Width() / _imageRawWidth;
                    }
                    else
                    {
                        GetBoundsForWidthAndRatio(_width, _maximumRatio, _drawableImageParameters.PreviewBounds);
                        scale = _drawableImageParameters.PreviewBounds.Height() / _imageRawHeight;
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
        private float StrictRatio => Grid.Bounds.Width() / (float)Grid.Bounds.Height();
        private float DisplayDrawableWidth => _drawableImageParameters.Scale * _imageRawWidth;
        private float DisplayDrawableHeight => _drawableImageParameters.Scale * _imageRawHeight;
        private float MaximumAllowedScale => FitRatioScale * 3f;
        private float MinimumAllowedScale => _useStrictBounds ? FitRatioScale : FitDrawableScale;
        private long _backDuration => 400;

        #endregion

        #region Constructors        
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

        private void Initialize(Context context)
        {
            _gestureDetector = new GestureDetector(Context, this);
            _scaleGestureDetector = new ScaleGestureDetector(context, this);

            Configure(ScaleType.Square, null, MinimumRatio, MaximumRatio, DefaultRatio);
            _maximumOverScroll = Resources.DisplayMetrics.Density * MaximumOverscrollMultiplier;
            _maximumOverScale = MaximumOverscale;

            _gesturesAnimator = new ValueAnimator();
            _gesturesAnimator.SetDuration(_backDuration);
            _gesturesAnimator.SetFloatValues(0, 1);
            _gesturesAnimator.SetInterpolator(new DecelerateInterpolator(0.25F));
            _gesturesAnimator.Update += GesturesAnimatorOnUpdate;

            Grid.SetCallback(this);
        }

        #endregion


        public override void InvalidateDrawable(Drawable drawable)
        {
            Invalidate();
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

        public void SetImage(GalleryMediaModel model)
        {
            _cancellationSignal?.Cancel();
            _cancellationSignal = new CancellationSignal();

            _reloadImage = model != _media;

            if (_reloadImage)
            {
                _media = model;
                _drawable = null;
            }

            var hasParameters = _media.Parameters != null;

            if (_useStrictBounds)
                Configure(hasParameters ? ScaleType.Undefined : ScaleType.Bind, _media.Parameters?.Copy(), StrictRatio, StrictRatio, StrictRatio);
            else
                Configure(hasParameters ? _currentScaleType : ScaleType.Square, null, MinimumRatio, MaximumRatio, DefaultRatio);

            RequestLayout();
            Invalidate();
        }

        public void SetImageBitmap(GalleryMediaModel model)
        {
            _reloadImage = false;
            _drawable = Drawable.CreateFromPath(model.TempPath);

            var options = new BitmapFactory.Options { InJustDecodeBounds = true };
            BitmapFactory.DecodeFile(model.TempPath, options);

            _imageRawWidth = options.OutWidth;
            _imageRawHeight = options.OutHeight;

            RequestLayout();
            Invalidate();
        }

        public void SetImageBitmap(Bitmap bitmap)
        {
            _reloadImage = false;
            _drawable = new BitmapDrawable(bitmap);

            _imageRawWidth = bitmap.Width;
            _imageRawHeight = bitmap.Height;

            RequestLayout();
            Invalidate();
        }

        public void Rotate(int angle)
        {
            IsBitmapReady = false;
            _drawableImageParameters.Rotation = angle % 360;
            _currentScaleType = _useStrictBounds ? ScaleType.Bind : _currentScaleType;
            _reloadImage = true;
            RequestLayout();
        }

        public void SwitchScale()
        {
            Reset(_currentScaleType == ScaleType.Square || _currentScaleType == ScaleType.Undefined ? ScaleType.Ratio : ScaleType.Square);
        }

        public bool OnDown(MotionEvent e) => true;

        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY) => false;

        public void OnLongPress(MotionEvent e) { }

        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            distanceX = -distanceX;
            distanceY = -distanceY;

            GetDisplayDrawableBounds(_drawableImageParameters.PreviewBounds);

            float overScrollX = MeasureOverScrollX(_drawableImageParameters.PreviewBounds);
            float overScrollY = MeasureOverScrollY(_drawableImageParameters.PreviewBounds);

            distanceX = ApplyOverScrollFix(distanceX, overScrollX);
            distanceY = ApplyOverScrollFix(distanceY, overScrollY);

            _displayDrawableLeft += distanceX;
            _displayDrawableTop += distanceY;

            UpdateGrid();
            Invalidate();

            return true;
        }

        public void OnShowPress(MotionEvent e) { }

        public bool OnSingleTapUp(MotionEvent e) => false;

        public bool OnScale(ScaleGestureDetector detector)
        {
            float overScale = MeasureOverScale();
            float scale = ApplyOverScaleFix(detector.ScaleFactor, overScale);

            _focusedScaleX = detector.FocusX;
            _focusedScaleY = detector.FocusY;

            SetScaleKeepingFocus(_drawableImageParameters.Scale * scale, _focusedScaleX, _focusedScaleY);

            return true;
        }

        public bool OnScaleBegin(ScaleGestureDetector detector) => true;

        public void OnScaleEnd(ScaleGestureDetector detector) { }

        public void OnBitmapFailed(Drawable p0) { }

        public void OnBitmapLoaded(Bitmap p0, Picasso.LoadedFrom p1)
        {
            SetImageBitmap(p0);
        }

        public void OnPrepareLoad(Drawable p0) { }

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
            {
                IsBitmapReady = false;

                var drawableRequest = await MakeDrawable(_cancellationSignal, BitmapUtils.MaxImageSize, BitmapUtils.MaxImageSize, _drawableImageParameters.Rotation);

                if (drawableRequest == null)
                {
                    RequestLayout();
                    Invalidate();
                    return;
                }

                _drawable = drawableRequest;
            }

            Reset(_currentScaleType);
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            if (_drawable == null)
                return;

            var path = new Path();
            path.AddRoundRect(new RectF(0, 0, canvas.Width, canvas.Height), CornerRadius, CornerRadius, Path.Direction.Cw);
            canvas.ClipPath(path);

            GetDisplayDrawableBounds(_drawableImageParameters.PreviewBounds);

            _drawable.SetBounds((int)_drawableImageParameters.PreviewBounds.Left, (int)_drawableImageParameters.PreviewBounds.Top, (int)_drawableImageParameters.PreviewBounds.Right, (int)_drawableImageParameters.PreviewBounds.Bottom);
            _drawable.Draw(canvas);

            Grid.Draw(canvas);
        }

        private Task<BitmapDrawable> MakeDrawable(CancellationSignal signal, int targetWidth, int targetHeight, float angle = 0)
        {
            return Task.Run(() =>
            {
                try
                {
                    if (Looper.MyLooper() == null)
                        Looper.Prepare();

                    using (var bitmap = BitmapUtils.DecodeSampledBitmapFromFile(Context, Uri.Parse(_media.Path), targetWidth, targetHeight))
                    {
                        var matrix = new Matrix();
                        matrix.PostRotate(angle);
                        var preparedBitmap = Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, true);
                        _imageRawWidth = preparedBitmap.Width;
                        _imageRawHeight = preparedBitmap.Height;

                        if (signal.IsCanceled)
                            return null;

                        return new BitmapDrawable(Context.Resources, preparedBitmap);
                    }
                }
                catch (Exception ex)
                {
                    AppSettings.Logger.Warning(ex);
                    return null;
                }
            });
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
                SetScaleKeepingFocus(_drawableImageParameters.Scale, _focusedScaleX, _focusedScaleY);
            UpdateGrid();

            Invalidate();
            IsBitmapReady = true;
        }

        private void SetDrawableScale(float scale)
        {
            _drawableImageParameters.Scale = scale;

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
            GetDisplayDrawableBounds(_drawableImageParameters.PreviewBounds);

            _drawableImageParameters.PreviewBounds.Intersect(0, 0, _width, _height);

            _drawableImageParameters.PreviewBounds.Set(_drawableImageParameters.PreviewBounds.Left, _drawableImageParameters.PreviewBounds.Top, _drawableImageParameters.PreviewBounds.Left + _drawableImageParameters.PreviewBounds.Width(), _drawableImageParameters.PreviewBounds.Top + _drawableImageParameters.PreviewBounds.Height());
            SetGridBounds(_drawableImageParameters.PreviewBounds);

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

        private void GesturesAnimatorOnUpdate(object sender, ValueAnimator.AnimatorUpdateEventArgs animatorUpdateEventArgs)
        {
            float value = (float)animatorUpdateEventArgs.Animation.AnimatedValue;
            GetDisplayDrawableBounds(_drawableImageParameters.PreviewBounds);

            float overScrollX = MeasureOverScrollX(_drawableImageParameters.PreviewBounds);
            float overScrollY = MeasureOverScrollY(_drawableImageParameters.PreviewBounds);
            float overScale = MeasureOverScale();

            _displayDrawableLeft -= overScrollX * value;
            _displayDrawableTop -= overScrollY * value;

            float targetScale = _drawableImageParameters.Scale / overScale;
            float newScale = (1 - value) * _drawableImageParameters.Scale + value * targetScale;

            SetScaleKeepingFocus(newScale, _focusedScaleX, _focusedScaleY);

            UpdateGrid();
            Invalidate();
        }

        private float MeasureOverScrollX(RectF displayDrawableBounds)
        {
            var drawableIsSmallerThanView = displayDrawableBounds.Width() <= _width;

            if (drawableIsSmallerThanView)
                return displayDrawableBounds.CenterX() - _width / 2f;

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
                return displayDrawableBounds.CenterY() - _height / 2f;

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

        private float MeasureOverScale()
        {
            float maximumAllowedScale = MaximumAllowedScale;
            float minimumAllowedScale = MinimumAllowedScale;

            if (maximumAllowedScale < minimumAllowedScale)
                maximumAllowedScale = minimumAllowedScale;

            if (_drawableImageParameters.Scale < minimumAllowedScale)
                return _drawableImageParameters.Scale / minimumAllowedScale;
            if (_drawableImageParameters.Scale > maximumAllowedScale)
                return _drawableImageParameters.Scale / maximumAllowedScale;
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
            GetDisplayDrawableBounds(_drawableImageParameters.PreviewBounds);

            float focusRatioX = (focusX - _drawableImageParameters.PreviewBounds.Left) / _drawableImageParameters.PreviewBounds.Width();
            float focusRatioY = (focusY - _drawableImageParameters.PreviewBounds.Top) / _drawableImageParameters.PreviewBounds.Height();

            _drawableImageParameters.Scale = scale;

            GetDisplayDrawableBounds(_drawableImageParameters.PreviewBounds);

            float scaledFocusX = _drawableImageParameters.PreviewBounds.Left + focusRatioX * _drawableImageParameters.PreviewBounds.Width();
            float scaledFocusY = _drawableImageParameters.PreviewBounds.Top + focusRatioY * _drawableImageParameters.PreviewBounds.Height();

            _displayDrawableLeft += focusX - scaledFocusX;
            _displayDrawableTop += focusY - scaledFocusY;

            UpdateGrid();
            Invalidate();
        }
    }

    public class ImageParameters
    {
        public float Scale { get; set; }

        public int Rotation { get; set; }

        [JsonConverter(typeof(RectFConverter))]
        public RectF PreviewBounds { get; private set; }

        [JsonConverter(typeof(RectFConverter))]
        public Rect CropBounds { get; set; }

        public ImageParameters()
        {
            PreviewBounds = new RectF();
        }

        public ImageParameters Copy() => new ImageParameters
        {
            Scale = Scale,
            Rotation = Rotation,
            PreviewBounds = new RectF(PreviewBounds),
            CropBounds = new Rect(CropBounds)
        };
    }

    public class RectFConverter : JsonConverter
    {
        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            
            if (value is RectF)
            {
                var typed = (RectF)value;
                writer.WritePropertyName(nameof(RectF.Left));
                writer.WriteValue(typed.Left);
                writer.WritePropertyName(nameof(RectF.Top));
                writer.WriteValue(typed.Top);
                writer.WritePropertyName(nameof(RectF.Right));
                writer.WriteValue(typed.Right);
                writer.WritePropertyName(nameof(RectF.Bottom));
                writer.WriteValue(typed.Bottom);
            }
            else
            {
                var typed = (Rect)value;
                writer.WritePropertyName(nameof(Rect.Left));
                writer.WriteValue(typed.Left);
                writer.WritePropertyName(nameof(Rect.Top));
                writer.WriteValue(typed.Top);
                writer.WritePropertyName(nameof(Rect.Right));
                writer.WriteValue(typed.Right);
                writer.WritePropertyName(nameof(Rect.Bottom));
                writer.WriteValue(typed.Bottom);
            }

            writer.WriteEndObject();
        }

        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            reader.Read();
            var left = reader.ReadAsDouble();
            reader.Read();
            var top = reader.ReadAsDouble();
            reader.Read();
            var right = reader.ReadAsDouble();
            reader.Read();
            var bottom = reader.ReadAsDouble();
            reader.Read();

            if (objectType == typeof(RectF))
                return new RectF((float)left, (float)top, (float)right, (float)bottom);

            return new Rect((int)left, (int)top, (int)right, (int)bottom);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(RectF) || objectType == typeof(Rect);
        }
    }
}