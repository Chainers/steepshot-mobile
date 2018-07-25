using System;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Java.Lang;
using Math = System.Math;
using Steepshot.Core.Localization;
using Steepshot.Core.Errors;
using Steepshot.Core.Utils;

namespace Steepshot.CustomViews
{
    public sealed class ScaleImageView : ImageView
    {
        public enum TouchState { None, Drag, Zoom, Fling, AnimateZoom, Click };

        //static string DEBUG = "DEBUG";

        //
        // SuperMin and SuperMax multipliers. Determine how much the image can be
        // zoomed below or above the zoom boundaries, before animating back to the
        // min/max zoom boundary.
        //
        private static readonly float SuperMinMultiplier = 0.5f;
        private static readonly float SuperMaxMultiplier = 1f;

        //
        // Scale of image ranges from minScale to maxScale, where minScale == 1
        // when the image is stretched to fit view.
        //
        private float _normalizedScale;

        //
        // Matrix applied to image. MSCALE_X and MSCALE_Y should always be equal.
        // MTRANS_X and MTRANS_Y are the other values used. prevMatrix is the matrix
        // saved prior to the screen rotating.
        //
        private Matrix _matrix;

        private Matrix _prevMatrix;

        //
        // Size of view and previous view size (ie before rotation)
        //
        public int ViewWidth, ViewHeight, PrevViewWidth, PrevViewHeight;

        private ScaleImageViewListener _listener;

        private TouchState _state;

        private float _minScale;
        private float _maxScale;
        private float _superMinScale;
        private float _superMaxScale;
        private float[] _m;

        private Context _context;
        private Fling _fling;

        private ScaleType _mScaleType;

        private bool _imageRenderedAtLeastOnce;
        private bool _onDrawReady;

        private ZoomVariables _delayedZoomVariables;

        //
        // Size of image when it is stretched to fit view. Before and After rotation.
        //
        private float _matchViewWidth, _matchViewHeight, _prevMatchViewWidth, _prevMatchViewHeight;

        //
        // After setting image, a value of true means the new image should maintain
        // the zoom of the previous image. False means it should be resized within the view.
        //
        //bool maintainZoomAfterSetImage;

        //
        // True when maintainZoomAfterSetImage has been set to true and setImage has been called.
        //
        //bool setImageCalledRecenterImage;

        private ScaleGestureDetector _scaleDetector;
        private GestureDetector _gestureDetector;
        private GestureDetector.IOnDoubleTapListener _doubleTapListener;
        private IOnTouchListener _touchListener;
        private const float Tolerance = 0.0000000001f;

        public ScaleImageView(Context context) : base(context)
        {
            SharedConstructing(context);
        }

        public ScaleImageView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            SharedConstructing(context);
        }

        public ScaleImageView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            SharedConstructing(context);
        }

        public ScaleImageView(IntPtr inptr, JniHandleOwnership hadle) : base(inptr, hadle) { }

        private void SharedConstructing(Context context)
        {
            Clickable = true;
            _context = context;
            _scaleDetector = new ScaleGestureDetector(context, new ScaleListener(this));
            _gestureDetector = new GestureDetector(context, new GestureListener(this));
            _matrix = new Matrix();
            _prevMatrix = new Matrix();
            _m = new float[9];
            _normalizedScale = 1;
            if (_mScaleType == null)
            {
                _mScaleType = ScaleType.CenterInside;
            }
            _minScale = 1;
            _maxScale = 4;
            _superMinScale = SuperMinMultiplier * _minScale;
            _superMaxScale = SuperMaxMultiplier * _maxScale;
            ImageMatrix = _matrix;
            SetScaleType(ScaleType.Matrix);
            SetState(TouchState.None);
            //МЕНЯЛ
            _listener = new ScaleImageViewListener(this);

            base.SetOnTouchListener(_listener);
        }

        public bool CanScrollHorizontallyFroyo(int direction)
        {
            return CanScrollHorizontally(direction);
        }

        public override void SetOnTouchListener(IOnTouchListener l)
        {
            _touchListener = l;
        }

        public void SetOnDoubleTapListener(GestureDetector.IOnDoubleTapListener l)
        {
            _doubleTapListener = l;
        }

        public override void SetImageResource(int resId)
        {
            base.SetImageResource(resId);
            SavePreviousImageValues();
            FitImageToView();
        }

        public override void SetImageBitmap(Bitmap bm)
        {
            base.SetImageBitmap(bm);
            SavePreviousImageValues();
            FitImageToView();
        }

        public override void SetImageDrawable(Drawable drawable)
        {
            base.SetImageDrawable(drawable);
            SavePreviousImageValues();
            FitImageToView();
        }

        public override void SetImageURI(Android.Net.Uri uri)
        {
            base.SetImageURI(uri);
            SavePreviousImageValues();
            FitImageToView();
        }

        public override void SetScaleType(ScaleType scaleType)
        {
            if (scaleType == ScaleType.FitStart || scaleType == ScaleType.FitEnd)
            {
                throw new ValidateException(LocalizationKeys.ErrorCameraScale);
            }
            if (scaleType == ScaleType.Matrix)
            {
                base.SetScaleType(ScaleType.Matrix);
            }
            else
            {
                _mScaleType = scaleType;
            }
        }

        public override ScaleType GetScaleType()
        {
            return _mScaleType;
        }

        /*void SetImageCalled() {
			if (!maintainZoomAfterSetImage) {
				setImageCalledRecenterImage = true;
			}
		}*/

        //
        // Returns false if image is in initial, unzoomed state. False, otherwise.
        // @return true if image is zoomed
        //
        public bool IsZoomed()
        {
            return Math.Abs(_normalizedScale - 1f) > Tolerance;
        }

        //
        // Return a Rect representing the zoomed image.
        // @return rect representing zoomed image
        //
        public RectF GetZoomedRect()
        {
            if (_mScaleType == ScaleType.FitXy)
            {
                throw new ValidateException(LocalizationKeys.ErrorCameraZoom);
            }
            var topLeft = TransformCoordTouchToBitmap(0, 0, true);
            var bottomRight = TransformCoordTouchToBitmap(ViewWidth, ViewHeight, true);
            float w = Drawable.IntrinsicWidth;
            float h = Drawable.IntrinsicHeight;
            return new RectF(topLeft.X / w, topLeft.Y / h, bottomRight.X / w, bottomRight.Y / h);
        }

        //
        // Save the current matrix and view dimensions
        // in the prevMatrix and prevView variables.
        //
        private void SavePreviousImageValues()
        {
            if (_matrix != null && ViewHeight != 0 && ViewWidth != 0)
            {
                _matrix.GetValues(_m);
                _prevMatrix.SetValues(_m);
                _prevMatchViewHeight = _matchViewHeight;
                _prevMatchViewWidth = _matchViewWidth;
                PrevViewHeight = ViewHeight;
                PrevViewWidth = ViewWidth;
            }
        }

        protected override IParcelable OnSaveInstanceState()
        {
            var bundle = new Bundle();
            bundle.PutParcelable("instanceState", base.OnSaveInstanceState());
            bundle.PutFloat("saveScale", _normalizedScale);
            bundle.PutFloat("matchViewHeight", _matchViewHeight);
            bundle.PutFloat("matchViewWidth", _matchViewWidth);
            bundle.PutInt("viewWidth", ViewWidth);
            bundle.PutInt("viewHeight", ViewHeight);
            _matrix.GetValues(_m);
            bundle.PutFloatArray("matrix", _m);
            bundle.PutBoolean("imageRendered", _imageRenderedAtLeastOnce);
            return bundle;
        }

        protected override void OnRestoreInstanceState(IParcelable state)
        {
            if (state is Bundle)
            {
                var bundle = (Bundle)state;
                _normalizedScale = bundle.GetFloat("saveScale");
                _m = bundle.GetFloatArray("matrix");
                _prevMatrix.SetValues(_m);
                _prevMatchViewHeight = bundle.GetFloat("matchViewHeight");
                _prevMatchViewWidth = bundle.GetFloat("matchViewWidth");
                PrevViewHeight = bundle.GetInt("viewHeight");
                PrevViewWidth = bundle.GetInt("viewWidth");
                _imageRenderedAtLeastOnce = bundle.GetBoolean("imageRendered");
                base.OnRestoreInstanceState((IParcelable)bundle.GetParcelable("instanceState"));
                return;
            }
            base.OnRestoreInstanceState(state);
        }

        protected override void OnDraw(Canvas canvas)
        {
            try
            {
                _onDrawReady = true;
                _imageRenderedAtLeastOnce = true;
                if (_delayedZoomVariables != null)
                {
                    SetZoom(_delayedZoomVariables.Scale, _delayedZoomVariables.FocusX, _delayedZoomVariables.FocusY, _delayedZoomVariables.ScaleType);
                    _delayedZoomVariables = null;
                }

                base.OnDraw(canvas);

                DrawReady?.Invoke(null, EventArgs.Empty);
            }
            catch (System.Exception ex)
            {
                AppSettings.Logger.Warning(ex);
            }
        }

        protected override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            SavePreviousImageValues();
        }

        //
        // Get the max zoom multiplier.
        // @return max zoom multiplier.
        //
        public float GetMaxZoom()
        {
            return _maxScale;
        }

        //
        // Set the max zoom multiplier. Default value: 3.
        // @param max max zoom multiplier.
        //
        public void SetMaxZoom(float max)
        {
            _maxScale = max;
            _superMaxScale = SuperMaxMultiplier * _maxScale;
        }

        //
        // Get the min zoom multiplier.
        // @return min zoom multiplier.
        //
        public float GetMinZoom()
        {
            return _minScale;
        }

        //
        // Get the current zoom. This is the zoom relative to the initial
        // scale, not the original resource.
        // @return current zoom multiplier.
        //
        public float GetCurrentZoom()
        {
            return _normalizedScale;
        }

        //
        // Set the min zoom multiplier. Default value: 1.
        // @param min min zoom multiplier.
        //
        public void SetMinZoom(float min)
        {
            _minScale = min;
            _superMinScale = SuperMinMultiplier * _minScale;
        }

        //
        // Reset zoom and translation to initial state.
        //
        public void ResetZoom()
        {
            _normalizedScale = 1;
            FitImageToView();
        }

        //
        // Set zoom to the specified scale. Image will be centered by default.
        // @param scale
        //
        public void SetZoom(float scale)
        {
            SetZoom(scale, 0.5f, 0.5f);
        }

        //
        // Set zoom to the specified scale. Image will be centered around the point
        // (focusX, focusY). These floats range from 0 to 1 and denote the focus point
        // as a fraction from the left and top of the view. For example, the top left 
        // corner of the image would be (0, 0). And the bottom right corner would be (1, 1).
        // @param scale
        // @param focusX
        // @param focusY
        //
        public void SetZoom(float scale, float focusX, float focusY)
        {
            SetZoom(scale, focusX, focusY, _mScaleType);
        }

        #region SetZoom
        public event EventHandler DrawReady;

        //
        // Set zoom to the specified scale. Image will be centered around the point
        // (focusX, focusY). These floats range from 0 to 1 and denote the focus point
        // as a fraction from the left and top of the view. For example, the top left 
        // corner of the image would be (0, 0). And the bottom right corner would be (1, 1).
        // @param scale
        // @param focusX
        // @param focusY
        // @param scaleType
        //
        public void SetZoom(float scale, float focusX, float focusY, ScaleType scaleType)
        {
            //
            // setZoom can be called before the image is on the screen, but at this point, 
            // image and view sizes have not yet been calculated in onMeasure. Thus, we should
            // delay calling setZoom until the view has been measured.
            //
            if (!_onDrawReady)
            {
                _delayedZoomVariables = new ZoomVariables(scale, focusX, focusY, scaleType);
                return;
            }
            SetScaleType(scaleType);
            ResetZoom();
            ScaleImage(scale, ViewWidth / 2f, ViewHeight / 2f, false);
            _matrix.GetValues(_m);
            _m[Matrix.MtransX] = -((focusX * GetImageWidth()) - (ViewWidth * 0.5f));
            _m[Matrix.MtransY] = -((focusY * GetImageHeight()) - (ViewHeight * 0.5f));
            _matrix.SetValues(_m);
            FixTrans();
            ImageMatrix = _matrix;
        }
        #endregion

        //
        // Set zoom parameters equal to another ScaleImageView. Including scale, position,
        // and ScaleType.
        // @param ScaleImageView
        //
        public void SetZoom(ScaleImageView img)
        {
            var center = img.GetScrollPosition();
            SetZoom(img.GetCurrentZoom(), center.X, center.Y, img.GetScaleType());
        }

        //
        // Return the point at the center of the zoomed image. The PointF coordinates range
        // in value between 0 and 1 and the focus point is denoted as a fraction from the left 
        // and top of the view. For example, the top left corner of the image would be (0, 0). 
        // And the bottom right corner would be (1, 1).
        // @return PointF representing the scroll position of the zoomed image.
        //
        public PointF GetScrollPosition()
        {
            var drawable = Drawable;
            if (drawable == null)
            {
                return null;
            }
            var drawableWidth = drawable.IntrinsicWidth;
            var drawableHeight = drawable.IntrinsicHeight;
            var point = TransformCoordTouchToBitmap(ViewWidth / 2f, ViewHeight / 2f, true);
            point.X /= drawableWidth;
            point.Y /= drawableHeight;
            return point;
        }

        //
        // Set the focus point of the zoomed image. The focus points are denoted as a fraction from the
        // left and top of the view. The focus points can range in value between 0 and 1. 
        // @param focusX
        // @param focusY
        //
        public void SetScrollPosition(float focusX, float focusY)
        {
            SetZoom(_normalizedScale, focusX, focusY);
        }

        //
        // Performs boundary checking and fixes the image matrix if it 
        // is out of bounds.
        //
        public void FixTrans()
        {
            _matrix.GetValues(_m);
            var transX = _m[Matrix.MtransX];
            var transY = _m[Matrix.MtransY];
            var fixTransX = GetFixTrans(transX, ViewWidth, GetImageWidth());
            var fixTransY = GetFixTrans(transY, ViewHeight, GetImageHeight());
            if (Math.Abs(fixTransX) > Tolerance || Math.Abs(fixTransY) > Tolerance)
            {
                _matrix.PostTranslate(fixTransX, fixTransY);
            }
        }

        //
        // When transitioning from zooming from focus to zoom from center (or vice versa)
        // the image can become unaligned within the view. This is apparent when zooming
        // quickly. When the content size is less than the view size, the content will often
        // be centered incorrectly within the view. fixScaleTrans first calls fixTrans() and 
        // then makes sure the image is centered correctly within the view.
        //
        private void FixScaleTrans()
        {
            FixTrans();
            _matrix.GetValues(_m);
            if (GetImageWidth() < ViewWidth)
            {
                _m[Matrix.MtransX] = (ViewWidth - GetImageWidth()) / 2;
            }
            if (GetImageHeight() < ViewHeight)
            {
                _m[Matrix.MtransY] = (ViewHeight - GetImageHeight()) / 2;
            }
            _matrix.SetValues(_m);
        }

        private float GetFixTrans(float trans, float viewSize, float contentSize)
        {
            float minTrans, maxTrans;

            if (contentSize <= viewSize)
            {
                minTrans = 0;
                maxTrans = viewSize - contentSize;
            }
            else
            {
                minTrans = viewSize - contentSize;
                maxTrans = 0;
            }
            if (trans < minTrans)
            {
                return -trans + minTrans;
            }
            if (trans > maxTrans)
            {
                return -trans + maxTrans;
            }
            return 0;
        }

        public float GetFixDragTrans(float delta, float viewSize, float contentSize)
        {
            if (contentSize <= viewSize)
            {
                return 0;
            }
            return delta;
        }

        public float GetImageWidth()
        {
            return _matchViewWidth * _normalizedScale;
        }

        public float GetImageHeight()
        {
            return _matchViewHeight * _normalizedScale;
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            var drawable = Drawable;
            if (drawable == null || drawable.IntrinsicWidth == 0 || drawable.IntrinsicHeight == 0)
            {
                SetMeasuredDimension(0, 0);
                return;
            }
            var drawableWidth = drawable.IntrinsicWidth;
            var drawableHeight = drawable.IntrinsicHeight;
            var widthSize = MeasureSpec.GetSize(widthMeasureSpec);
            var widthMode = (int)MeasureSpec.GetMode(widthMeasureSpec);
            var heightSize = MeasureSpec.GetSize(heightMeasureSpec);
            var heightMode = (int)MeasureSpec.GetMode(heightMeasureSpec);
            ViewWidth = SetViewSize(widthMode, widthSize, drawableWidth);
            ViewHeight = SetViewSize(heightMode, heightSize, drawableHeight);

            //
            // Set view dimensions
            //
            SetMeasuredDimension(ViewWidth, ViewHeight);

            //
            // Fit content within view
            //
            FitImageToView();
        }

        //
        // If the normalizedScale is equal to 1, then the image is made to fit the screen. Otherwise,
        // it is made to fit the screen according to the dimensions of the previous image matrix. This
        // allows the image to maintain its zoom after rotation.
        //
        private void FitImageToView()
        {
            var drawable = Drawable;
            if (drawable == null || drawable.IntrinsicWidth == 0 || drawable.IntrinsicHeight == 0)
            {
                return;
            }
            if (_matrix == null || _prevMatrix == null)
            {
                return;
            }
            var drawableWidth = drawable.IntrinsicWidth;
            var drawableHeight = drawable.IntrinsicHeight;

            //
            // Scale image for view
            //
            var scaleX = (float)ViewWidth / drawableWidth;
            var scaleY = (float)ViewHeight / drawableHeight;

            if (_mScaleType == ScaleType.Center)
            {
                scaleX = scaleY = 1f;
            }
            else if (_mScaleType == ScaleType.CenterCrop)
            {
                scaleX = scaleY = Math.Max(scaleX, scaleY);
            }
            else if (_mScaleType == ScaleType.CenterInside)
            {
                scaleX = scaleY = Math.Min(1f, Math.Min(scaleX, scaleY));
            }
            else if (_mScaleType == ScaleType.FitCenter)
            {
                scaleX = scaleY = Math.Min(scaleX, scaleY);
            }
            else if (_mScaleType == ScaleType.FitXy)
            {
            }
            else
            {
                throw new ValidateException(LocalizationKeys.ErrorCameraScale);
            }

            //
            // Center the image
            //
            var redundantYSpace = ViewHeight - (scaleX * drawableHeight);
            var redundantXSpace = ViewWidth - (scaleY * drawableWidth);
            _matchViewWidth = ViewWidth - redundantXSpace;
            _matchViewHeight = ViewHeight - redundantYSpace;
            if (!IsZoomed() && !_imageRenderedAtLeastOnce)
            {
                //
                // Stretch and center image to fit view
                //
                _matrix.SetScale(scaleX, scaleY);
                _matrix.PostTranslate(redundantXSpace / 2, redundantYSpace / 2);
                _normalizedScale = 1;
            }
            else
            {
                if (Math.Abs(_prevMatchViewWidth) < Tolerance || Math.Abs(_prevMatchViewHeight) < Tolerance)
                {
                    SavePreviousImageValues();
                }
                _prevMatrix.GetValues(_m);

                //
                // Rescale Matrix after rotation
                //
                _m[Matrix.MscaleX] = _matchViewWidth / drawableWidth * _normalizedScale;
                _m[Matrix.MscaleY] = _matchViewHeight / drawableHeight * _normalizedScale;

                //
                // TransX and TransY from previous matrix
                //
                var transX = _m[Matrix.MtransX];
                var transY = _m[Matrix.MtransY];

                //
                // Width
                //
                var prevActualWidth = _prevMatchViewWidth * _normalizedScale;
                var actualWidth = GetImageWidth();
                TranslateMatrixAfterRotate(Matrix.MtransX, transX, prevActualWidth, actualWidth, PrevViewWidth, ViewWidth, drawableWidth);

                //
                // Height
                //
                var prevActualHeight = _prevMatchViewHeight * _normalizedScale;
                var actualHeight = GetImageHeight();
                TranslateMatrixAfterRotate(Matrix.MtransY, transY, prevActualHeight, actualHeight, PrevViewHeight, ViewHeight, drawableHeight);

                //
                // Set the matrix to the adjusted scale and translate values.
                //
                _matrix.SetValues(_m);
            }
            FixTrans();
            ImageMatrix = _matrix;
        }

        //
        // Set view dimensions based on layout params
        // @param mode 
        // @param size
        // @param drawableWidth
        // @return
        //
        private int SetViewSize(int mode, int size, int drawableWidth)
        {
            switch ((MeasureSpecMode)mode)
            {
                case MeasureSpecMode.Exactly:
                    return size;
                case MeasureSpecMode.AtMost:
                    return Math.Min(drawableWidth, size);
                case MeasureSpecMode.Unspecified:
                    return drawableWidth;
            }
            return size;
        }

        //
        // After rotating, the matrix needs to be translated. This function finds the area of image 
        // which was previously centered and adjusts translations so that is again the center, post-rotation.
        // @param axis Matrix.MTRANS_X or Matrix.MTRANS_Y
        // @param trans the value of trans in that axis before the rotation
        // @param prevImageSize the width/height of the image before the rotation
        // @param imageSize width/height of the image after rotation
        // @param prevViewSize width/height of view before rotation
        // @param viewSize width/height of view after rotation
        // @param drawableSize width/height of drawable
        //
        private void TranslateMatrixAfterRotate(int axis, float trans, float prevImageSize, float imageSize, int prevViewSize, int viewSize, int drawableSize)
        {
            if (imageSize < viewSize)
            {
                //
                // The width/height of image is less than the view's width/height. Center it.
                //
                _m[axis] = (viewSize - (drawableSize * _m[Matrix.MscaleX])) * 0.5f;
            }
            else if (trans > 0)
            {
                //
                // The image is larger than the view, but was not before rotation. Center it.
                //
                _m[axis] = -((imageSize - viewSize) * 0.5f);
            }
            else
            {
                //
                // Find the area of the image which was previously centered in the view. Determine its distance
                // from the left/top side of the view as a fraction of the entire image's width/height. Use that percentage
                // to calculate the trans in the new view width/height.
                //
                var percentage = (Math.Abs(trans) + (0.5f * prevViewSize)) / prevImageSize;
                _m[axis] = -((percentage * imageSize) - (viewSize * 0.5f));
            }
        }

        private void SetState(TouchState state)
        {
            _state = state;
        }

        //
        // Gesture Listener detects a single click or long click and passes that on
        // to the view's listener.
        // @author Ortiz
        //
        private class GestureListener : GestureDetector.SimpleOnGestureListener
        {
            readonly ScaleImageView _view;

            public GestureListener(ScaleImageView view)
            {
                _view = view;
            }

            public override bool OnSingleTapConfirmed(MotionEvent e)
            {
                if (_view._doubleTapListener != null)
                {
                    return _view._doubleTapListener.OnSingleTapConfirmed(e);
                }
                return _view.PerformClick();
            }

            //public override bool OnSingleTapUp (MotionEvent e)
            //{
            //	return true;
            //}

            public override void OnLongPress(MotionEvent e)
            {
                _view.PerformLongClick();
            }

            public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
            {
                //
                // If a previous fling is still active, it should be cancelled so that two flings
                // are not run simultaenously.
                //
                _view._fling?.CancelFling();
                _view._fling = new Fling(_view, (int)velocityX, (int)velocityY);
                _view.CompatPostOnAnimation(_view._fling);
                return base.OnFling(e1, e2, velocityX, velocityY);
            }

            public override bool OnDoubleTap(MotionEvent e)
            {
                var consumed = false;
                if (_view._doubleTapListener != null)
                {
                    consumed = _view._doubleTapListener.OnDoubleTap(e);
                }
                if (_view._state == TouchState.None)
                {
                    var targetZoom = (Math.Abs(_view._normalizedScale - _view._minScale) < Tolerance) ? _view._maxScale : _view._minScale;
                    var doubleTap = new DoubleTapZoom(_view, targetZoom, e.GetX(), e.GetY(), false);
                    _view.CompatPostOnAnimation(doubleTap);
                    consumed = true;
                }
                return consumed;
            }

            public override bool OnDoubleTapEvent(MotionEvent e)
            {
                if (_view._doubleTapListener != null)
                {
                    return _view._doubleTapListener.OnDoubleTapEvent(e);
                }
                return false;
            }
        }

        //
        // Responsible for all touch events. Handles the heavy lifting of drag and also sends
        // touch events to Scale Detector and Gesture Detector.
        // @author Ortiz
        //
        private class ScaleImageViewListener : Java.Lang.Object, IOnTouchListener
        {
            //
            // Remember last point position for dragging
            //
            float _lastX, _lastY;

            readonly ScaleImageView _view;

            public ScaleImageViewListener(ScaleImageView view)
            {
                _view = view;
            }

            public bool OnTouch(View v, MotionEvent evt)
            {
                _view._touchListener?.OnTouch(v, evt); // User-defined handler, maybe
                _view._scaleDetector?.OnTouchEvent(evt);
                _view._gestureDetector?.OnTouchEvent(evt);
                var currentX = evt.GetX();
                var currentY = evt.GetY();

                if (_view._state == TouchState.None || _view._state == TouchState.Drag || _view._state == TouchState.Fling)
                {
                    switch (evt.Action)
                    {
                        case MotionEventActions.Down:
                            _lastX = currentX;
                            _lastY = currentY;
                            _view._fling?.CancelFling();
                            _view.SetState(TouchState.Drag);
                            break;
                        case MotionEventActions.Move:
                            if (_view._state == TouchState.Drag)
                            {
                                var deltaX = currentX - _lastX;
                                var deltaY = currentY - _lastY;
                                var fixTransX = _view.GetFixDragTrans(deltaX, _view.ViewWidth, _view.GetImageWidth());
                                var fixTransY = _view.GetFixDragTrans(deltaY, _view.ViewHeight, _view.GetImageHeight());
                                _view._matrix.PostTranslate(fixTransX, fixTransY);
                                _view.FixTrans();
                                _lastX = currentX;
                                _lastY = currentY;
                            }
                            break;
                        case MotionEventActions.Up:
                        case MotionEventActions.PointerUp:
                            _view.SetState(TouchState.None);
                            break;
                    }
                }
                _view.ImageMatrix = _view._matrix;
                //
                // indicate event was handled
                //
                return true;
            }
        }

        //
        // ScaleListener detects user two finger scaling and scales image.
        // @author Ortiz
        //
        private class ScaleListener : ScaleGestureDetector.SimpleOnScaleGestureListener
        {
            readonly ScaleImageView _view;

            public ScaleListener(ScaleImageView view)
            {
                _view = view;
            }

            public override bool OnScaleBegin(ScaleGestureDetector detector)
            {
                _view.SetState(TouchState.Zoom);
                return true;
            }

            public override bool OnScale(ScaleGestureDetector detector)
            {
                _view.ScaleImage(detector.ScaleFactor, detector.FocusX, detector.FocusY, true);
                return true;
            }

            public override void OnScaleEnd(ScaleGestureDetector detector)
            {
                base.OnScaleEnd(detector);
                _view.SetState(TouchState.None);
                var animateToZoomBoundary = false;
                var targetZoom = _view._normalizedScale;
                if (_view._normalizedScale > _view._maxScale)
                {
                    targetZoom = _view._maxScale;
                    animateToZoomBoundary = true;
                }
                else if (_view._normalizedScale < _view._minScale)
                {
                    targetZoom = _view._minScale;
                    animateToZoomBoundary = true;
                }
                if (animateToZoomBoundary)
                {
                    var doubleTap = new DoubleTapZoom(_view, targetZoom, _view.ViewWidth / 2f, _view.ViewHeight / 2f, true);
                    _view.CompatPostOnAnimation(doubleTap);
                }
            }
        }

        private void ScaleImage(float deltaScale, float focusX, float focusY, bool stretchImageToSuper)
        {
            float lowerScale, upperScale;

            if (stretchImageToSuper)
            {
                lowerScale = _superMinScale;
                upperScale = _superMaxScale;
            }
            else
            {
                lowerScale = _minScale;
                upperScale = _maxScale;
            }
            var origScale = _normalizedScale;
            _normalizedScale *= deltaScale;
            if (_normalizedScale > upperScale)
            {
                _normalizedScale = upperScale;
                deltaScale = upperScale / origScale;
            }
            else if (_normalizedScale < lowerScale)
            {
                _normalizedScale = lowerScale;
                deltaScale = lowerScale / origScale;
            }
            _matrix.PostScale(deltaScale, deltaScale, focusX, focusY);
            FixScaleTrans();
        }

        //
        // DoubleTapZoom calls a series of runnables which apply
        // an animated zoom in/out graphic to the image.
        // @author Ortiz
        //
        private class DoubleTapZoom : Java.Lang.Object, IRunnable
        {
            private static readonly float ZoomTime = 500;

            private readonly long _startTime;
            private readonly float _startZoom;
            private readonly float _targetZoom;
            private readonly float _bitmapX;
            private readonly float _bitmapY;
            private readonly bool _stretchImageToSuper;
            private readonly AccelerateDecelerateInterpolator _interpolator = new AccelerateDecelerateInterpolator();
            private readonly PointF _startTouch;
            private readonly PointF _endTouch;
            private readonly ScaleImageView _view;

            public DoubleTapZoom(ScaleImageView view, float targetZoom, float focusX, float focusY, bool stretchImageToSuper)
            {
                _view = view;
                view.SetState(TouchState.AnimateZoom);
                _startTime = DateTime.Now.Ticks;
                _startZoom = view._normalizedScale;
                _targetZoom = targetZoom;
                _stretchImageToSuper = stretchImageToSuper;
                var bitmapPoint = view.TransformCoordTouchToBitmap(focusX, focusY, false);
                _bitmapX = bitmapPoint.X;
                _bitmapY = bitmapPoint.Y;

                //
                // Used for translating image during scaling
                //
                _startTouch = view.TransformCoordBitmapToTouch(_bitmapX, _bitmapY);
                _endTouch = new PointF(view.ViewWidth / 2f, view.ViewHeight / 2f);
            }

            public void Run()
            {
                var t = Interpolate();
                var deltaScale = CalculateDeltaScale(t);
                _view.ScaleImage(deltaScale, _bitmapX, _bitmapY, _stretchImageToSuper);
                TranslateImageToCenterTouchPosition(t);
                _view.FixScaleTrans();
                _view.ImageMatrix = _view._matrix;
                if (t < 1f)
                {
                    //
                    // We haven't finished zooming
                    //
                    _view.CompatPostOnAnimation(this);
                }
                else
                {
                    //
                    // Finished zooming
                    //
                    _view.SetState(TouchState.None);
                }
            }

            //
            // Interpolate between where the image should start and end in order to translate
            // the image so that the point that is touched is what ends up centered at the end
            // of the zoom.
            // @param t
            //
            void TranslateImageToCenterTouchPosition(float t)
            {
                var targetX = _startTouch.X + t * (_endTouch.X - _startTouch.X);
                var targetY = _startTouch.Y + t * (_endTouch.Y - _startTouch.Y);
                var curr = _view.TransformCoordBitmapToTouch(_bitmapX, _bitmapY);
                _view._matrix.PostTranslate(targetX - curr.X, targetY - curr.Y);
            }

            //
            // Use interpolator to get t
            // @return
            //
            float Interpolate()
            {
                var currTime = DateTime.Now.Ticks;
                var elapsed = (currTime - _startTime) / ZoomTime;
                elapsed = Math.Min(1f, elapsed);
                return _interpolator.GetInterpolation(elapsed);
            }

            //
            // Interpolate the current targeted zoom and get the delta
            // from the current zoom.
            // @param t
            // @return
            //
            float CalculateDeltaScale(float t)
            {
                var zoom = _startZoom + t * (_targetZoom - _startZoom);
                return zoom / _view._normalizedScale;
            }
        }

        //
        // This function will transform the coordinates in the touch event to the coordinate 
        // system of the drawable that the imageview contain
        // @param x x-coordinate of touch event
        // @param y y-coordinate of touch event
        // @param clipToBitmap Touch event may occur within view, but outside image content. True, to clip return value to the bounds of the bitmap size.
        // @return Coordinates of the point touched, in the coordinate system of the original drawable.
        //
        private PointF TransformCoordTouchToBitmap(float x, float y, bool clipToBitmap)
        {
            _matrix.GetValues(_m);
            float origW = Drawable.IntrinsicWidth;
            float origH = Drawable.IntrinsicHeight;
            var transX = _m[Matrix.MtransX];
            var transY = _m[Matrix.MtransY];
            var finalX = ((x - transX) * origW) / GetImageWidth();
            var finalY = ((y - transY) * origH) / GetImageHeight();
            if (clipToBitmap)
            {
                finalX = Math.Min(Math.Max(x, 0f), origW);
                finalY = Math.Min(Math.Max(y, 0f), origH);
            }
            return new PointF(finalX, finalY);
        }

        //
        // Inverse of transformCoordTouchToBitmap. This function will transform the coordinates in the
        // drawable's coordinate system to the view's coordinate system.
        // @param bx x-coordinate in original bitmap coordinate system
        // @param by y-coordinate in original bitmap coordinate system
        // @return Coordinates of the point in the view's coordinate system.
        //
        private PointF TransformCoordBitmapToTouch(float bx, float by)
        {
            _matrix.GetValues(_m);
            float origW = Drawable.IntrinsicWidth;
            float origH = Drawable.IntrinsicHeight;
            var px = bx / origW;
            var py = by / origH;
            var finalX = _m[Matrix.MtransX] + GetImageWidth() * px;
            var finalY = _m[Matrix.MtransY] + GetImageHeight() * py;
            return new PointF(finalX, finalY);
        }

        //
        // Fling launches sequential runnables which apply
        // the fling graphic to the image. The values for the translation
        // are interpolated by the Scroller.
        // @author Ortiz
        //
        private class Fling : Java.Lang.Object, IRunnable
        {
            readonly ScaleImageView _view;
            Scroller _scroller;
            int _currX, _currY;

            public Fling(ScaleImageView view, int velocityX, int velocityY)
            {
                try
                {
                    _view = view;
                    view.SetState(TouchState.Fling);
                    _scroller = new Scroller(view._context);
                    view._matrix.GetValues(view._m);

                    var startX = (int)view._m[Matrix.MtransX];
                    var startY = (int)view._m[Matrix.MtransY];
                    int minX, maxX, minY, maxY;

                    if (view.GetImageWidth() > view.ViewWidth)
                    {
                        minX = view.ViewWidth - (int)view.GetImageWidth();
                        maxX = 0;
                    }
                    else
                    {
                        minX = maxX = startX;
                    }
                    if (view.GetImageHeight() > view.ViewHeight)
                    {
                        minY = view.ViewHeight - (int)view.GetImageHeight();
                        maxY = 0;
                    }
                    else
                    {
                        minY = maxY = startY;
                    }
                    _scroller.Fling(startX, startY, velocityX, velocityY, minX, maxX, minY, maxY);
                    _currX = startX;
                    _currY = startY;
                }
                catch (System.Exception ex)
                {
                    AppSettings.Logger.Warning(ex);
                }
            }

            public void CancelFling()
            {
                if (_scroller != null)
                {
                    _view.SetState(TouchState.None);
                    _scroller.ForceFinished(true);
                }
            }

            public void Run()
            {
                try
                {
                    if (_scroller.IsFinished)
                    {
                        _scroller = null;
                        return;
                    }
                    if (_scroller.ComputeScrollOffset())
                    {
                        var newX = _scroller.CurrX;
                        var newY = _scroller.CurrY;
                        var transX = newX - _currX;
                        var transY = newY - _currY;
                        _currX = newX;
                        _currY = newY;
                        _view._matrix.PostTranslate(transX, transY);
                        _view.FixTrans();
                        _view.ImageMatrix = _view._matrix;
                        _view.CompatPostOnAnimation(this);
                    }
                }
                catch (System.Exception ex)
                {
                    AppSettings.Logger.Warning(ex);
                }
            }
        }

        //class CompatScroller
        //{
        //    readonly Scroller _scroller;
        //    readonly OverScroller _overScroller;
        //    readonly bool _isPreGingerbread;

        //    public CompatScroller(Context context)
        //    {
        //        if (Build.VERSION.SdkInt < BuildVersionCodes.Gingerbread)
        //        {
        //            _isPreGingerbread = true;
        //            _scroller = new Scroller(context);
        //        }
        //        else
        //        {
        //            _isPreGingerbread = false;
        //            _overScroller = new OverScroller(context);
        //        }
        //    }

        //    public void Fling(int startX, int startY, int velocityX, int velocityY, int minX, int maxX, int minY, int maxY)
        //    {
        //        if (_isPreGingerbread)
        //        {
        //            _scroller.Fling(startX, startY, velocityX, velocityY, minX, maxX, minY, maxY);
        //        }
        //        else
        //        {
        //            _overScroller.Fling(startX, startY, velocityX, velocityY, minX, maxX, minY, maxY);
        //        }
        //    }

        //    public void ForceFinished(bool finished)
        //    {
        //        if (_isPreGingerbread)
        //        {
        //            _scroller.ForceFinished(finished);
        //        }
        //        else
        //        {
        //            _overScroller.ForceFinished(finished);
        //        }
        //    }

        //    public bool IsFinished()
        //    {
        //        if (_isPreGingerbread)
        //        {
        //            return _scroller.IsFinished;
        //        }
        //        return _overScroller.IsFinished;
        //    }

        //    public bool ComputeScrollOffset()
        //    {
        //        if (_isPreGingerbread)
        //        {
        //            return _scroller.ComputeScrollOffset();
        //        }
        //        return _overScroller.ComputeScrollOffset();
        //    }

        //    public int GetCurrX()
        //    {
        //        if (_isPreGingerbread)
        //        {
        //            return _scroller.CurrX;
        //        }
        //        return _overScroller.CurrX;
        //    }

        //    public int GetCurrY()
        //    {
        //        if (_isPreGingerbread)
        //        {
        //            return _scroller.CurrY;
        //        }
        //        return _overScroller.CurrY;
        //    }
        //}

        public void CompatPostOnAnimation(IRunnable runnable)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBean)
            {
                PostOnAnimation(runnable);
            }
            else
            {
                PostDelayed(runnable, 1000 / 60);
            }
        }

        private class ZoomVariables
        {
            public readonly float Scale;
            public readonly float FocusX;
            public readonly float FocusY;
            public readonly ScaleType ScaleType;

            public ZoomVariables(float scale, float focusX, float focusY, ScaleType scaleType)
            {
                Scale = scale;
                FocusX = focusX;
                FocusY = focusY;
                ScaleType = scaleType;
            }
        }

        //private void PrintMatrixInfo()
        //{
        //    _matrix.GetValues(_m);
        //}
    }
}