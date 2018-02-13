using System;
using Android.Content;
using Android.Util;
using Android.Widget;

namespace Steepshot.Utils
{
	public sealed class TopCropScaleImageView : ImageView
	{
		public TopCropScaleImageView(Context c) : base(c)
		{
			Setup();
		}

		public TopCropScaleImageView(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			Setup();
		}

		public TopCropScaleImageView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
		{
			Setup();
		}

		private void Setup()
		{
			SetScaleType(ScaleType.Matrix);
		}

		protected override bool SetFrame(int l, int t, int r, int b)
		{
			if (Drawable == null)
				return false;
			
			float frameWidth = r - l;
			float frameHeight = b - t;
			var originalImageWidth = (float)Drawable.IntrinsicWidth;
			var originalImageHeight = (float)Drawable.IntrinsicHeight;

			float usedScaleFactor = 1;

			if ((frameWidth > originalImageWidth) || (frameHeight > originalImageHeight))
			{
				var fitHorizontallyScaleFactor = frameWidth / originalImageWidth;
				var fitVerticallyScaleFactor = frameHeight / originalImageHeight;

				usedScaleFactor = Math.Max(fitHorizontallyScaleFactor, fitVerticallyScaleFactor);
			}

			var newImageWidth = originalImageWidth * usedScaleFactor;
			var newImageHeight = originalImageHeight * usedScaleFactor;

			var matrix = ImageMatrix;
			matrix.SetScale(usedScaleFactor, usedScaleFactor, 0, 0); // Replaces the old matrix completly

			matrix.PostTranslate((frameWidth - newImageWidth) / 2, frameHeight - newImageHeight);
			ImageMatrix.Set(matrix);
			return base.SetFrame(l, t, r, b);
		}
	}
}
