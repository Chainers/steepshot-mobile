using System;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Graphics;
using System.Collections.Generic;
using System.Linq;
using Android.Widget;
using Java.IO;
using System.IO;

namespace Steemix.Droid
{
	#pragma warning disable CS0618 // Type or member is obsolete (class uses in pre-lollipop versions)
	public class CameraPreview : SurfaceView, ISurfaceHolderCallback, Android.Hardware.Camera.IShutterCallback, Android.Hardware.Camera.IPictureCallback
	{
		private Android.Hardware.Camera mCamera;
		public event EventHandler<string> PictureTaken;

		private List<Android.Hardware.Camera.Size> mSupportedPreviewSizes;
		private List<Android.Hardware.Camera.Size> mSupportedPictureSizes;
		private Android.Hardware.Camera.Size mPreviewSize,mPictureSize;

		public CameraPreview(Context context, Android.Hardware.Camera camera) : base(context)
		{
			this.mCamera = camera;
			Holder.AddCallback(this);

			mSupportedPreviewSizes = mCamera.GetParameters().SupportedPreviewSizes.ToList();
			mSupportedPictureSizes = mCamera.GetParameters().SupportedPictureSizes.ToList();
			Holder.SetType(SurfaceType.PushBuffers);
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
				System.Console.WriteLine("Error setting camera preview: " + e.Message);
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

				if (mPictureSize != null)
				{
					p.SetPictureSize(mPictureSize.Width, mPictureSize.Height);
				}
				p.SetRotation(90);
				mCamera.SetParameters(p);
				mCamera.SetPreviewDisplay(holder);
				mCamera.StartPreview();
			}
			catch (Exception e)
			{
				System.Console.WriteLine("Error setting camera preview: " + e.Message);
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
				System.Console.WriteLine("Error setting camera preview: " + e.Message);
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
			InitPictureSize();
		}

		private void InitPictureSize()
		{
			if (mSupportedPictureSizes != null)
			{
				for (int i = mSupportedPictureSizes.Count-1; i >=0; i--)
				{
					if (mSupportedPictureSizes[i].Width > Width)
						mPictureSize = mSupportedPictureSizes[i];
				}
			}
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

		private Java.IO.File GetDirectoryForPictures()
		{
			var _dir = new Java.IO.File(
				Android.OS.Environment.GetExternalStoragePublicDirectory(
					Android.OS.Environment.DirectoryPictures), "SteepShot");
			if (!_dir.Exists())
			{
				_dir.Mkdirs();
			}

			return _dir;
		}

		string ExportBitmapAsPNG(byte[] data)
		{
			var pP = GetDirectoryForPictures().AbsolutePath;
			string filePath = System.IO.Path.Combine(pP, string.Format("{0}.jpg", DateTime.Now));
			var stream = new FileStream(filePath, FileMode.Create);
			stream.Write(data, 0, data.Length);
			stream.Flush();
			stream.Close();
			return filePath;
		}

		public void OnPictureTaken(byte[] data, Android.Hardware.Camera camera)
		{
			try
			{
				PictureTaken?.Invoke(this, ExportBitmapAsPNG(data));
			}
			catch (Exception e)
			{
				System.Console.WriteLine(e.Message);
			}
			try
			{
				mCamera?.StartPreview();
			}
			catch { }
		}

		public void OnShutter()
		{
		}
	}
#pragma warning restore CS0618 // Type or member is obsolete
}
