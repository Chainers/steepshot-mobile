using System;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Steemix.Droid
{
	#pragma warning disable CS0618 // Type or member is obsolete (class uses in pre-lollipop versions)
	public class CameraPreview : SurfaceView, ISurfaceHolderCallback
	{
		private Android.Hardware.Camera mCamera;

		private List<Android.Hardware.Camera.Size> mSupportedPreviewSizes;
		private Android.Hardware.Camera.Size mPreviewSize;

		public CameraPreview(Context context, Android.Hardware.Camera camera) : base(context)
		{
			this.mCamera = camera;
			Holder.AddCallback(this);

			mSupportedPreviewSizes = mCamera.GetParameters().SupportedPreviewSizes.ToList();
		}

		public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
		{
			if (holder.Surface == null)
				return;

			try
			{
				mCamera.StopPreview();
			}
			catch (Exception e)
			{
				Console.WriteLine("Error setting camera preview: " + e.Message);
			}

			try
			{
				var p = mCamera.GetParameters();
				switch (Display.Rotation)
				{
					case SurfaceOrientation.Rotation0:
						mCamera.SetDisplayOrientation(90);
						break;
					case SurfaceOrientation.Rotation90:
						break;
					case SurfaceOrientation.Rotation180:
						break;
					case SurfaceOrientation.Rotation270:
						mCamera.SetDisplayOrientation(180);
						break;
				}
				p.SetPreviewSize(mPreviewSize.Width, mPreviewSize.Height);
				mCamera.SetParameters(p);
				mCamera.SetPreviewDisplay(holder);
				mCamera.StartPreview();
			}
			catch (Exception e)
			{
				Console.WriteLine("Error setting camera preview: " + e.Message);
			}
		}

		public void SurfaceCreated(ISurfaceHolder holder)
		{
			try
			{
				mCamera.SetPreviewDisplay(holder);
				mCamera.StartPreview();
			}
			catch (Exception e)
			{
				Console.WriteLine("Error setting camera preview: " + e.Message);
			}
		}

		public void SurfaceDestroyed(ISurfaceHolder holder)
		{

		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			int width = ResolveSize(SuggestedMinimumWidth, widthMeasureSpec);
			int height = ResolveSize(SuggestedMinimumHeight, heightMeasureSpec);

			if (mSupportedPreviewSizes != null)
			{
				mPreviewSize = getOptimalPreviewSize(mSupportedPreviewSizes, width, height);
			}

			float ratio;
			if (mPreviewSize.Height >= mPreviewSize.Width)
				ratio = (float)mPreviewSize.Height / (float)mPreviewSize.Width;
			else
				ratio = (float)mPreviewSize.Width / (float)mPreviewSize.Height;

			// One of these methods should be used, second method squishes preview slightly
			SetMeasuredDimension(width, (int)(width * ratio));
		}

		private Android.Hardware.Camera.Size getOptimalPreviewSize(List<Android.Hardware.Camera.Size> sizes, int w, int h)
		{
			double ASPECT_TOLERANCE = 0.1;
			double targetRatio = (double)h / w;

			if (sizes == null)
				return null;

			Android.Hardware.Camera.Size optimalSize = null;
			double minDiff = double.MaxValue;

			int targetHeight = h;

			foreach (Android.Hardware.Camera.Size size in sizes)
			{
				double ratio = (double)size.Height / size.Width;
				if (Math.Abs(ratio - targetRatio) > ASPECT_TOLERANCE)
					continue;

				if (Math.Abs(size.Height - targetHeight) < minDiff)
				{
					optimalSize = size;
					minDiff = Math.Abs(size.Height - targetHeight);
				}
			}

			if (optimalSize == null)
			{
				minDiff = double.MaxValue;
				foreach (Android.Hardware.Camera.Size size in sizes)
				{
					if (Math.Abs(size.Height - targetHeight) < minDiff)
					{
						optimalSize = size;
						minDiff = Math.Abs(size.Height - targetHeight);
					}
				}
			}

			return optimalSize;
		}
	}
#pragma warning restore CS0618 // Type or member is obsolete
}
