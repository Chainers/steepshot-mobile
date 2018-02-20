using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Graphics;

namespace Steepshot.VideoPlayerManager
{
    public abstract class ScalableTextureView : TextureView
    {
        private int _contentWidth = -1;
        private int _contentHeight = -1;

        private float _pivotX;
        private float _pivotY;

        private float _contentScaleX = 1f;
        private float _contentScaleY = 1f;

        private float _contentRotation;

        private float _contentScaleMultiplier = 1f;

        private int _contentX;
        private int _contentY;

        private readonly Matrix _transformMatrix = new Matrix();


        public ScaleType ScaleType { get; set; }

        public override float Rotation
        {
            get => _contentRotation;
            set
            {
                _contentRotation = value;
                UpdateMatrixScaleRotate();
            }
        }

        public override float PivotX
        {
            get => _pivotX;
            set => _pivotX = value;
        }

        public override float PivotY
        {
            get => _pivotY;
            set => _pivotY = value;
        }

        
        public float ContentAspectRatio => _contentWidth != -1 && _contentHeight != -1 ? (float)_contentWidth / _contentHeight : 0;

        /// <summary>
        /// Use it to animate TextureView content x position
        /// </summary>
        public int ContentX
        {
            get => _contentX;
            set
            {
                _contentX = value - (MeasuredWidth - ScaledContentWidth) / 2;
                UpdateMatrixTranslate();
            }
        }

        /// <summary>
        /// Use it to animate TextureView content y position
        /// </summary>
        public int ContentY
        {
            get => _contentY;
            set
            {
                _contentY = value - (MeasuredHeight - ScaledContentHeight) / 2;
                UpdateMatrixTranslate();
            }
        }


        public int ScaledContentWidth => (int)(_contentScaleX * _contentScaleMultiplier * MeasuredWidth);

        public int ScaledContentHeight => (int)(_contentScaleY * _contentScaleMultiplier * MeasuredHeight);

        public int ContentHeight
        {
            get => _contentHeight;
            set => _contentHeight = value;
        }

        public int ContentWidth
        {
            get => _contentWidth;
            set => _contentWidth = value;
        }

        public float ContentScale
        {
            get => _contentScaleMultiplier;
            set
            {
                _contentScaleMultiplier = value;
                UpdateMatrixScaleRotate();
            }
        }


        #region Contructors

        protected ScalableTextureView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        protected ScalableTextureView(Context context) : base(context)
        {
        }

        protected ScalableTextureView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        protected ScalableTextureView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        protected ScalableTextureView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }

        #endregion Contructors


        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            UpdateTextureViewSize();
        }

        public void UpdateTextureViewSize()
        {
            if (_contentWidth == -1 || _contentHeight == -1)
                return;

            float viewWidth = MeasuredWidth;
            float viewHeight = MeasuredHeight;
            SetScale(viewWidth, viewHeight);
            SetPivot(viewWidth, viewHeight);

            UpdateMatrixScaleRotate();
        }

        private void SetScale(float viewWidth, float viewHeight)
        {
            float contentWidth = _contentWidth;
            float contentHeight = _contentHeight;

            float scaleX = 1.0f;
            float scaleY = 1.0f;

            float fitCoef = 1;

            switch (ScaleType)
            {
                case ScaleType.Fill:
                    if (viewWidth > viewHeight)
                    {   // device in landscape
                        scaleX = (viewHeight * contentWidth) / (viewWidth * contentHeight);
                    }
                    else
                    {
                        scaleY = (viewWidth * contentHeight) / (viewHeight * contentWidth);
                    }
                    break;
                case ScaleType.Bottom:
                case ScaleType.CenterCrop:
                case ScaleType.Top:
                    if (contentWidth > viewWidth && contentHeight > viewHeight)
                    {
                        scaleX = contentWidth / viewWidth;
                        scaleY = contentHeight / viewHeight;
                    }
                    else if (contentWidth < viewWidth && contentHeight < viewHeight)
                    {
                        scaleY = viewWidth / contentWidth;
                        scaleX = viewHeight / contentHeight;
                    }
                    else if (viewWidth > contentWidth)
                    {
                        scaleY = (viewWidth / contentWidth) / (viewHeight / contentHeight);
                    }
                    else if (viewHeight > contentHeight)
                    {
                        scaleX = (viewHeight / contentHeight) / (viewWidth / contentWidth);
                    }

                    if (_contentHeight > _contentWidth)
                    { //Portrait video
                        fitCoef = viewWidth / (viewWidth * scaleX);
                    }
                    else
                    { //Landscape video
                        fitCoef = viewHeight / (viewHeight * scaleY);
                    }

                    break;
            }

            _contentScaleX = fitCoef * scaleX;
            _contentScaleY = fitCoef * scaleY;
        }

        /// <summary>
        /// Calculate pivot points, in our case crop from center
        /// </summary>
        private void SetPivot(float viewWidth, float viewHeight)
        {
            float pivotPointX;
            float pivotPointY;

            switch (ScaleType)
            {
                case ScaleType.Top:
                    pivotPointX = 0;
                    pivotPointY = 0;
                    break;
                case ScaleType.Bottom:
                    pivotPointX = viewWidth;
                    pivotPointY = viewHeight;
                    break;
                case ScaleType.CenterCrop:
                    pivotPointX = viewWidth / 2;
                    pivotPointY = viewHeight / 2;
                    break;
                case ScaleType.Fill:
                    pivotPointX = _pivotX;
                    pivotPointY = _pivotY;
                    break;
                default:
                    throw new ArgumentException($"pivotPointX, pivotPointY for ScaleType {ScaleType} are not defined");
            }

            _pivotX = pivotPointX;
            _pivotY = pivotPointY;

        }

        private void UpdateMatrixScaleRotate()
        {
            _transformMatrix.Reset();
            _transformMatrix.SetScale(_contentScaleX * _contentScaleMultiplier, _contentScaleY * _contentScaleMultiplier, _pivotX, _pivotY);
            _transformMatrix.PostRotate(_contentRotation, _pivotX, _pivotY);
            SetTransform(_transformMatrix);
        }

        /// <summary>
        ///  Use it to set content of a TextureView in the center of TextureView
        /// </summary>
        public void CentralizeContent()
        {
            _contentX = 0;
            _contentY = 0;

            UpdateMatrixScaleRotate();
        }

        private void UpdateMatrixTranslate()
        {
            float scaleX = _contentScaleX * _contentScaleMultiplier;
            float scaleY = _contentScaleY * _contentScaleMultiplier;

            _transformMatrix.Reset();
            _transformMatrix.SetScale(scaleX, scaleY, _pivotX, _pivotY);
            _transformMatrix.PostTranslate(_contentX, _contentY);
            SetTransform(_transformMatrix);
        }
    }
}