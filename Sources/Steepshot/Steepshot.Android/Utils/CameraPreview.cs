using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Steepshot.Core;

namespace Steepshot.Utils
{
    //TODO:KOA - In the development. Needed for replace PhotoFragment
#pragma warning disable CS0618 // Type or member is obsolete (class uses in pre-lollipop versions)
    public class CameraPreview : SurfaceView, ISurfaceHolderCallback, Android.Hardware.Camera.IShutterCallback, Android.Hardware.Camera.IPictureCallback
    {
        private readonly Android.Hardware.Camera _mCamera;
        public event EventHandler<string> PictureTaken;

        private readonly List<Android.Hardware.Camera.Size> _mSupportedPreviewSizes;
        private readonly List<Android.Hardware.Camera.Size> _mSupportedPictureSizes;
        private Android.Hardware.Camera.Size _mPreviewSize, _mPictureSize;

        public CameraPreview(Context context, Android.Hardware.Camera camera) : base(context)
        {
            _mCamera = camera;
            Holder.AddCallback(this);

            _mSupportedPreviewSizes = _mCamera.GetParameters().SupportedPreviewSizes.ToList();
            _mSupportedPictureSizes = _mCamera.GetParameters().SupportedPictureSizes.ToList();
            Holder.SetType(SurfaceType.PushBuffers);
        }

        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {
            if (holder.Surface == null)
                return;

            try
            {
                _mCamera.StopPreview();
            }
            catch (Exception e)
            {
                //TODO:KOA: remove Console!
                Console.WriteLine(Localization.Errors.ErrorCameraPreview + e.Message);
            }

            try
            {

                var p = _mCamera.GetParameters();
                p.SetPreviewSize(_mPreviewSize.Width, _mPreviewSize.Height);
                if (p.SupportedFocusModes.Contains(Android.Hardware.Camera.Parameters.FocusModeContinuousVideo))
                {
                    p.FocusMode = Android.Hardware.Camera.Parameters.FocusModeContinuousVideo;
                    _mCamera.SetParameters(p);
                }
                switch (Display.Rotation)
                {
                    case SurfaceOrientation.Rotation0:
                        _mCamera.SetDisplayOrientation(90);
                        break;
                    case SurfaceOrientation.Rotation90:
                        break;
                    case SurfaceOrientation.Rotation180:
                        break;
                    case SurfaceOrientation.Rotation270:
                        _mCamera.SetDisplayOrientation(180);
                        break;
                }
                p.SetPreviewSize(_mPreviewSize.Width, _mPreviewSize.Height);

                if (_mPictureSize != null)
                {
                    p.SetPictureSize(_mPictureSize.Width, _mPictureSize.Height);
                }
                p.SetRotation(90);
                _mCamera.SetParameters(p);
                _mCamera.SetPreviewDisplay(holder);
                _mCamera.StartPreview();
            }
            catch (Exception e)
            {
                //TODO:KOA: remove Console!
                Console.WriteLine(Localization.Errors.ErrorCameraPreview + e.Message);
            }
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            try
            {
                _mCamera.SetPreviewDisplay(holder);
                _mCamera.StartPreview();
            }
            catch (Exception e)
            {
                //TODO:KOA: remove Console!
                Console.WriteLine(Localization.Errors.ErrorCameraPreview + e.Message);
            }
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {

        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            var width = ResolveSize(SuggestedMinimumWidth, widthMeasureSpec);
            var height = ResolveSize(SuggestedMinimumHeight, heightMeasureSpec);

            if (_mSupportedPreviewSizes != null)
            {
                _mPreviewSize = GetOptimalPreviewSize(_mSupportedPreviewSizes, width, height);
            }

            float ratio;
            if (_mPreviewSize.Height >= _mPreviewSize.Width)
                ratio = _mPreviewSize.Height / (float)_mPreviewSize.Width;
            else
                ratio = _mPreviewSize.Width / (float)_mPreviewSize.Height;

            // One of these methods should be used, second method squishes preview slightly
            SetMeasuredDimension(width, (int)(width * ratio));
            InitPictureSize();
        }

        private void InitPictureSize()
        {
            if (_mSupportedPictureSizes != null)
            {
                for (var i = _mSupportedPictureSizes.Count - 1; i >= 0; i--)
                {
                    if (_mSupportedPictureSizes[i].Width > Width)
                        _mPictureSize = _mSupportedPictureSizes[i];
                }
            }
        }

        private Android.Hardware.Camera.Size GetOptimalPreviewSize(List<Android.Hardware.Camera.Size> sizes, int w, int h)
        {
            var aspectTolerance = 0.1;
            var targetRatio = (double)h / w;

            if (sizes == null)
                return null;

            Android.Hardware.Camera.Size optimalSize = null;
            var minDiff = double.MaxValue;

            var targetHeight = h;

            foreach (var size in sizes)
            {
                var ratio = (double)size.Height / size.Width;
                if (Math.Abs(ratio - targetRatio) > aspectTolerance)
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
                foreach (var size in sizes)
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
            var dir = new Java.IO.File(
                Android.OS.Environment.GetExternalStoragePublicDirectory(
                    Android.OS.Environment.DirectoryPictures), "Steepshot");
            if (!dir.Exists())
            {
                dir.Mkdirs();
            }

            return dir;
        }

        string ExportBitmapAsPng(byte[] data)
        {
            var pP = GetDirectoryForPictures().AbsolutePath;
            var filePath = System.IO.Path.Combine(pP, $"{Guid.NewGuid()}.jpg");
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
                PictureTaken?.Invoke(this, ExportBitmapAsPng(data));
                Toast.MakeText(Context, "PICTURE TAKEN SUCESSFULLY", ToastLength.Long).Show();
            }
            catch (Exception e)
            {
                Toast.MakeText(Context, e.Message, ToastLength.Long).Show();
            }
            try
            {
                _mCamera?.StartPreview();
            }
            catch { }
        }

        public void OnShutter()
        {
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}

