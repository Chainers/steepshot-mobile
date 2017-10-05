using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Hardware;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Lilarcor.Cheeseknife;
using Steepshot.Activity;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Utils;
using Steepshot.Utils;
using Camera = Android.Hardware.Camera;

namespace Steepshot.Fragment
{
#pragma warning disable 0649, 4014, 0618
    public class OldCameraFragment : BaseFragment, ISurfaceHolderCallback, Camera.IPictureCallback
    {
        private ISurfaceHolder _holder;
        private Camera _camera;
        private int _cameraId = 0;
        private const bool _fullScreen = false;
        private const int galleryRequestCode = 228;
        private CameraOrientationEventListener _orientationListner;

        [InjectView(Resource.Id.surfaceView)] private SurfaceView _sv;
        [InjectView(Resource.Id.flash_button)] private ImageButton _flashButton;
        [InjectView(Resource.Id.shot_button)] private ImageButton _shotButton;
        [InjectView(Resource.Id.revert_button)] private ImageButton _revertButton;
#pragma warning restore 0649

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var v = inflater.Inflate(Resource.Layout.lyt_old_camera, null);
            Cheeseknife.Inject(this, v);
            return v;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            _orientationListner = new CameraOrientationEventListener(Activity, SensorDelay.Normal);
            _holder = _sv.Holder;
            _holder.SetType(SurfaceType.PushBuffers);
            _holder.AddCallback(this);
        }

        public override void OnResume()
        {
            base.OnResume();
            _camera = Camera.Open(_cameraId);
            SetPreviewSize(_fullScreen);
            _orientationListner.Enable();
        }

        public override void OnPause()
        {
            base.OnPause();
            if (_camera != null)
                _camera.Release();
            _camera = null;
            _orientationListner.Disable();
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (resultCode == -1 && requestCode == galleryRequestCode)
            {
                var i = new Intent(Context, typeof(PostDescriptionActivity));
                i.PutExtra("FILEPATH", GetPathToImage(data.Data));
                StartActivity(i);
            }
        }

        [InjectOnClick(Resource.Id.shot_button)]
        private void TakePhotoClick(object sender, EventArgs e)
        {
            _camera?.TakePicture(null, null, this);
        }

        [InjectOnClick(Resource.Id.close_button)]
        private void GoBack(object sender, EventArgs e)
        {
            Activity.OnBackPressed();
        }

        [InjectOnClick(Resource.Id.gallery_button)]
        private void OpenGallery(object sender, EventArgs e)
        {
            Intent intent = new Intent();
            intent.SetAction(Intent.ActionGetContent);
            intent.SetType("image/*");
            StartActivityForResult(intent, galleryRequestCode);
        }

        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {
            if (_camera == null)
                return;
            _camera.StopPreview();
            SetCameraDisplayOrientation(_cameraId);
            try
            {
                _camera.SetPreviewDisplay(holder);
                _camera.StartPreview();
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            if (_camera == null)
                return;
            try
            {
                _camera.SetPreviewDisplay(holder);
                _camera.StartPreview();
            }
            catch (Java.IO.IOException ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
        }

        private void SetPreviewSize(bool fullScreen)
        {
            Display display = Activity.WindowManager.DefaultDisplay;
            bool widthIsMax = display.Width > display.Height;

            var parameters = _camera.GetParameters();
            var cameraSizes = GetSizes(parameters.SupportedPreviewSizes, parameters.SupportedPictureSizes);           

            parameters.SetPictureSize(cameraSizes.Item2.Width, cameraSizes.Item2.Height);
            parameters.SetPreviewSize(cameraSizes.Item1.Width, cameraSizes.Item1.Height);
            _camera.SetParameters(parameters);
            var size = _camera.GetParameters().PreviewSize;

            RectF rectDisplay = new RectF();
            RectF rectPreview = new RectF();

            rectDisplay.Set(0, 0, display.Width, display.Height);

            if (widthIsMax)
                rectPreview.Set(0, 0, size.Width, size.Height);
            else
                rectPreview.Set(0, 0, size.Height, size.Width);

            Matrix matrix = new Matrix();
            if (!fullScreen)
                matrix.SetRectToRect(rectPreview, rectDisplay, Matrix.ScaleToFit.Start);
            else
            {
                matrix.SetRectToRect(rectDisplay, rectPreview, Matrix.ScaleToFit.Start);
                matrix.Invert(matrix);
            }
            matrix.MapRect(rectPreview);
            _sv.LayoutParameters.Height = (int)(rectPreview.Bottom);
            _sv.LayoutParameters.Width = (int)(rectPreview.Right);
        }

        private Tuple<Camera.Size, Camera.Size> GetSizes(IList<Camera.Size> supportedPreviewSizes,IList<Camera.Size> supportedPictureSizes)
        {
            var previewSizes = supportedPreviewSizes.OrderByDescending((arg) => arg.Width).ToList();
            var pictureSizes = supportedPictureSizes.OrderByDescending((arg) => arg.Width).ToList();

            foreach (var previewSize in previewSizes)
            {
                var previewCoeff = previewSize.Height / previewSize.Width;
                foreach (var pictureSize in pictureSizes)
                {
                    if (pictureSize.Width > previewSize.Width)
                        continue;

                    if (Math.Abs((previewSize.Height / pictureSize.Height) - (previewSize.Width / pictureSize.Width)) < 0.001)
                    {
                        return new Tuple<Camera.Size, Camera.Size>(previewSize, pictureSize);
                    }
                }
                return new Tuple<Camera.Size, Camera.Size>(previewSizes[0], pictureSizes[0]);
            }
            return new Tuple<Camera.Size, Camera.Size>(previewSizes[0], pictureSizes[0]);
        }

        private void SetCameraDisplayOrientation(int cameraId)
        {
            var rotation = Activity.WindowManager.DefaultDisplay.Rotation;
            int degrees = 0;
            switch (rotation)
            {
                case SurfaceOrientation.Rotation0:
                    degrees = 0;
                    break;
                case SurfaceOrientation.Rotation90:
                    degrees = 90;
                    break;
                case SurfaceOrientation.Rotation180:
                    degrees = 180;
                    break;
                case SurfaceOrientation.Rotation270:
                    degrees = 270;
                    break;
            }

            int result = 0;
            Camera.CameraInfo info = new Camera.CameraInfo();
            Camera.GetCameraInfo(cameraId, info);
            if (info.Facing == CameraFacing.Back)
            {
                result = ((360 - degrees) + info.Orientation);
            }
            else if (info.Facing == CameraFacing.Front)
            {
                result = ((360 - degrees) - info.Orientation);
                result += 360;
            }
            result = result % 360;
            _camera.SetDisplayOrientation(result);

            var parameters = _camera.GetParameters();
            parameters.PictureFormat = ImageFormat.Jpeg;
            parameters.JpegQuality = 90;

            if (parameters.SupportedFocusModes.Contains(Camera.Parameters.FocusModeContinuousVideo))
            {
                parameters.FocusMode = Camera.Parameters.FocusModeContinuousVideo;
            }
            _camera.SetParameters(parameters);

        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {

        }

        public void OnPictureTaken(byte[] data, Camera camera)
        {
            var directoryPictures = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures);
            var directory = new Java.IO.File(directoryPictures, Constants.Steepshot);
            if (!directory.Exists())
                directory.Mkdirs();

            var _photoUri = $"{directory}/{Guid.NewGuid()}.jpeg";

            var options = new BitmapFactory.Options { InJustDecodeBounds = true };
            var bitmap = BitmapFactory.DecodeByteArray(data, 0, data.Length, options);

            options.InSampleSize = BitmapUtils.CalculateInSampleSize(options, 1600, 1600);
            options.InJustDecodeBounds = false;

            bitmap = BitmapFactory.DecodeByteArray(data, 0, data.Length, options);
            bitmap = BitmapUtils.RotateImage(bitmap, _orientationListner.Orientation);
            var stream = new FileStream(_photoUri, FileMode.Create);
            bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);

            var i = new Intent(Context, typeof(PostDescriptionActivity));
            i.PutExtra("FILEPATH", _photoUri);
            i.PutExtra("SHOULD_COMPRESS", false);
            StartActivity(i);
        }

        private string GetPathToImage(Android.Net.Uri uri)
        {
            string doc_id = "";
            using (var c1 = Activity.ContentResolver.Query(uri, null, null, null, null))
            {
                c1.MoveToFirst();
                String document_id = c1.GetString(0);
                doc_id = document_id.Substring(document_id.LastIndexOf(":") + 1);
            }
            string path = null;
            string selection = MediaStore.Images.Media.InterfaceConsts.Id + " =? ";
            using (var cursor = Activity.ManagedQuery(MediaStore.Images.Media.ExternalContentUri, null, selection, new string[] { doc_id }, null))
            {
                if (cursor == null) return path;
                var columnIndex = cursor.GetColumnIndexOrThrow(MediaStore.Images.Media.InterfaceConsts.Data);
                cursor.MoveToFirst();
                path = cursor.GetString(columnIndex);
            }
            return path;
        }
    }
}
